# 角色模块核心 API

> **适用场景**：读写角色控制器代码、搭建状态节点/状态机配置、接意图供给源（玩家/AI）、排查动画/运动/时序问题
> **模块边界**：外部只允许引 `SPCharacter.Contract` + `SPCharacter.Wiring`，禁止引 `SPCharacter.Core` / `SPCharacter.Core.Editor`

## 模块总览

角色模块 = 意图驱动 + 数据驱动状态机 + 动画/运动落位，呈三层结构：

```
意图源 (ICharacterIntentionProvider)
   │  CharacterIntentionFrame（每帧快照）
   ▼
IntentionProcessor → 写入 CharacterRunTimeData（黑板）
   ▼
StateMachine（按转移规则切换状态）→ 发布 CurrentStateId / StateVersion
   ▼
AnimationDriver（Animancer 播放）  MotionDriver（转向 + 根运动位移）
```

- **Root MonoBehaviour**：`SPCC`（`[DefaultExecutionOrder(-300)]`），只做装配与时序分发，不含游戏逻辑
- **时序**（SPCC 内硬编码，勿改动顺序）：
  1. `Update`：写意图 → 状态机 LogicUpdate → 动画指令下发 → 黑板意图复位
  2. `OnAnimatorMove`：空实现（`applyRootMotion = false`，阻断默认根运动）
  3. `LateUpdate`：动画进度回写黑板 → 旋转更新 → 位移更新

---

## 一、Contract 层（外部可用）

### 原子意图：CharacterIntention（位掩码）

```csharp
namespace SPCharacter.Contract
{
    [Flags]
    public enum CharacterIntention : uint
    {
        None = 0,
        AnimationCompleted = 1 << 0,  // 非循环动画已播完
        WantToMove        = 1 << 1,  // 存在移动输入（含归零缓冲期）
        WantToAttack      = 1 << 2,  // 攻击键按下边沿
        WantToHoldAttack  = 1 << 3,  // 攻击键长按（持续按压超过阈值）
        WantToEvade       = 1 << 4,  // 闪避键按下边沿
        WantToTurn        = 1 << 5,  // 输入方向相对上一帧发生大角度转向（> 阈值）
    }
}
```

### 意图快照：CharacterIntentionFrame

```csharp
namespace SPCharacter.Contract
{
    public struct CharacterIntentionFrame
    {
        public Vector2 MoveAxis { get; init; }  // 世界 XZ 目标方向（XY 分量对应世界 XZ 轴）
        public CharacterIntention Flags { get; init; }  // 本帧原子意图位掩码
    }
}
```

**注意**：`MoveAxis` 已是世界 XZ 目标方向（由相机坐标转换器换算），不是输入平面方向。

### 意图供给：ICharacterIntentionProvider / CharacterIntentionProviderAsset

```csharp
namespace SPCharacter.Contract
{
    public interface ICharacterIntentionProvider
    {
        CharacterIntentionFrame CurrentFrame { get; }   // 当前帧意图快照
    }

    // 便于 SPCC 在 Inspector 序列化引用：新建 SO 资产继承此类
    public abstract class CharacterIntentionProviderAsset : ScriptableObject, ICharacterIntentionProvider
    {
        public abstract CharacterIntentionFrame CurrentFrame { get; }
    }
}
```

### 命名空间引用

```csharp
using SPCharacter.Contract;   // CharacterIntention, CharacterIntentionFrame, ICharacterIntentionProvider, CharacterIntentionProviderAsset
using SPCharacter.Wiring;     // InputTranslator
```

---

## 二、Wiring 层（外部可用）

### InputTranslator（玩家意图翻译机）

把输入模块的后处理数据翻译为角色意图快照。它是 `CharacterIntentionProviderAsset` 的 SO 实现，直接挂到 SPCC 的意图注入槽位。

```csharp
namespace SPCharacter.Wiring
{
    [CreateAssetMenu(menuName = "SPCharacter/Input Translator", fileName = "PlayerInputTranslator")]
    public class InputTranslator : CharacterIntentionProviderAsset
    {
        // 序列化字段（Inspector 接线）
        // _frameInput          : FrameInputProviderSO（输入模块帧输入槽位，必填，未配置抛异常）
        // _coordinateConverter : CoordinateConverterProviderSO（相机坐标转换器；未配置时直通输入方向）
        // _turnAngleThreshold  : [0,180]°，默认 135°，相邻有效输入方向夹角严格大于此值产生 WantToTurn
    }
}
```

