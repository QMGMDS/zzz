# 角色模块 API 速览

> 适用场景：装配角色控制器、配置状态节点/转移规则、接入玩家或 AI 意图源、排查动画/运动/时序问题。
> 边界约定：角色模块当前**没有稳定 public Contract API**；`Assets/_Scripts/Character/Contract` 为空，外部模块不要引用 `SPCharacter.Core`。
> 使用原则：通过 Inspector 组装 `SPCC`、状态配置和模块内 Wiring；跨模块新增能力时先补 Contract + 模块服务。

## 一、核心结构

### 当前命名空间

```csharp
// 当前仅角色模块内部使用
using SPCharacter.Core;
using SPCharacter.Wiring;
```

- `Core`：角色控制器、状态机、动画、运动、黑板、配置资产。
- `Wiring`：模块内胶水组件，例如玩家输入到角色意图的接线。
- `Contract`：已预留目录，但当前没有可供外部依赖的接口。

> `PlayerInputIntentionWiring` 也是 `internal`，属于角色自身装配，不是外部调用 API。

### Root 组件：SPCC

`SPCC` 挂在角色根对象上，要求同对象具备：

- `Animator`：骨骼/Avatar 入口；不要用 Animator Controller 状态机。
- `AnimancerComponent`：实际动画播放入口。
- `CharacterController`：运动落位入口。
- `CCStateConfigSO`：该角色状态图配置。

运行时：`SPCC.Start` 会关闭 `Animator.applyRootMotion`，创建黑板、状态机、动画驱动、运动驱动和 Wiring 扩展管线。`SPCC` 只做装配和时序，不写具体玩法逻辑。

### 固定帧时序

```text
Update:
  Wiring 写入本帧意图
  StateMachine 判断转移
  ResetIntentions 清空本帧控制意图
  AnimationDriver 播放新状态动画

LateUpdate:
  AnimationDriver 回写动画进度 / 报告非循环动画完成
  MotionDriver 更新朝向
  MotionDriver 按 RootMotionProfile 位移
```

- 玩家/AI 控制意图只保留一帧。
- `AnimationCompleted` 由动画层在 `LateUpdate` 写入，供下一帧状态机消费。
- 位移统一由 `MotionDriver` 调用 `CharacterController.Move`；外部不要直接移动角色根对象。

## 二、配置资产

### StateNodeSO

创建菜单：`SPCharacter/State/StateNode`

| 字段 | 含义 |
|---|---|
| `Id` | 状态唯一标识；同一 `CCStateConfigSO` 内必须唯一。 |
| `IsLooping` | 循环状态不会自动产生 `AnimationCompleted`。 |
| `Animation` | Animancer `TransitionAssetBase`，进入状态时播放。 |
| `RootMotionProfile` | 离线烘焙累计本地位移；为空表示无根运动位移。 |
| `TurnSpeedDegreesPerSecond` | 每秒最大转向角；`0` 表示不主动转向。 |

新增动作优先新增 `StateNodeSO` + Animancer Transition，不新增 Animator Controller 状态；位移写在节点和 RootMotionProfile，不在外部脚本手推角色。

### CCStateConfigSO

创建菜单：`SPCharacter/State/CCStateConfig`

- `EntryId`：入口状态 Id。
- `Nodes`：该角色全部状态节点。
- `Rules`：状态转移规则。

运行时会校验：`Nodes` 非空、`EntryId` 存在、`Node.Id` 非空且不重复、`Rule.FromId/ToId` 均指向已配置节点。

### 状态转移规则

`StateTransitionRule` 由意图位掩码驱动：

- `Required`：必须全部为 1。
- `Forbidden`：必须全部为 0。
- `InterruptPoint`：来源动画归一化进度达到该值后才允许转移，范围 `0..1`。
- `Priority`：数值越大越优先；同优先级按配置顺序判断。

可用意图：

| 意图 | 来源/用途 |
|---|---|
| `AnimationCompleted` | 动画层自动写入，表示当前非循环动画完成。 |
| `WantToMove` | 控制意图：希望移动。 |
| `WantToAttack` | 控制意图：攻击按下。 |
| `WantToHoldAttack` | 控制意图：攻击长按。 |
| `WantToEvade` | 控制意图：闪避按下。 |
| `WantToTurn` / `WantToSwitchIn` / `WantToSwitchOut` | 预留控制意图，当前没有内置生产者。 |

### Excel 导入状态矩阵

`CCStateConfigSO` Inspector 提供“从 Excel 导入状态转移规则（.xlsx）”。

```text
A1 空置；B1.. 写 To 状态 Id
A2.. 写 From 状态 Id
交叉单元格写条件；空白或 None 表示无转移
```

单元格示例：

