# 队伍模块 API 速览

> 适用场景：读写队伍/切换相关代码、接入队伍服务、订阅队伍事件、排查切换时序与相机跟随问题。
> 边界约定：外部模块只允许引用 `SPTeam.Contract`（模块级服务 `ITeamService` 与事件 `TeamEvents`），不要引用 `SPTeam.Core` / `SPTeam.Wiring`。
> 架构定位：队伍是**编排型模块**——`Core` 只维护队伍数据（零外部依赖），切换相关跨模块协调（角色切换、相机跟随、输入监听）集中在 `Wiring` 层。
> 场景装配（资源实例化 → 名册移交 → 初始相机跟随）归编排层 `TeamAssemblyFlow`（SPFlow，见 framework-core.md 编排层章节），不属于本模块。

## 一、核心结构

### 命名空间分层

```csharp
// 外部模块可用
using SPTeam.Contract;
// 仅队伍模块内部使用
using SPTeam.Core;
using SPTeam.Wiring;
```

| 层 | 内容 | 职责 |
|---|---|---|
| `Contract` | `ITeamService`、`TeamEvents`、`TeamSlotPlan`、`TeamAssemblyEntry` | 公开契约：切换请求 + 状态查询 + 装配交接 + 事实广播 |
| `Core` | `TeamService`（数据层）、`TeamRoster`、`TeamSwitchCoordinator`、`TeamConfigSO` | 只维护名册与双锁状态，零外部命名空间依赖 |
| `Wiring` | `TeamWiring`（接线）、`TeamSwitchOrchestrator`（编排器） | 切换相关跨模块协调；编排器纯 C# 并实现 `ITeamService` |

> 与 Camera/Input/Resource（被消费型）不同：队伍的服务实现位于 `Wiring` 层而非 `Core`。调用方无需感知，`ModuleServiceHub.Get<ITeamService>()` 拿到的就是编排器。

### 公开契约：ITeamService（模块级服务）

```csharp
public interface ITeamService : IModuleService
{
    string ActiveCharacterId { get; }   // 当前上场角色 Id 名册未初始化时为空
    bool IsSwitching { get; }           // 切换双锁未全开时为真
    bool IsOperationLocked { get; }     // 目标入场完成前为真
    bool TryRequestSwitch();            // 顺序切换下一个角色 成功返回 true 未初始化返回 false
    IReadOnlyList<TeamSlotPlan> GetSlotPlan();                        // 装配计划 含配置校验
    void InitializeRoster(IReadOnlyList<TeamAssemblyEntry> entries);  // 移交装配结果
    Transform GetCharacterTransform(string characterId);              // 实例变换 未初始化或 Id 不存在返回 null
}
```

- 必选服务语义：`Start` 及之后 `Get<ITeamService>()` 直接用，不判空。
- 切换是"能力调用"（命令），不是事件；请求走服务方法，完成走事件。

### 公开契约：TeamEvents（事实广播）

| 事件 | 负载 | 发布时机 |
|---|---|---|
| `TeamEvents.ActiveCharacterChanged` | `TeamActiveCharacterChangedEvent(previous, current)` | 切换**发起时**（入场完成前） |
| `TeamEvents.SwitchLockChanged` | `TeamSwitchLockChangedEvent(isOperationLocked, isSwitchLocked)` | 锁状态变化时（带去重，实际发布两次：发起锁上 / 入场完成解锁） |

注意：`ActiveCharacterChanged` 发布时 `ITeamService.ActiveCharacterId` 仍返回旧角色，直到入场完成（`CompleteSwitchIn`）才更新——不要在事件回调里反查该属性做强一致判断。

## 二、运行时装配与接线

### 场景结构

```text
队伍根 GameObject:
  TeamService            // 数据层 持有 _config（TeamConfigSO）
  TeamWiring             // 接线胶水 _service 引用同物体 TeamService
    - Awake:  创建 TeamSwitchOrchestrator 并注册 ITeamService；缺 _service 抛异常
    - OnEnable/OnDisable: 订阅/退订角色切换完成事件与上场位姿应用事件
    - Update:  轮询 IProvideFrameInput.SwitchCharacter.IsPressed → TryRequestSwitch
  TeamAssemblyFlow       // 场景装配流程（SPFlow）同样挂在队伍根物体上
    - Start:  读装配计划 GetSlotPlan → 资源实例化 → InitializeRoster 移交名册 → 初始相机跟随
```

- 执行顺序：`TeamWiring` 与 `TeamService` 均为 `[DefaultExecutionOrder(-350)]`；输入服务 `-390`、资源/相机服务 `-380` 先注册，`Start` 取用安全。装配流程在 `Start` 运行（所有 `Awake` 之后），`Update` 首次轮询切换时名册已就绪；未初始化期间 `TryRequestSwitch` 直接返回 false 兜底。
- 装配失败 fail fast：配置校验前置（`GetSlotPlan` 先于实例化抛异常）；角色实例化失败由流程释放已建句柄后**抛异常**，不静默降级。
- 实例化参数：`shouldActivateAfterCreate: false`，仅初始角色被激活；其余角色停留在实例化位置（队伍根位置）直到切换。

### TeamConfigSO（配置资产）

创建菜单：`SPTeam/Team Config`