- `WantToAttack` = `input.Attack.IsPressed`（按下边沿）
- `WantToHoldAttack` = `input.Attack.IsHeld`（长按，持续按压超过阈值）
- `WantToEvade` = `input.Evade.IsPressed`
- `WantToMove` = `input.HasMoveInput`（含归零缓冲期）
- `WantToTurn` = 相邻两帧有效输入方向夹角 > `_turnAngleThreshold`
- `MoveAxis` = 经 `ICoordinateConverter.ConvertToWorldMoveDirection` 换算后的世界方向；转换器为 null 时直通输入方向

**行为细节**：
- 按 `FrameIndex` 缓存快照：同一帧重复读取 `CurrentFrame` 返回缓存，不重复计算
- 无 provider / provider 为 null 时静默返回 `default`（不抛异常）
- `_frameInput` 槽位 SO 未配置时抛 `InvalidOperationException` 拒绝运行

**依赖的跨模块 Contract**：
```csharp
using SPInput.Contract;    // ProcessedFrameInput, IFrameInputProvider
using SPInput.Wiring;      // FrameInputProviderSO
using SPCamera.Contract;   // ICoordinateConverter
using SPCamera.Wiring;     // CoordinateConverterProviderSO
```

---

## 三、Core 层（外部禁引，仅供了解）

### SPCC（角色控制器 Root）

- `[DefaultExecutionOrder(-300)]`，`[RequireComponent(Animator + AnimancerComponent)]`
- Inspector 必配：`_animator`、`_animancer`、`_config`（CharacterStateConfigSO）、`_directProvider`（意图供给 SO）
- `Awake` 强校验：Animancer/Config/Animator 任一缺失抛异常；关闭 `applyRootMotion`
- 每帧时序见模块总览，改动子系统的装配/顺序即改此处

### CharacterRunTimeData（黑板）

角色内部持有的数据交换中心，外部无修改入口（写入方法均为 `internal`）。

| 读取属性 | 含义 |
|---------|------|
| `MoveInput` | 本帧移动目标方向（世界 XZ） |
| `CurrentStateId` | 当前状态节点 Id |
| `StateVersion` | 状态版本，每次切到不同节点递增；动画/运动层据此检测状态变化 |
| `AnimationTime` / `AnimationNormalizedTime` | 当前动画时刻 / 归一化进度 |
| `AnimationEntryNormalizedTime` | 状态动画开始播放时的归一化进度（运动层首帧采样基线） |
| `PendingCompletionRotationDegrees` | 等待运动层消费的状态结束相对 Y 轴旋转补偿（度） |
| `EvaluateCondition(StateTransitionCondition)` | 按 Required/Forbidden 位掩码评估当前意图是否满足转移条件 |

**意图语义**：本帧意图由 SPCC 在 `Update` 末尾调用 `ResetIntentions()` 清空；转移条件（如 `AnimationCompleted`）都是“本帧有效”。

### StateMachine（状态机）

- 构造时从 `CharacterStateConfigSO` 构建两个哈希表：`_nodesById`、`_rulesByFromId`（转移规则按来源 Id 分桶）
- 构造强校验：节点/规则非空、节点 Id 不重复、规则 From/To 均存在、入口节点存在
- `LogicUpdate()`：只遍历当前状态的出边规则，规则命中顺序 = 配置数组顺序
- 转移条件（`TryTransitionRule`）：
  1. `_blackboard.EvaluateCondition(rule.Condition)` 意图匹配
  2. `_blackboard.AnimationNormalizedTime >= rule.InterruptPoint` 动画打断点已到
- 转移时发布上一节点的 `CompletionRotationDegrees`（由 MotionDriver 消费），再 `PublishState` 新状态
- 自环规则（`ToId == FromId`）会命中但不会实际切换（`TransitionToNode` 直接 return）

### StateNodeSO（状态节点，纯数据）

`[CreateAssetMenu(menuName = "SPCharacter/StateLogic/StateNode")]`

