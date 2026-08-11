# 输入模块 API 速览

> 适用场景：下游系统按帧读取玩家输入（移动、攻击、闪避、技能、切角色、大招）。
> 边界约定：外部只允许引用 `SPInput.Contract` + `SPInput.Wiring`，不要直接引入 `SPInput.Core` / `SPInput.Debug`。
> 读取原则：输入模块只提供 Pull，不做事件分发；下游每帧自行读取。

## 一、核心 API

### 对外公开的命名空间

```csharp
using SPInput.Contract;
using SPInput.Wiring;
```

- `Contract`：稳定的数据结构与读取接口。
- `Wiring`：槽位 SO 和接线胶水，负责把内部实现接到外部入口。

### 数据结构：RawFrameInput

```csharp
public struct RawFrameInput
{
    public ulong FrameIndex { get; init; }
    public Vector2 MoveAxisValue { get; init; }
    public bool IsAttackPressed { get; init; }
    public bool IsEvadePressed { get; init; }
    public bool IsSkillPressed { get; init; }
    public bool IsSwitchCharacterPressed { get; init; }
    public bool IsUltimatePressed { get; init; }
}
```

- 表示硬件输入的帧级事实。
- 布尔值都是“本帧按下边沿”，不是持续按住。
- 不包含死区、归一化、缓冲、长按等后处理。

### 数据结构：ProcessedFrameInput / ButtonInputState

```csharp
public struct ProcessedFrameInput
{
    public ulong FrameIndex { get; init; }
    public ButtonInputState Attack { get; init; }
    public ButtonInputState Evade { get; init; }
    public ButtonInputState Skill { get; init; }
    public ButtonInputState SwitchCharacter { get; init; }
    public ButtonInputState Ultimate { get; init; }
    public Vector2 MoveDirection { get; init; }
    public bool HasMoveInput { get; init; }
}

public struct ButtonInputState
{
    public bool IsPressed { get; init; }
    public bool IsHeld { get; init; }
}
```

- `IsPressed`：本帧按下边沿。
- `IsHeld`：持续按压时间超过统一长按阈值后为真，松开即复位。
- `MoveDirection`：经过归零缓冲后的方向，并归一化为单位向量。
- `HasMoveInput`：本帧有有效输入，或仍处于归零缓冲期。

### 读取接口：IProvideFrameInput

```csharp
public interface IProvideFrameInput
{
    RawFrameInput CurrentFrame { get; }
    ProcessedFrameInput CurrentProcessed { get; }
}
```

- 这是“读取契约”，不是采集器本体。
- 外部只依赖这个接口，不直接依赖 `FrameInputCollector`。

### 入口资产：FrameInputProviderSO

- 外部通过 `FrameInputProviderSO.Provider` 获取 `IProvideFrameInput`。
- `Provider` 可能为空；为空时应直接跳过，不要抛异常。
- `Bind(...)` / `Clear()` 为 `internal`，仅供输入模块内部接线胶水使用。

### 内部实现：FrameInputCollector / FrameInputWiring

- `FrameInputCollector`：采集硬件输入，执行后处理，并实现 `IProvideFrameInput`。
- `FrameInputWiring`：把 `FrameInputCollector` 注入到 `FrameInputProviderSO`。
- 这两者属于模块内部实现，不是跨模块 API。

### 后处理参数：ProcessedFrameConfigSO

- `HoldThreshold`：统一长按阈值，控制所有按键形输入的 `IsHeld`。
- `ReleaseBuffer`：轴输入归零缓冲时长，决定空档期是否沿用上一帧方向。
- 外部不重复实现这些手感逻辑，直接消费 `CurrentProcessed`。

## 二、使用模式

### 标准消费方式

```csharp
using UnityEngine;

using SPInput.Contract;
using SPInput.Wiring;

public sealed class InputConsumerExample : MonoBehaviour
{
    [SerializeField] private FrameInputProviderSO _inputProviderSO;

    private void Update()
    {
        IProvideFrameInput provider = _inputProviderSO != null ? _inputProviderSO.Provider : null;
        if (provider == null) return;

        ProcessedFrameInput input = provider.CurrentProcessed;

        if (input.HasMoveInput)
            Debug.Log($"移动方向: {input.MoveDirection}");

        if (input.Attack.IsPressed)
            Debug.Log("攻击");

        if (input.Skill.IsHeld)
            Debug.Log("技能长按");
    }
}
```

### 何时读 CurrentFrame，何时读 CurrentProcessed

- `CurrentFrame`：调试原始按键、分析底层输入、做特殊诊断时使用。
- `CurrentProcessed`：正常玩法逻辑优先使用，角色、状态、技能判断都尽量走它。

### 推荐的空源保护

```csharp
var provider = _inputProviderSO?.Provider;
if (provider == null) return;
```

- `Provider` 为空通常意味着尚未接线、对象已销毁、或场景未就绪。
- 这里应静默返回，不要把空源当成异常流程。

### 推荐的读取方式

```csharp
private void TickInput(IProvideFrameInput provider)
{
    var input = provider.CurrentProcessed;
    // 直接消费，不重复做死区、归一化、防抖、长按判断
}
```

- 输入模块已经完成统一处理。
- 下游只关注“当前能不能用、是否按下、是否长按、方向是什么”。

### 执行顺序

```text
FrameInputCollector 先采集 + 后处理
        ↓
FrameInputWiring 再注入 FrameInputProviderSO
        ↓
下游模块每帧 Pull Provider
```

## 三、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| `using SPInput.Core` | `using SPInput.Contract` + `using SPInput.Wiring` | Core 是实现层，外部禁引 |
| 直接引用 `FrameInputCollector` | 通过 `FrameInputProviderSO.Provider` 获取 `IProvideFrameInput` | 采集器不是项目级 API |
| `provider.CurrentFrame` 不判空 | `if (provider == null) return;` | Provider 可能未注入或已销毁 |
| 外部手调 `_inputProviderSO.Bind(...)` | 不调用 Bind | `Bind/Clear` 是内部接线入口 |
| 在输入模块里做推送/订阅 | 外部自行 Pull | 输入模块只产输入，不做分发 |
| 在输入模块里定义业务事件 | 业务事件放事件模块 | 输入模块只负责输入语义，不理解业务 |
| 使用 `UnityEngine.Input.GetAxis(...)` | 使用 `provider.CurrentFrame.MoveAxisValue` | 项目约束是 Input System，不走旧 Input Manager |
| 角色侧重复做死区、缓冲、长按 | 直接用 `CurrentProcessed` | 手感处理已在输入模块内统一完成 |

## 四、交叉引用

| 相关文档 | 用途 |
|---|---|
| [project-module-boundaries.md](project-module-boundaries.md) | 项目级模块边界、`public` / `internal` 约定、跨模块引用规则 |
| [camera-module-api.md](camera-module-api.md) | 摄像机模块 API；常与输入方向联动 |
| [character-module-api.md](character-module-api.md) | 角色模块 API；通常直接消费 `CurrentProcessed` |

> 建议顺序：先看 `project-module-boundaries.md`，再看输入、摄像机、角色三份模块文档。