```text
WantToMove
WantToMove+!WantToAttack
AnimationCompleted @0.8 #10
```

- `+` 组合条件；`!` 表示 forbidden。
- `@` 设置 `InterruptPoint`；`#` 设置 `Priority`，放在单元格末尾。
- 意图名称大小写敏感，使用 `CCIntention` 精确枚举名。

### RootMotionProfileSO

创建菜单：`SPCharacter/Motion/RootMotionProfile`

- Inspector 选择 `AnimationClip` 后点击“从 AnimationClip 烘焙位移曲线”。
- 资产记录动画本地 X/Z 的**累计位移曲线**。
- 运行时用“当前采样 - 上帧采样”得到本帧位移，再按角色朝向转到世界空间并 `CharacterController.Move`。

## 三、使用模式

### 玩家角色装配

```text
角色根 GameObject:
  Animator
  AnimancerComponent
  CharacterController
  SPCC
    _animator / _animancer / _characterController / _config
  PlayerInputIntentionWiring
    ModuleServiceHub.Get<IProvideFrameInput>()
    ModuleServiceHub.Get<IConvertCameraTransform>()
```

`PlayerInputIntentionWiring` 当前映射：

- `CurrentProcessed.MoveDirection` → 可选相机坐标转换 → `SetMoveAxis` + `WantToMove`
- `Attack.IsPressed` → `WantToAttack`
- `Attack.IsHeld` → `WantToHoldAttack`
- `Evade.IsPressed` → `WantToEvade`

空源约定：输入服务必选，`Start+` 直接取用；摄像机转换服务可选，为空则回退输入方向。

### 新增动作配置流程

1. 创建或复用 Animancer Transition 资产。
2. 创建 `StateNodeSO`，填写唯一 `Id`、动画、循环标记、转向速度。
3. 需要位移时创建 `RootMotionProfileSO` 并从 AnimationClip 烘焙。
4. 把节点加入 `CCStateConfigSO.Nodes`，再通过 `Rules` 或 Excel 状态矩阵添加转移。
5. 检查打断点、优先级、循环状态和 `AnimationCompleted` 语义。

### 意图源扩展边界

角色模块内部扩展通过 `ICCWiringExtension.UpdateWiring(CCWiringContext, IWriteIntention)` 写入意图。

- 扩展组件必须和 `SPCC` 在同一个 GameObject 上；管线通过 `GetComponents<MonoBehaviour>()` 收集。
- 执行顺序等于组件顺序；禁用组件会被跳过。
- `IWriteIntention` 只能写控制意图，不能写 `AnimationCompleted`。
- 每帧开始会先把移动方向清为 `Vector2.zero`。

当前内置扩展只有 `PlayerInputIntentionWiring`。AI/队伍系统若要驱动角色，不要直接拿黑板；应先设计 `SPCharacter.Contract` + 模块服务，或在角色模块内新增专用 Wiring。

## 四、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| 外部模块 `using SPCharacter.Core` | 等待/新增 `SPCharacter.Contract` + 模块服务 | Core 是实现层，当前无 public 角色 API |
| 直接改 `CCRunTimeBlackboard` | 通过角色模块内 Wiring 写意图 | 黑板只服务内部子系统 |
| 用 Animator Controller / `SetTrigger` | `StateNodeSO` + Animancer Transition | 项目约束是 Animancer 按需播放 |
| 开启 `Animator.applyRootMotion` | `RootMotionProfileSO` + `MotionDriver` | 根运动由烘焙曲线统一控制 |
| 外部移动角色 Transform | 由 `MotionDriver` 执行位移 | 避免破坏状态/动画/运动时序 |
| 外部写入 `AnimationCompleted` | 只由 `AnimationDriver` 自动报告 | 它不是控制意图 |
| 角色侧重复读硬件输入 | 消费输入模块 `CurrentProcessed` | 输入手感已统一处理 |
| 自己重算相机系方向 | `ModuleServiceHub.Get<IConvertCameraTransform>()` | 坐标转换属于摄像机模块能力 |
| 手改 `.asset` YAML 的意图整数 | 用 Inspector / Excel 导入 | 避免位掩码和 GUID 错配 |

## 五、交叉引用

| 相关文档 | 用途 |
|---|---|
| [framework-core.md](framework-core.md) | 访问级别语义、Contract / 模块服务、事件总线 |
| [input-module-api.md](input-module-api.md) | 输入服务 `IProvideFrameInput` 与 `ProcessedFrameInput` 语义 |
| [camera-module-api.md](camera-module-api.md) | 输入方向转相机系世界方向、摄像机跟随目标接口 |

> 建议顺序：先看 `framework-core.md`，再看输入、摄像机、角色三份模块文档。
