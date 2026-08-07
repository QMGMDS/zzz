# 输入模块核心 API

> **适用场景**：下游系统获取玩家帧输入（移动轴/攻击/闪避/技能/切角色/大招按键）

## 核心原则

1. **只 Pull 不 Push**：输入模块每帧产出 `FrameRawInput`（原始）与 `ProcessedFrameInput`（后处理），外部自行拉取。输入模块不维护订阅者、不推送、不分发
2. **经槽位 SO 取数据**：通过 `FrameInputProviderSO.Provider` 获取 `IFrameInputProvider`，禁止直接引用 Core 实现类
3. **空源静默跳过**：`Provider` 为 null 时不抛异常，静默 return
4. **守边界**：只引 `SPInput.Contract` + `SPInput.Wiring`，禁止引 `SPInput.Core` / `SPInput.Debug`

---

## 一、核心 API

### 数据结构：FrameRawInput（原始 - 纯硬件事实）

```csharp
namespace SPInput.Contract
{
    public struct FrameRawInput
    {
        public ulong FrameIndex;
        public Vector2 MoveAxisValue;       // WASD 移动轴（连续值）
        public bool AttackPressed;           // 本帧按下边沿
        public bool EvadePressed;
        public bool SkillPressed;
        public bool SwitchCharacterPressed;
        public bool UltimatePressed;
    }
}
```

布尔字段均为 `WasPressedThisFrame` 语义（本帧按下边沿），非持续状态。

### 数据结构：ProcessedFrameInput（后处理特供数据）

```csharp
namespace SPInput.Contract
{
    public struct ProcessedFrameInput
    {
        public ulong FrameIndex;
        public ButtonInputState Attack;
        public ButtonInputState Evade;
        public ButtonInputState Skill;
        public ButtonInputState SwitchCharacter;
        public ButtonInputState Ultimate;
        public Vector2 MoveDirection;   // 延时缓冲 + 归一化后的单位方向向量（无输入时为零向量）
        public bool HasMoveInput;       // 是否存在有效移动输入（含归零缓冲期）
    }

    public struct ButtonInputState
    {
        public bool IsPressed;  // 本帧被按下（按下边沿）
        public bool IsHeld;     // 被长按 - 持续按压时长超过阈值，松开即失效复位
    }
}
```

**按键形语义**：
- `IsPressed` = 本帧被按下（与原始 `FrameRawInput` 同源的按下边沿）。
- `IsHeld` = 本次持续按压时长已超过统一长按阈值，松开即归零并失效。
- 所有按键形共用同一长按阈值（由 `InputProcessingConfigSO.HoldThreshold` 配置）。

**轴输入形语义**：
- `MoveDirection` = 延时缓冲后的方向，再归一化为单位向量。
  - 本帧非零：直接采用并刷新缓冲计时。
  - 本帧为零但归零缓冲期内（`ReleaseBuffer` 秒内）：沿用上一帧非零方向。
  - 缓冲超时：冻结为零向量。
- `HasMoveInput` = 本帧非零或处于归零缓冲期内时为 true，否则 false。

### 访问接口：IFrameInputProvider

```csharp
namespace SPInput.Contract
{
    public interface IFrameInputProvider
    {
        FrameRawInput CurrentFrame { get; }         // 原始输入
        ProcessedFrameInput CurrentProcessed { get; } // 后处理特供数据
    }
}
```

### 槽位 SO：FrameInputProviderSO

```csharp
namespace SPInput.Wiring
{
    public class FrameInputProviderSO : ScriptableObject
    {
        public IFrameInputProvider Provider { get; }   // 未注入时为 null
        internal void Bind(IFrameInputProvider provider);
        internal void Clear();
    }
}
```

### 命名空间引用

```csharp
using SPInput.Contract;   // FrameRawInput, ProcessedFrameInput, ButtonInputState, IFrameInputProvider
using SPInput.Wiring;     // FrameInputProviderSO
```

两个都要引。不合并不妥协 - Contract 是稳定数据形状，Wiring 是可换接线机制，分立是架构核心。

---

## 二、使用模式

### 标准下游消费（以角色输入源为例）

```csharp
using SPInput.Contract;
using SPInput.Wiring;
using UnityEngine;

public class ConsumerExample : MonoBehaviour
{
    [SerializeField] private FrameInputProviderSO _inputProviderSO;

    private void Update()
    {
        var provider = _inputProviderSO != null ? _inputProviderSO.Provider : null;
        if (provider == null) return;

        ProcessedFrameInput input = provider.CurrentProcessed;

        if (input.HasMoveInput)
            Debug.Log($"移动方向: {input.MoveDirection}");

        if (input.Attack.IsPressed)
            Debug.Log("攻击");

        if (input.Skill.IsHeld)
            Debug.Log("技能长按蓄力");
    }
}
```