| 字段 | 含义 |
|---|---|
| `_slotCount` | 队伍角色数量，范围 1–3；必须等于槽位列表数量 |
| `_initialIndex` | 初始上场槽位索引（从 0 开始） |
| `_slots` | 槽位列表：`_characterId`（与角色实例服务 Id 一致）+ `_resourceKey`（string，与资源目录键完全一致） |

> 槽位键是普通 `string`（非 `ResourceKey` 强类型），格式校验在装配期由资源服务解析失败暴露。修改 `.asset` 请在 Inspector 操作，不要手改 YAML。

## 三、切换流程（时序）

```text
TeamWiring.Update:
  CanRequestSwitch 通过
  nextRoot.SetActive(true)
  CanBeginSwitch 预检（两个角色的 ICharacterSwitchService 均注册）
  TryCommitSwitch 提交双锁（数据层）
  BeginSwitch：锁两角色操作 + BeginSwitchOut + BeginSwitchIn(pose)（仅请求，不落位）
  发布 ActiveCharacterChanged / SwitchLockChanged（相机不动）

角色驱动帧:
  ApplySwitchInPose 落位（SetPositionAndRotation）
  发布 CharacterSwitchEvents.SwitchInPoseApplied
  → Team 校验 SwitchInCharacterId 后 SetCameraFollowTarget（相机此刻才切换）

完成阶段:
  SwitchInCompleted  → CompleteSwitchIn → 解锁入场角色 → SwitchLockChanged(解锁)
  SwitchOutCompleted → CompleteSwitchOut → SetActive(false) 隐藏退场角色 → SwitchLockChanged
```

### 关键坑：相机跟随时机

- `BeginSwitchIn(pose)` 是**异步请求**：落位由角色模块在后续驱动帧执行，调用返回时目标角色**尚未就位**。
- **不要在 `TryRequestSwitch` 里立即 `SetCameraFollowTarget(nextRoot.transform)`**——目标还停留在失活期间的位置（队伍根位置），平滑跟随会导致镜头大幅偏移。
- 正确做法：订阅 `CharacterSwitchEvents.SwitchInPoseApplied`（落位完成事实），由编排器在落位后切换相机；编排器会校验事件角色确为当前入场角色（`TeamService.SwitchInCharacterId`）。

## 四、使用模式

### 请求切换（输入/UI/AI 通用入口）

```csharp
ITeamService team = ModuleServiceHub.Get<ITeamService>();
if (team.TryRequestSwitch()) { /* 已发起 */ }
```

### 订阅队伍事件

```csharp
private IDisposable _subscription;

private void OnEnable()
{
    _subscription = EventBus.Subscribe(TeamEvents.ActiveCharacterChanged, OnActiveChanged);
}

private void OnDisable()
{
    _subscription?.Dispose();
    _subscription = null;
}

private void OnActiveChanged(TeamActiveCharacterChangedEvent payload) { /* 只读事实 */ }
```

- 订阅必须 `OnEnable`/`OnDisable` 成对；纯 C# 类在 `Dispose` 退订。
- 事件负载只补全事实，不要通过事件要求队伍做动作（那是 `TryRequestSwitch` 的职责）。

## 五、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| 外部模块 `using SPTeam.Core` | 只引用 `SPTeam.Contract` + 服务/事件 | Core 是实现层，外部只依赖契约 |
| 在 `Awake` 里 `Get<ITeamService>()` 并调用 | `Start` 及之后取用 | 服务注册在 `Awake`，装配在 `Start` |
| `TryRequestSwitch` 后立即 `SetCameraFollowTarget` | 订阅 `SwitchInPoseApplied` 后切相机 | 落位是异步的，提前切会镜头偏移 |
| 给 `TeamService`（Core）加跨模块逻辑 | 放 `TeamSwitchOrchestrator`（Wiring） | Core 保持纯数据，编排归 Wiring |
| 在装配流程之外调用 `InitializeRoster` / 直接给 `TeamService` 传名册 | 装配统一走 `TeamAssemblyFlow` | 名册移交是装配流程步骤，模块只提供能力 |
| 手改 `TeamConfig.asset` YAML | Inspector 配置 | 键类型为 string，避免序列化错配 |
| 把切换请求发成事件 | 调用 `ITeamService.TryRequestSwitch()` | 事件是事实不是命令 |
| 订阅 `TeamEvents` 后在回调里反查 `ActiveCharacterId` 做强一致判断 | 以事件负载为准 | 事件先于状态更新发布 |

## 六、交叉引用

| 相关文档 | 用途 |
|---|---|
| [framework-core.md](framework-core.md) | 访问级别语义、模块服务/实例服务、事件总线、编排层（Flow）约定与启用判据 |
| [character-module-api.md](character-module-api.md) | `ICharacterSwitchService` 实例服务与切换完成事件、状态意图 |
| [input-module-api.md](input-module-api.md) | 切换按键输入 `ProcessedFrameInput.SwitchCharacter` |
| [camera-module-api.md](camera-module-api.md) | `ISetCameraFollowTarget` 相机跟随能力 |
| [resource-module-api.md](resource-module-api.md) | 角色预制体实例化与句柄释放 |

> 建议顺序：先看 `framework-core.md`，再看角色与队伍两份模块文档；排查切换问题以本页"三、切换流程"时序为准。