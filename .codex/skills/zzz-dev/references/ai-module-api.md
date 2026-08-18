# AI 模块说明

## 一、模块概述

AI 模块（`Module.SPAI` 程序集）负责敌人的感知、运行时决策状态与行为树任务接线。模块当前没有对外 `Contract/`，外部系统不直接调用 AI 内部类型；敌人实例通过 `AIBrain` 驱动，行为树通过内部任务读取代码黑板并描述决策流程。

模块目录职责如下：

| 目录 | 命名空间 | 职责 |
| --- | --- | --- |
| `Core/` | `SPAI.Core` | 运行时黑板、敌人配置与感知计算，不直接参与跨模块通信 |
| `Wiring/` | `SPAI.Wiring` | `AIBrain`，负责感知更新、服务获取与行为树接线 |
| `Wiring/Tasks/` | `SPAI.Wiring.Tasks` | Behavior Designer 条件节点与动作节点 |

模块依赖：

- `SPCharacter.Contract`：按角色 Id 获取 `ICharacterAgentSession`，提交移动、朝向与攻击意图。
- `SPTeam.Contract`：获取当前上场角色 Id 与目标实例变换。
- `SPFramework.Service`：通过 `ModuleServiceHub` 与 `InstanceServiceHub` 获取服务。
- `BehaviorDesigner.RuntimeSource`：运行行为树任务与组合节点。

模块内部的 `SPAI.Core`、`SPAI.Wiring` 与 `SPAI.Wiring.Tasks` 类型均为 `internal`，其他模块不得直接引用。AI 与其他模块的交互必须使用对方的 `Contract`。

### AIBrain

`AIBrain` 挂在敌人根物体上，是单个敌人实例的感知与接线中枢：

- `Awake` 校验 `EnemyConfigSO` 与角色 Id，并以自身出生位置初始化巡逻锚点。
- `Update` 每帧从 `ITeamService` 获取当前上场角色，按视野距离和视锥角更新目标感知。
- 目标可见时写入目标当前位置；目标暂时不可见时保留最后目击位置，仅将 `IsTargetVisible` 置为 `false`。
- 敌人离巡逻锚点超过 `MaxChaseDistance` 时，立即清除目标并进入脱战回归状态。
- `TryGetAgentSession` 按角色 Id 按需获取角色代理会话，不长期缓存服务接口。

当前感知只进行距离与水平视锥角判定，不进行遮挡物或射线检测。若需要加入遮挡判定，应扩展 AI 感知实现，不应把感知逻辑复制到行为树动作中。

## 二、代码运行时黑板

`AIRuntimeBlackboard` 是 AI 决策状态的唯一共享来源。Behavior Designer 的 Variables 面板不承载目标、可见性、巡逻点等 AI 运行时共享变量；自定义节点通过 `AIBrain.Blackboard` 读取或调用黑板方法写入。

| 数据 | 含义 | 主要写入时机 |
| --- | --- | --- |
| `AnchorPosition` | 敌人出生位置，也是巡逻锚点 | 初始化时写入 |
| `HasTarget` | 当前是否持有有效目标 | 发现目标或清除目标时变化 |
| `IsTargetVisible` | 当前目标是否处于视野内 | 感知更新时变化 |
| `IsReturning` | 是否处于脱战回归巡逻范围状态 | 清除目标或超出最大追击距离时置真，回归完成时置假 |
| `TargetPosition` | 当前目标位置；目标不可见时为最后目击位置 | 感知发现目标时更新，清除目标时归零 |
| `PatrolPoint` | 当前巡逻目标点 | 取点动作写入 |
| `HasLastPatrolPoint` | 是否存在上一巡逻点 | 首次取点后置真 |
| `LastPatrolPoint` | 上一次生成的巡逻点 | 每次取点时更新 |

黑板状态转换：

```text
发现目标        -> SetVisibleTarget(position)
目标暂时丢失    -> MarkTargetNotVisible()
完成追击或超出范围 -> ClearTargetAndBeginReturning()
回归巡逻范围    -> CompleteReturning()
生成巡逻点      -> SetPatrolPoint(point)
```

状态写入边界：

- 感知事实由 `AIBrain` 写入，行为节点不自行查询队伍或重复计算视野。
- 跨节点共享、影响决策的 AI 状态必须进入代码黑板，并通过明确的方法改变。
- 行为节点可以保留自身的短期执行控制数据，例如等待动作的计时器，但不得把目标或巡逻状态复制成 BD `SharedVariable`。
- 黑板属性使用私有 setter，新增状态应优先增加语义明确的方法，避免节点任意修改字段组合造成非法状态。

## 三、敌人配置

`EnemyConfigSO` 是敌人 AI 的静态配置资产，由 `AIBrain.Config` 提供给行为节点读取。配置按以下类别组织：

| 类别 | 配置 |
| --- | --- |
| 巡逻 | `PatrolRadius`、`PatrolArriveDistance`、`PatrolWaitSeconds`、`PatrolMinStepDistance` |
| 感知 | `ViewDistance`、`ViewAngle` |
| 追击 | `MaxChaseDistance`、`LastSeenWaitSeconds` |
| 攻击 | `AttackRange` |

配置属于敌人类型或实例的设计参数，运行时由代码读取。不要为了让 BD 节点显示参数而在行为树 Variables 面板中再建立一份同名配置；这样会产生两个来源，难以判断实际生效值。

## 四、行为树节点

Behavior Designer 只负责描述决策流程与节点组合。AI 运行时共享状态由代码黑板持有，节点不使用 BD `SharedVariable` 传递目标、可见性、巡逻点等数据。

### 条件节点