### 减少显式类型名（可选）

若私有方法需要传入输入数据，可传 `IFrameInputProvider` 而非 `ProcessedFrameInput`，省掉 `using SPInput.Contract`：

```csharp
// 方法签名收接口而非 struct，调用侧用 var 推断
private void Process(IFrameInputProvider provider)
{
    var input = provider.CurrentProcessed;   // var 推断，字段访问不需类型名可见
    // ...
}
```

### 空源保护

```csharp
// Provider 可能为 null：接线胶水未放 / 未注入 / Collector 已销毁
// 必须 null 保护，静默跳过
var provider = _inputProviderSO?.Provider;
if (provider == null) return;
```

### 何时用原始 vs 后处理

- 用 `CurrentFrame`（原始）：需要硬件原始事实、自己做手感（极少，仅特殊调试）。
- 用 `CurrentProcessed`（后处理）：常态 - 角色控制、状态机判断手感统一由输入模块托管。

---

## 三、常见错误

| 错误写法 | 正确写法 | 原因 |
|---------|---------|------|
| `using SPInput.Core` | `using SPInput.Contract` + `using SPInput.Wiring` | Core 是实现层，外部禁引 |
| `Provider.CurrentFrame` 无 null 检查 | `if (provider == null) return;` | Provider 可能为 null，必须静默跳过 |
| `_inputProviderSO.Bind(...)` | 不调用 Bind | Bind/Clear 是 internal，只由接线胶水调 |
| 在输入模块内加 `IInputFrameSink` / 推送 | 外部自行 Pull | 输入模块不做分发，分发交事件模块 |
| 在输入模块内定义业务语义事件 | 事件定义放事件模块 | 输入模块只产硬件数据 + 手感后处理，不知业务 |
| `UnityEngine.Input.GetAxis(...)` | `provider.CurrentFrame.MoveAxisValue` | 禁止旧 Input Manager，走新输入系统经模块产出 |
| 在角色侧重复做死区/防抖/归一化 | 直接用 `CurrentProcessed` | 手感处理已还给输入模块，下游严禁重复 |

---

## 四、内部结构（外部禁引，仅供了解）

| 层 | 命名空间 | 文件 | 职责 |
|----|---------|------|------|
| Contract | `SPInput.Contract` | `FrameRawInput.cs` | 原始帧数据 struct |
| Contract | `SPInput.Contract` | `ProcessedFrameInput.cs` | 后处理帧数据 struct + ButtonInputState |
| Contract | `SPInput.Contract` | `IFrameInputProvider.cs` | 访问接口（原始 + 后处理） |
| Core | `SPInput.Core` | `FrameInputCollector.cs` | 采集器 + 后处理，实现接口，Awake 强校验 binding + processingConfig |
| Core | `SPInput.Core` | `InputBindingSO.cs` | InputActionReference 绑定 SO |
| Core | `SPInput.Core` | `InputProcessingConfigSO.cs` | 后处理参数配置 SO（长按阈值、归零缓冲） |
| Wiring | `SPInput.Wiring` | `FrameInputProviderSO.cs` | 槽位 SO（运行时信箱） |
| Wiring | `SPInput.Wiring` | `InputFrameWiring.cs` | 接线胶水，Bind/Clear |
| Debug | `SPInput.Debug` | `FrameInputDebugger.cs` | 按键按下时 Debug.Log |

执行时序：`FrameInputCollector [-400]` 采集+后处理 → `InputFrameWiring [-390]` 注入 → 下游 Pull。

### 后处理可调参数（InputProcessingConfigSO）

| 参数 | 字段 | Inspector Range | SO 缺省值 | 含义 |
|------|------|-----------------|-----------|------|
| 长按判定阈值 | `HoldThreshold` | [0, 2] s | 0.3 s | 按键持续按压超过此时长即长按；所有按键形共用 |
| 归零缓冲时长 | `ReleaseBuffer` | [0, 0.5] s | 0.1 s | 轴输入归零后沿用上一帧方向的最长时长，补偿 A→D 空隙 |

> SO 缺省值指 `InputProcessingConfigSO` 资产序列化字段的初值，可在 Inspector 调整。
> `FrameInputCollector` 不做兜底：未配置 `InputProcessingConfigSO` 时，Awake 抛异常拒绝运行，与 `InputBindingSO` 漏配同处理。

---

## 五、交叉引用

| 相关文档 | 内容 |
|---------|------|
| 暂无 | 暂无 |