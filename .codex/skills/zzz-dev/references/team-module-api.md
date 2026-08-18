# 队伍模块 API

## 一、模块概述

队伍模块（`Module.SPTeam` 程序集）对外提供一项**模块级服务**与两组事实广播，契约定义在 `SPTeam.Contract` 命名空间：

| 契约 | 形式 | 能力 |
| --- | --- | --- |
| `ITeamService` | 模块级服务 | 队伍角色切换请求、切换状态查询、装配计划获取与装配结果移交 |
| `TeamEvents.ActiveCharacterChanged` | 事件 | 当前上场角色变化 |
| `TeamEvents.SwitchLockChanged` | 事件 | 队伍切换锁状态变化 |

边界约定：外部只允许引用 `SPTeam.Contract`，通过 `ModuleServiceHub.TryGet` 获取服务、经 `EventBus.Subscribe` 订阅事件；`SPTeam.Core` 与 `SPTeam.Wiring` 中的类型均为 `internal`，编译期对外不可见。

### ITeamService（模块级服务）

```csharp
public interface ITeamService : IModuleService
{
    string ActiveCharacterId { get; }
    bool IsSwitching { get; }
    bool IsOperationLocked { get; }
    bool TryRequestSwitch();
    IReadOnlyList<TeamSlotPlan> GetSlotPlan();
    void InitializeRoster(IReadOnlyList<TeamAssemblyEntry> entries);
    Transform GetCharacterTransform(string characterId);
}
```

状态属性：

- `ActiveCharacterId`：当前上场角色 Id；名册未初始化时为 `null`。
- `IsSwitching`：是否处于切换中——切换双锁未全开时为真；名册未初始化时为 `false`。
- `IsOperationLocked`：是否锁定玩家操作——目标入场完成前为真；名册未初始化时为 `false`。

方法：

- `TryRequestSwitch()`：请求按槽位顺序切换到下一个角色，返回切换是否成功发起。成功发起为异步语义——返回 `true` 仅代表切换会话已开启，推进进度以事件为准；名册未初始化、切换进行中或目标角色不可用时返回 `false`。成功发起时会立即广播 `Team.ActiveCharacterChanged` 并刷新锁状态。
- `GetSlotPlan()`：获取队伍装配计划——校验配置后返回按切换顺序排列的槽位清单；配置缺失或校验失败时抛 `InvalidOperationException`。
- `InitializeRoster(entries)`：移交队伍装配结果——登记名册并激活初始角色。`entries` 必须与装配计划一一对应（数量相等、顺序一致、各项 `CharacterId` 与对应槽位逐一匹配），否则抛 `InvalidOperationException`。**移交后实例所有权归模块**：各项的 `Release` 委托由模块持有，需要释放实例时由模块调用，调用方移交后不得再自行销毁或释放这些实例。
- `GetCharacterTransform(characterId)`：获取指定角色的实例变换；名册未初始化或 Id 不存在时返回 `null`。

### 装配数据类型

```csharp
public readonly struct TeamSlotPlan
{
    public TeamSlotPlan(string characterId, string resourceKey);
    public string CharacterId { get; } // 角色唯一标识
    public string ResourceKey { get; } // 角色资源键
}

public readonly struct TeamAssemblyEntry
{
    public TeamAssemblyEntry(string characterId, GameObject instance, Action release);
    public string CharacterId { get; }  // 角色唯一标识
    public GameObject Instance { get; } // 角色实例对象
    public Action Release { get; }      // 释放实例的委托
}
```

- `TeamSlotPlan`：装配计划槽位，描述单个角色的装配需求。`CharacterId` 为该角色的唯一标识，须与移交时实例的标识一致；`ResourceKey` 为该角色的资源键，供调用方按键装配实例。
- `TeamAssemblyEntry`：装配结果项，单个角色的实例化结果移交。`CharacterId` 须与装配计划对应槽位一致；`Instance` 为已装配出的角色实例对象；`Release` 为释放该实例的委托。

### 事件

```csharp
public static class TeamEvents
{
    // 事件标识名："Team.ActiveCharacterChanged"
    public static readonly EventKey<TeamActiveCharacterChangedEvent> ActiveCharacterChanged;

    // 事件标识名："Team.SwitchLockChanged"
    public static readonly EventKey<TeamSwitchLockChangedEvent> SwitchLockChanged;
}

public readonly struct TeamActiveCharacterChangedEvent
{
    public TeamActiveCharacterChangedEvent(string previousCharacterId, string currentCharacterId);
    public string PreviousCharacterId { get; } // 切换前的角色 Id
    public string CurrentCharacterId { get; }  // 切换后的角色 Id
}

public readonly struct TeamSwitchLockChangedEvent
{
    public TeamSwitchLockChangedEvent(bool isOperationLocked, bool isSwitchLocked);
    public bool IsOperationLocked { get; } // 是否锁定玩家操作
    public bool IsSwitchLocked { get; }    // 是否处于切换中
}
```

事件语义：

