# 输入模块核心 API

> **适用场景**：下游系统获取玩家帧输入（移动轴/攻击/闪避/技能/切角色/大招按键）

## 核心原则

1. **只 Pull 不 Push**：输入模块每帧产出 `FrameRawInput`，外部自行拉取。输入模块不维护订阅者、不推送、不分发
2. **经槽位 SO 取数据**：通过 `FrameInputProviderSO.Provider` 获取 `IFrameInputProvider`，禁止直接引用 Core 实现类
3. **空源静默跳过**：`Provider` 为 null 时不抛异常，静默 return
4. **守边界**：只引 `SPInput_Contract` + `SPInput_Wiring`，禁止引 `SPInput_Core` / `SPInput_Debug`

---

## 一、核心 API

### 数据结构：FrameRawInput

```csharp
namespace SPInput_Contract
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

### 访问接口：IFrameInputProvider

```csharp
namespace SPInput_Contract
{
    public interface IFrameInputProvider
    {
        FrameRawInput CurrentFrame { get; }
    }
}
```

### 槽位 SO：FrameInputProviderSO

```csharp
namespace SPInput_Wiring
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
using SPInput_Contract;   // FrameRawInput, IFrameInputProvider
using SPInput_Wiring;     // FrameInputProviderSO
```

两个都要引。不合并不妥协——Contract 是稳定数据形状，Wiring 是可换接线机制，分立是架构核心。

---

## 二、使用模式

### 标准下游消费（以角色输入源为例）

```csharp
using SPInput_Contract;
using SPInput_Wiring;
using UnityEngine;

public class ConsumerExample : MonoBehaviour
{
    [SerializeField] private FrameInputProviderSO _inputProviderSO;

    private void Update()
    {
        var provider = _inputProviderSO != null ? _inputProviderSO.Provider : null;
        if (provider == null) return;

        FrameRawInput input = provider.CurrentFrame;

        if (input.MoveAxisValue.magnitude > 0.1f)
            Debug.Log($"移动: {input.MoveAxisValue}");

        if (input.AttackPressed)
            Debug.Log("攻击");
    }
}
```

### 减少显式类型名（可选）

若私有方法需要传入输入数据，可传 `IFrameInputProvider` 而非 `FrameRawInput`，省掉 `using SPInput_Contract`：

```csharp
// 方法签名收接口而非 struct，调用侧用 var 推断
private void Process(IFrameInputProvider provider)
{
    var input = provider.CurrentFrame;   // var 推断，字段访问不需类型名可见
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

---

## 三、常见错误

| 错误写法 | 正确写法 | 原因 |
|---------|---------|------|
| `using SPInput_Core` | `using SPInput_Contract` + `using SPInput_Wiring` | Core 是实现层，外部禁引 |
| `Provider.CurrentFrame` 无 null 检查 | `if (provider == null) return;` | Provider 可能为 null，必须静默跳过 |
| `_inputProviderSO.Bind(...)` | 不调用 Bind | Bind/Clear 是 internal，只由接线胶水调 |
| 在输入模块内加 `IInputFrameSink` / 推送 | 外部自行 Pull | 输入模块不做分发，分发交事件模块 |
| 在输入模块内定义业务语义事件 | 事件定义放事件模块 | 输入模块只产硬件数据，不知业务 |
| `UnityEngine.Input.GetAxis(...)` | `provider.CurrentFrame.MoveAxisValue` | 禁止旧 Input Manager，走新输入系统经模块产出 |

---

## 四、内部结构（外部禁引，仅供了解）

| 层 | 命名空间 | 文件 | 职责 |
|----|---------|------|------|
| Contract | `SPInput_Contract` | `FrameRawInput.cs` | 帧数据 struct |
| Contract | `SPInput_Contract` | `IFrameInputProvider.cs` | 访问接口 |
| Core | `SPInput_Core` | `FrameInputCollector.cs` | 采集器，实现接口，Awake 强校验 binding（throw） |
| Core | `SPInput_Core` | `InputBindingSO.cs` | InputActionReference 绑定 SO |
| Wiring | `SPInput_Wiring` | `FrameInputProviderSO.cs` | 槽位 SO（运行时信箱） |
| Wiring | `SPInput_Wiring` | `InputFrameWiring.cs` | 接线胶物，Bind/Clear |
| Debug | `SPInput_Debug` | `FrameInputDebugger.cs` | 按键按下时 Debug.Log |

执行时序：`FrameInputCollector [-400]` 采集 → `InputFrameWiring [-390]` 注入 → 下游 Pull。

---

## 五、交叉引用

| 相关文档 | 内容 |
|---------|------|
| 暂无 | 暂无 |
