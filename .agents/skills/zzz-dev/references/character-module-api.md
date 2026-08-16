# 角色模块 API

## 一、模块概述

角色模块（`Module.SPCharacter` 程序集）对外提供一项**实例级服务**与三组事实广播，契约定义在 `SPCharacter.Contract` 命名空间：

| 契约 | 形式 | 能力 |
| --- | --- | --- |
| `ICharacterSwitchSession` | 实例级服务 | 驱动单个可切换角色的上场/退场切换会话、设置该角色的玩家操作锁 |
| `CharacterEvents.SwitchInPoseApplied` | 事件 | 角色上场位姿已应用 |
| `CharacterEvents.SwitchInCompleted` | 事件 | 角色上场动画完成 |
| `CharacterEvents.SwitchOutCompleted` | 事件 | 角色退场动画完成 |

边界约定：外部只允许引用 `SPCharacter.Contract`；服务经 `InstanceServiceHub.TryGet<T>(id, out T)` 按角色 Id 获取，事件经 `EventBus.Subscribe` 订阅。`SPCharacter.Core` 与 `SPCharacter.Wiring` 中的类型均为 `internal`，编译期对外不可见。

### ICharacterSwitchSession（实例级服务）

```csharp
public interface ICharacterSwitchSession : IInstanceService
{
    void BeginSwitchOut();
    void BeginSwitchIn(Pose pose);
    void SetOperationLocked(bool isLocked);
}
```

每个可切换角色实例以自身角色 Id 为注册键自注册；会话随角色实例启用而注册、随禁用而注销，实例未激活或已销毁时 `TryGet` 失败。

- `BeginSwitchOut()`：请求角色播放退场动画。请求为异步语义——调用返回仅代表请求已受理，模块择机进入退场状态，退场动画播放完成时广播 `Character.SwitchOutCompleted`。
- `BeginSwitchIn(Pose pose)`：请求角色落位到 `pose` 并播放上场动画。模块先应用落位（落位完成时广播 `Character.SwitchInPoseApplied`），再进入上场状态；上场动画播放完成时广播 `Character.SwitchInCompleted`。
- `SetOperationLocked(bool isLocked)`：设置该角色的玩家操作锁。锁定期间角色不响应玩家操作输入，传入 `false` 解锁后恢复。

行为约定：

- 三个方法均为"请求-广播"式异步语义，调用返回不代表动作完成，完成与否以对应事件为准。
- 同一次会话内，上场/退场完成事件各自只广播一次，不重复。
- 请求可重复发起：`BeginSwitchIn` 以最新一次的位姿为准；`BeginSwitchOut` 重复调用无额外效果。

### 事件

```csharp
public static class CharacterEvents
{
    // 事件标识名："Character.SwitchInPoseApplied"
    public static readonly EventKey<CharacterSwitchInPoseAppliedEvent> SwitchInPoseApplied;

    // 事件标识名："Character.SwitchInCompleted"
    public static readonly EventKey<CharacterSwitchInCompletedEvent> SwitchInCompleted;

    // 事件标识名："Character.SwitchOutCompleted"
    public static readonly EventKey<CharacterSwitchOutCompletedEvent> SwitchOutCompleted;
}

public readonly struct CharacterSwitchInPoseAppliedEvent
{
    public CharacterSwitchInPoseAppliedEvent(string characterId);
    public string CharacterId { get; } // 完成落位的角色 Id
}

public readonly struct CharacterSwitchInCompletedEvent
{
    public CharacterSwitchInCompletedEvent(string characterId);
    public string CharacterId { get; } // 完成上场的角色 Id
}

public readonly struct CharacterSwitchOutCompletedEvent
{
    public CharacterSwitchOutCompletedEvent(string characterId);
    public string CharacterId { get; } // 完成退场的角色 Id
}
```

事件语义：

- `Character.SwitchInPoseApplied`：角色已落位到上场位姿。此刻角色已处于上场位置，但上场动画尚未完成；需要在"新角色已就位"这一时机响应的订阅方应订阅本事件，而不是等待上场完成。
- `Character.SwitchInCompleted`：角色上场动画播放完成。
- `Character.SwitchOutCompleted`：角色退场动画播放完成，该角色的退场流程至此结束。

时序约定：对同一角色的同一次上场，`SwitchInPoseApplied` 必先于 `SwitchInCompleted` 广播。

## 二、API 调用示例

实例级服务统一使用 `InstanceServiceHub.TryGet<T>(id, out T)` 获取，`id` 为角色 Id。服务未注册（角色实例未激活或已销毁）时返回 `false` 且 `out` 结果为 `null`，调用方必须自行降级，不可默认服务必然可用。

### 请求角色上场/退场

```csharp
using UnityEngine;

using SPCharacter.Contract;
using SPFramework.Service;

// 请求目标角色落位并上场
if (InstanceServiceHub.TryGet<ICharacterSwitchSession>(targetId, out ICharacterSwitchSession target))
    target.BeginSwitchIn(new Pose(spawnPosition, spawnRotation));
// 服务缺失时跳过本次请求

// 请求当前角色退场
if (InstanceServiceHub.TryGet<ICharacterSwitchSession>(currentId, out ICharacterSwitchSession current))
    current.BeginSwitchOut();
```

### 锁定/解锁玩家操作

```csharp
using SPCharacter.Contract;
using SPFramework.Service;

if (InstanceServiceHub.TryGet<ICharacterSwitchSession>(characterId, out ICharacterSwitchSession session))
    session.SetOperationLocked(true); // 锁定；传入 false 解锁
```

### 订阅角色切换事实

```csharp
using System;

using SPCharacter.Contract;
using SPFramework.Event;

private IDisposable _switchInCompletedSubscription;

private void OnEnable()
    => _switchInCompletedSubscription = EventBus.Subscribe(CharacterEvents.SwitchInCompleted, OnSwitchInCompleted);

private void OnDisable()
{
    _switchInCompletedSubscription?.Dispose();
    _switchInCompletedSubscription = null;
}

private void OnSwitchInCompleted(CharacterSwitchInCompletedEvent payload)
{
    string characterId = payload.CharacterId; // 完成上场的角色 Id
}
```

## 三、反例

| 反例 | 正确做法 |
| --- | --- |
| 引用 `SPCharacter.Core` / `SPCharacter.Wiring` 中的类型 | 只引用 `SPCharacter.Contract`，经 `InstanceServiceHub` 获取会话接口 |
| 用模块级 `ModuleServiceHub.TryGet` 获取角色切换会话 | 该契约为**实例级**服务，用 `InstanceServiceHub.TryGet(id, out ...)` 按角色 Id 获取 |
| 调用 `BeginSwitchIn` / `BeginSwitchOut` 后立即假定动作已完成 | 异步语义，完成与否以 `SwitchInCompleted` / `SwitchOutCompleted` 事件为准 |
| 直接修改角色实例的 `Transform` 来模拟上场落位 | 调用 `BeginSwitchIn(pose)`，落位与上场动画由模块内部完成 |
| 假定角色会话必然可用，不处理 `TryGet` 失败 | 会话随角色实例启用/禁用而注册/注销，每次按需 `TryGet` 并处理失败 |
| 长期缓存会话接口并假设其永久有效 | 会话随实例生命周期变动，每次按需 `TryGet` |
| 订阅事件后不持有句柄或忘记 `Dispose` | 持有 `IDisposable` 句柄，并在失效时成对 `Dispose` |