| 字段 | 含义 |
|------|------|
| `Id` | 状态唯一标识，同一 Config 内唯一，供转移规则引用 |
| `Animation` | `SPAnimClip`（Animancer Transition Asset 引用） |
| `IsLooping` | 循环状态不产生动画完成意图 |
| `RootMotionProfile` | 离线烘焙根运动位移曲线资产；留空 = 无根运动位移 |
| `TurnSpeedDegreesPerSecond` | 每秒最大转向角度（度/秒）；0 = 不主动旋转，默认 720 |
| `CompletionRotationDegrees` | 状态动画完成时一次性施加的相对 Y 轴旋转补偿（度） |

### CharacterStateConfigSO（状态配置资产）

`[CreateAssetMenu(menuName = "SPCharacter/StateLogic/CharacterStateConfig")]`

| 字段 | 含义 |
|------|------|
| `EntryId` | 状态机入口节点 Id（对应 StateNodeSO.Id） |
| `Nodes` | 所有状态节点（StateNodeSO 数组） |
| `Rules` | 状态间转移规则（StateTransitionRule 数组，可由 TransitionTableImporter 从 Excel 生成） |

### 转移规则与条件（struct）

```csharp
public struct StateTransitionRule
{
    public string FromId;                 // 来源节点 Id
    public string ToId;                   // 目标节点 Id
    public StateTransitionCondition Condition;  // 触发条件
    public float InterruptPoint;          // [0,1]，来源动画归一化进度达到后才允许转移；0 = 立即
}

public struct StateTransitionCondition
{
    public CharacterIntention Required;   // 必须全部为 1 的意图位（None = 不要求）
    public CharacterIntention Forbidden;  // 必须全部为 0 的意图位（None = 不禁止）
}
```

**条件判定语义**（黑板 `EvaluateCondition`）：
- `Required` 中的位必须全部为 1
- `Forbidden` 中的位必须全部为 0
- 未出现在任一组中的位视为“自由”，不影响判定

### IntentionProcessor（意图后处理）

`Process(in CharacterIntentionFrame frame)`：把快照的 `MoveAxis` 写入黑板，并按位拆分写入各控制意图。意图合成等后处理逻辑在此扩展。

### AnimationDriver + AnimationSource（动画层）

- `AnimationDriver.LogicUpdate()`：监听 `StateVersion` 变化，按黑板当前状态 Id 反查节点，`AnimationSource.Play(node.Animation)` 播放 Animancer Transition，记录入口进度基线
- `AnimationDriver.SyncAnimProgress()`（LateUpdate）：回写 `AnimationTime` / `AnimationNormalizedTime`；非循环动画归一化进度 ≥ 1 时报告 `AnimationCompleted`（每状态只报一次）
- 循环动画归一化时间用 `Floor` 取小数部分，避免数值溢出到 ≥ 1
- `AnimationSource`：唯一对接 Animancer 的指令源，便于更换动画系统；`SPAnimClip` 是 `TransitionAssetBase` 的隔离包装

### MotionDriver（运动层）

- `PositionUpdate()`：按 `AnimationNormalizedTime` 采样 `RootMotionProfileSO` 的 LocalX/LocalZ 曲线（本地累计位移），当前采样减上一帧采样得到本帧增量，旋转到世界空间后落位；循环状态跨 0 点时按 `end - prev + current - start` 补回
- `RotationUpdate()`：先消费黑板 `PendingCompletionRotationDegrees`（一次性旋转补偿），再按 `MoveInput` 方向以 `TurnSpeedDegreesPerSecond` 向目标 yaw 平滑转向（`MoveTowardsAngle`）
- 状态切换时用 `AnimationEntryNormalizedTime` 建立首帧采样基线，防止切状态瞬间跳位移
- 必须在动画进度回写之后、位移更新之前调用旋转更新（SPCC 已排好序）

### RootMotionProfileSO（根运动曲线资产）

`[CreateAssetMenu(menuName = "SPCharacter/Motion/RootMotionProfile")]`

| 字段 | 含义 |
|------|------|
| `LocalX` | 本地坐标系 X 轴累计位移曲线（米），key 为归一化时间 |
| `LocalZ` | 本地坐标系 Z 轴累计位移曲线（米），key 为归一化时间 |

曲线存储**从动画起点到当前归一化时间的累计本地位移**，运行期差分采样，切勿手工改成增量曲线。

---

## 四、Editor 工具（外部禁引，仅供使用）

### 根运动烘焙：Tools/SPCharacter/Root Motion Baker