| 节点 | 通过条件 |
| --- | --- |
| `HasTargetCondition` | 黑板持有有效目标 |
| `IsTargetVisibleCondition` | 黑板持有目标且目标当前可见 |
| `TargetInAttackRangeCondition` | 目标可见且距离不大于攻击范围 |
| `IsReturningCondition` | 黑板处于脱战回归状态 |

### 动作节点

| 节点 | 职责 |
| --- | --- |
| `AttackAction` | 目标可见且在攻击范围内时停止移动、面向目标并逐帧请求攻击 |
| `ChaseTargetAction` | 目标可见时持续向目标当前位置移动 |
| `MoveToLastSeenTargetAction` | 目标不可见时移动到黑板中的最后目击位置 |
| `WaitForLastSeenTargetAction` | 抵达最后目击位置后按 `LastSeenWaitSeconds` 原地等待 |
| `ClearTargetAction` | 清除目标并进入脱战回归状态 |
| `PickPatrolPointAction` | 按巡逻配置生成新的巡逻点并写入黑板 |
| `MoveToPatrolPointAction` | 向当前巡逻点移动 |
| `WaitPatrolAction` | 抵达巡逻点后按 `PatrolWaitSeconds` 停留 |
| `ReturnToAnchorAction` | 向巡逻锚点移动，回到巡逻范围后结束 |
| `ClearReturningAction` | 完成脱战回归并清除回归状态 |

### Chase 分支组合

追击分支应将可见追击与最后目击点追击拆成两个行为树分支，并使用 BD 内置的 `Inverter` 与 `ReturnFailure`：

```text
Chase Sequence
├─ HasTargetCondition
└─ Selector（Conditional Abort = Both）
   ├─ Sequence
   │  ├─ IsTargetVisibleCondition
   │  └─ ChaseTargetAction
   └─ Sequence
      ├─ Inverter
      │  └─ IsTargetVisibleCondition
      ├─ MoveToLastSeenTargetAction
      ├─ WaitForLastSeenTargetAction
      └─ ReturnFailure
         └─ ClearTargetAction
```

`Conditional Abort = Both` 用于在目标重新出现或再次丢失时切换两个追击子分支。`ClearTargetAction` 本身返回成功，再由 `ReturnFailure` 将追击分支转换为失败，使上层 Selector 继续评估脱战回归分支。

典型的敌人顶层流程为：

```text
Selector
├─ Attack Sequence
├─ Chase Sequence
├─ Return Sequence
└─ Patrol Sequence
```

其中 Return 分支通常由 `IsReturningCondition`、`ReturnToAnchorAction` 与 `ClearReturningAction` 组成；Patrol 分支通常由 `PickPatrolPointAction`、`MoveToPatrolPointAction` 与 `WaitPatrolAction` 组成。优先级、Conditional Abort 和具体节点引用属于行为树资源配置，不由 AI 代码黑板承担。

## 五、跨模块调用

### 驱动角色

AI 不直接引用角色实现，而是通过 `AIBrain.TryGetAgentSession` 按角色 Id 获取 `ICharacterAgentSession`：

```csharp
if (_brain.TryGetAgentSession(out ICharacterAgentSession session))
{
    session.SetMoveAxis(worldDirection);       // 写入本帧移动意图
    session.SetFacingDirection(worldDirection); // 写入本帧朝向意图
    session.RequestAttack();                   // 写入本帧攻击意图
}
```

代理会话的写入是每帧语义，当帧有效，消费后清空。持续移动、持续面向或连续攻击必须由行为节点持续调用；不能缓存会话后等待角色模块主动执行。

### 获取目标

`AIBrain` 通过 `ModuleServiceHub.TryGet<ITeamService>` 获取队伍服务，再读取 `ActiveCharacterId` 与 `GetCharacterTransform`。队伍服务未注册、名册未初始化或目标实例不存在时，AI 按无目标处理。

AI 只依赖 `SPCharacter.Contract`、`SPTeam.Contract` 与框架服务，不得引用 `SPCharacter.Core`、`SPCharacter.Wiring`、`SPTeam.Core` 或 `SPTeam.Wiring`。

## 六、扩展约定

新增 AI 行为时按以下顺序判断：

1. 感知事实是否已经由 `AIBrain` 提供；如果已提供，节点直接读取黑板，不重复查询队伍或实现感知。
2. 数据是否需要跨节点共享或影响后续决策；如果需要，在 `AIRuntimeBlackboard` 中增加状态和语义方法，不添加 BD `SharedVariable`。
3. 是否能用现有 BD 组合节点表达流程；优先复用 `Selector`、`Sequence`、`Inverter`、`ReturnFailure` 等节点，再新增职责单一的自定义节点。
4. 新节点是否只负责一个动作或一个条件；不要在单个动作中重新建立大型状态机。
5. 是否需要跨模块能力；只通过目标模块的 `Contract` 与服务中心获取，不直接访问模块内部类。

常见错误：

| 错误做法 | 正确做法 |
| --- | --- |
| 在 BD Variables 中保存目标、目标位置、可见性或巡逻点 | 从 `AIBrain.Blackboard` 读取代码黑板 |
| 在多个节点中各自缓存一份目标状态 | 由 `AIBrain` 统一更新黑板，节点只读取 |
| 把可见追击、丢失追击、等待和清除目标写成一个状态机节点 | 拆为条件、动作和 BD 组合节点 |
| 在 Chase 节点内重复实现视野判定 | 读取 `AIBrain` 已更新的 `IsTargetVisible` |
| 直接查找角色组件或修改角色 Transform | 获取 `ICharacterAgentSession`，提交角色意图 |
| 缓存角色代理会话接口 | 每帧按需 `TryGetAgentSession`，处理服务缺失 |
| 为 `EnemyConfigSO` 的字段在 BD 中建立同名变量 | 统一从 `AIBrain.Config` 读取配置 |