- `Team.ActiveCharacterChanged`：一次切换**成功发起时立即广播**（不等切换流程完成），负载给出切换前后的角色 Id。
- `Team.SwitchLockChanged`：`IsOperationLocked` 或 `IsSwitchLocked` 任一发生变化时广播；状态未变化时不重复广播。负载中的 `IsSwitchLocked` 与服务属性 `IsSwitching` 同源。
- 切换会话设有超时兜底：会话超过配置时限仍未完成时，模块强制收尾并复位锁状态（同时输出 `LogWarning`），订阅方总会收到收尾后的锁状态广播，无需处理永久卡死。

## 二、API 调用示例

获取服务统一使用 `ModuleServiceHub.TryGet`。服务未注册或已销毁时返回 `false` 且 `out` 结果为 `null`，调用方必须自行降级，不可默认服务必然可用。

### 查询状态并请求切换

```csharp
using SPFramework.Service;
using SPTeam.Contract;

if (ModuleServiceHub.TryGet<ITeamService>(out ITeamService team))
{
    string activeId = team.ActiveCharacterId;   // 名册未初始化时为 null

    if (!team.IsSwitching && team.TryRequestSwitch())
    {
        // 切换已成功发起，推进进度以 Team.SwitchLockChanged 等事件为准
    }
}
// 服务缺失时跳过本次请求
```

### 装配流程：获取计划并移交结果

```csharp
using System.Collections.Generic;

using UnityEngine;

using SPFramework.Service;
using SPTeam.Contract;

if (ModuleServiceHub.TryGet<ITeamService>(out ITeamService team))
{
    IReadOnlyList<TeamSlotPlan> plan = team.GetSlotPlan(); // 配置无效时抛 InvalidOperationException

    var entries = new List<TeamAssemblyEntry>(plan.Count);
    foreach (TeamSlotPlan slot in plan)
    {
        // 调用方按 slot.ResourceKey 装配角色实例（装配手段由调用方决定）
        GameObject instance = InstantiateByKey(slot.ResourceKey);
        entries.Add(new TeamAssemblyEntry(slot.CharacterId, instance, () => ReleaseByKey(instance)));
    }

    team.InitializeRoster(entries); // 数量/顺序/Id 与计划不符时抛 InvalidOperationException
}
```

### 读取角色实例变换

```csharp
using SPFramework.Service;
using SPTeam.Contract;

if (ModuleServiceHub.TryGet<ITeamService>(out ITeamService team))
{
    Transform characterTransform = team.GetCharacterTransform(characterId); // 未初始化或 Id 不存在时为 null
    if (characterTransform != null)
    {
        // 使用变换
    }
}
```

### 订阅队伍事实

```csharp
using System;

using SPFramework.Event;
using SPTeam.Contract;

private IDisposable _activeChangedSubscription;
private IDisposable _lockChangedSubscription;

private void OnEnable()
{
    _activeChangedSubscription = EventBus.Subscribe(TeamEvents.ActiveCharacterChanged, OnActiveCharacterChanged);
    _lockChangedSubscription = EventBus.Subscribe(TeamEvents.SwitchLockChanged, OnSwitchLockChanged);
}

private void OnDisable()
{
    _activeChangedSubscription?.Dispose();
    _lockChangedSubscription?.Dispose();
    _activeChangedSubscription = null;
    _lockChangedSubscription = null;
}

private void OnActiveCharacterChanged(TeamActiveCharacterChangedEvent payload)
{
    // payload.PreviousCharacterId -> payload.CurrentCharacterId
}

private void OnSwitchLockChanged(TeamSwitchLockChangedEvent payload)
{
    // payload.IsOperationLocked / payload.IsSwitchLocked
}
```

## 三、反例

| 反例 | 正确做法 |
| --- | --- |
| 引用 `SPTeam.Core` / `SPTeam.Wiring` 中的类型 | 只引用 `SPTeam.Contract`，经 `ModuleServiceHub` 获取服务 |
| 不判空直接调用服务，默认其必然可用 | 用 `TryGet` 获取，失败时按上文示例降级 |
| `TryRequestSwitch` 返回 `true` 后立即假定切换已完成 | 切换为异步推进，进度以 `Team.SwitchLockChanged` 等事件为准 |
| 名册未初始化时请求切换或查询角色变换，并假定返回有效 | 初始化前 `TryRequestSwitch` 返回 `false`、`GetCharacterTransform` 返回 `null`、`ActiveCharacterId` 为 `null`；先完成 `InitializeRoster` |
| 移交 `InitializeRoster` 后仍自行销毁/释放已移交的实例 | 实例所有权随移交归模块，释放一律由模块通过移交的 `Release` 委托进行 |
| `InitializeRoster` 传入与装配计划不一致的列表（数量、顺序、Id 不符） | 严格按 `GetSlotPlan()` 返回的顺序逐项构造 entries |
| 长期缓存服务接口并假设其永久有效 | 服务随模块内部的注册/注销生命周期变动，每次按需 `TryGet` |
| 订阅事件后不持有句柄或忘记 `Dispose` | 持有 `IDisposable` 句柄，并在失效时成对 `Dispose` |
