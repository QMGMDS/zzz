# 输入模块 API

## 一、模块概述

输入模块（`Module.SPInput` 程序集）对外提供一项能力：读取当前帧的玩家输入。能力以**模块级服务**形式注册到 `ModuleServiceHub`，契约定义在 `SPInput.Contract` 命名空间：

| 契约接口 | 能力 |
| --- | --- |
| `IProvideFrameInput` | 提供当前帧的玩家输入，分原始输入与后处理输入两份数据 |

边界约定：外部只允许引用 `SPInput.Contract`，通过 `ModuleServiceHub.TryGet` 获取服务；`SPInput.Core` 与 `SPInput.Wiring` 中的类型均为 `internal`，编译期对外不可见。

### IProvideFrameInput

```csharp
public interface IProvideFrameInput : IModuleService
{
    RawFrameInput CurrentFrame { get; }
    ProcessedFrameInput CurrentProcessed { get; }
}
```

- `CurrentFrame`：原始输入，纯硬件事实汇报，无任何手感处理。
- `CurrentProcessed`：后处理输入，在原始数据基础上做了手感加工（长按判定、归零缓冲、方向归一化）。

两份数据均为值类型，构造期定稿后只读，下游不可修改；每帧刷新一次，读取到的总是当前帧数据。按需要的加工深度二选一即可。

方向约定：本文档中所有平面方向（`MoveAxisValue`、`MoveDirection`）的 `x` 分量表示右方向，`y` 分量表示前方向。

### RawFrameInput

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

- `FrameIndex`：采集器每帧递增的帧计数，可用于判断数据是否已刷新。
- `MoveAxisValue`：移动轴原始读值，未做死区过滤与归一化。
- 各 `IsXxxPressed`：按键的按下边沿，仅按键落下的那一帧为 `true`。

### ProcessedFrameInput

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
```

- 各按键为 `ButtonInputState`：
  - `IsPressed`：按下边沿，与原始输入同源。
  - `IsHeld`：持续按压时长已超过长按阈值，松开即失效并复位。
- `MoveDirection`：经归零缓冲并归一化的单位方向向量；无输入时为零向量。松键后的一小段缓冲期内仍输出最后的非零方向。
- `HasMoveInput`：本帧是否存在有效移动输入（轴非零，或处于归零缓冲期内）；为 `false` 时 `MoveDirection` 必为零向量。

### ButtonInputState

`ButtonInputState` 与 `ProcessedFrameInput` 定义于同一文件（`Contract/Data/ProcessedFrameInput.cs`，命名空间 `SPInput.Contract`）：

```csharp
public struct ButtonInputState
{
    public bool IsPressed { get; init; }
    public bool IsHeld { get; init; }
}
```

- `IsPressed`：本帧被按下（按下边沿，与原始 `RawFrameInput` 同源）。
- `IsHeld`：被长按——持续按压时长已超过长按判定阈值，松开即失效并复位。

## 二、API 调用示例

获取服务统一使用 `ModuleServiceHub.TryGet`。服务未注册或已销毁时返回 `false` 且 `out` 结果为 `null`，调用方必须自行降级，不可默认服务必然可用。

### 读取后处理帧输入

```csharp
using SPFramework.Service;
using SPInput.Contract;

if (ModuleServiceHub.TryGet<IProvideFrameInput>(out IProvideFrameInput provider))
{
    ProcessedFrameInput input = provider.CurrentProcessed;

    Vector2 moveDirection = input.HasMoveInput ? input.MoveDirection : Vector2.zero;
    bool attackPressed = input.Attack.IsPressed; // 本帧攻击按下边沿
    bool attackHeld = input.Attack.IsHeld;       // 攻击键是否已构成长按
}
// 服务缺失时跳过本次读取
```

### 读取原始帧输入

```csharp
using SPFramework.Service;
using SPInput.Contract;

if (ModuleServiceHub.TryGet<IProvideFrameInput>(out IProvideFrameInput provider))
{
    RawFrameInput raw = provider.CurrentFrame;

    Vector2 rawAxis = raw.MoveAxisValue;    // 未加工的移动轴读值
    bool evadePressed = raw.IsEvadePressed; // 本帧闪避按下边沿
}
// 服务缺失时跳过本次读取
```

## 三、反例

| 反例 | 正确做法 |
| --- | --- |
| 引用 `SPInput.Core` / `SPInput.Wiring` 中的类型 | 只引用 `SPInput.Contract`，经 `ModuleServiceHub` 获取接口 |
| 使用旧输入管理器（`Input.GetAxis`、`Input.GetKey` 等）自行读取硬件输入 | 借用 `IProvideFrameInput` 读取帧输入 |
| 自行启用 InputAction 重复采集玩家输入 | 借用 `IProvideFrameInput`，采集由模块内部统一完成 |
| 需要手感输入时，基于 `CurrentFrame` 自行重复实现长按判定、归零缓冲 | 直接读取 `CurrentProcessed` 的后处理结果 |
| 不判空直接调用服务，默认其必然可用 | 用 `TryGet` 获取，失败时按上文示例降级 |
| 长期缓存服务接口并假设其永久有效 | 服务随模块内部的注册/注销生命周期变动，每次按需 `TryGet` |
| 缓存帧数据跨帧使用 | 帧输入只描述当前帧，每帧按需重新读取 |