- 从 AnimationClip 的 Generic 根节点 `m_LocalPosition` / Humanoid `RootT` 曲线提取 X/Z 位移
- 按采样数（8~240，默认 60）重采样为归一化时间曲线，并简化关键帧
- 烘焙到已有 `RootMotionProfileSO`（需先创建资产再拖入）

### 状态转换表导入：Tools/SPCharacter/Transition Table Importer

- 读取 XLSX（仅 `.xlsx`，支持 sharedStrings / inlineStr），把二维状态矩阵解析为规则并写入目标 `CharacterStateConfigSO.Rules`
- **表格格式**：方阵。第 1 行是目标状态名（对应 StateNodeSO 资产名），第 1 列是来源状态名（必须与第 1 行同序），左上角 A1 必须有内容
- **单元格格式**：`条件表达式@打断点`，如 `WantToAttack@0.4`；省略 `@打断点` 时打断点为 0；`None` 表示无规则
- **条件表达式**：`+` 连接多个意图位，`!` 前缀表示必须为 0，如 `AnimationCompleted+!WantToEvade`；名称必须与 `CharacterIntention` 枚举一致（大小写敏感）
- 状态名 → 节点 Id 映射：先精确匹配资产名，再匹配 `_状态名` 后缀；多匹配/找不到时报错
- 工作簿多 sheet 时选择第一个能完整映射的 sheet；`Undo.RecordObject` 支持撤销

---

## 五、常用接入流程

### 1. 接玩家控制

1. 创建 `InputTranslator` 资产（`Create > SPCharacter > Input Translator`）
2. 挂上输入模块的 `FrameInputProviderSO`（必填）与相机模块的 `CoordinateConverterProviderSO`（可选）
3. 挂到场景中 SPCC 的 `_directProvider` 意图注入槽位

### 2. 搭状态配置

1. 创建各 `StateNodeSO`：填 Id、动画、IsLooping、根运动 Profile、转向速度、完成旋转
2. 创建 `CharacterStateConfigSO`：填入口 Id + 节点数组
3. 用 XLSX 表（Tools/SPCharacter/Transition Table Importer）导入转移规则，或在 Config 上手动填 Rules
4. 挂到 SPCC 的 `_config`

### 3. 做根运动

1. 创建 `RootMotionProfileSO`
2. 用 Tools/SPCharacter/Root Motion Baker 从 AnimationClip 烘焙
3. 挂到对应 `StateNodeSO.RootMotionProfile`

---

## 六、常见错误

| 错误写法 | 正确写法 | 原因 |
|---------|---------|------|
| `using SPCharacter.Core` | `using SPCharacter.Contract`（+ `SPCharacter.Wiring`） | Core 是实现层，外部禁引 |
| 外部调用黑板写入口（`WriteInput` / `PublishState` 等） | 只通过 `ICharacterIntentionProvider` 供给意图 | 黑板写入方法均为 internal，外部只能 Pull 读取 |
| 新玩家意图源直接实现 `ICharacterIntentionProvider` 挂在场景物体 | 继承 `CharacterIntentionProviderAsset` 做成 SO | SPCC 的 `_directProvider` 是 SO 槽位，Inspector 只能拖资产 |
| 重复做输入手感（死区/缓冲/归一化） | 直接用输入模块 `CurrentProcessed` | 手感处理已归输入模块，角色侧只管翻译 |
| 让状态转移依赖“上一帧”意图 | 意图每帧末由 `ResetIntentions` 清空，条件只判断本帧 | 需要跨帧记忆时扩展 IntentionProcessor/黑板，而不是在规则里猜 |
| 用 Animator Controller 状态机 | StateNodeSO.Animation 配 Animancer Transition | 项目约束：动画走 Animancer 按需播放 |
| 手写根运动位移累加 | 差分采样 RootMotionProfile 曲线 | 曲线存累计位移，直接累加会重复叠加 |
| 在 Update 里改 Transform 位置/旋转 | 走 MotionDriver 的 Rotation/PositionUpdate | SPCC 时序已固定，绕开会导致帧序错乱 |
| 外部引 `SPCharacter.Core.Editor` | 编辑器工具只经菜单使用 | Editor 命名空间是内部实现 |

---

## 七、交叉引用

| 相关文档 | 内容 |
|---------|------|
| [input-module-api.md](input-module-api.md) | 输入模块 API：FrameInputProviderSO、ProcessedFrameInput、ButtonInputState |
| 暂无 | 相机坐标转换器（ICoordinateConverter）细节 |
