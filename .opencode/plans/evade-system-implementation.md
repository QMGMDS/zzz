# 闪避系统实施方案

## 实施步骤

共需修改 **4** 个文件，新建 **6** 个文件。

---

## 第一步：修改 `PlayerStateType` 枚举

**文件**: `Assets/_Scripts/Player/StateLogic/PlayerStateType.cs`

将：
```csharp
        // 闪避族状态
        Evade,
```
替换为：
```csharp
        // 闪避族状态 — 分布在各宿主族
        EvadeFront,
        EvadeFrontEnd,
        EvadeBack,
        EvadeBackEnd,
```

---

## 第二步：新建 `EvadeInputProcessor`

**文件**: `Assets/_Scripts/Player/Input/Processor/SubProcessors/EvadeInputProcessor.cs`

```csharp
namespace SPPlayer
{
    /// <summary>
    /// 闪避输入意图翻译器
    /// </summary>
    public class EvadeInputProcessor : IInputProcessor
    {
        /// <summary>
        /// 将处理后的输入翻译为闪避意图，写入黑板
        /// </summary>
        /// <param name="current">当前帧处理后的输入数据</param>
        /// <param name="last">上一帧处理后的输入数据</param>
        /// <param name="blackboard">玩家大脑黑板</param>
        public void UpdateIntentionTranslation(in ProcessedInputData current, in ProcessedInputData last, PlayerBrain blackboard)
        {
            if (current.EvadePressed)
                blackboard.WantToEvade = true;
        }
    }
}
```

---

## 第三步：注册 `EvadeInputProcessor`

**文件**: `Assets/_Scripts/Player/Input/Processor/InputMainProcessor.cs`

将 `_processors` 列表：
```csharp
            _processors = new List<IInputProcessor>
            {
                new MoveInputProcessor(),
            };
```
修改为：
```csharp
            _processors = new List<IInputProcessor>
            {
                new MoveInputProcessor(),
                new EvadeInputProcessor(),
            };
```

---

## 第四步：新建 4 个 Evade 状态类

### 4.1 EvadeFrontState（奔跑族）

**文件**: `Assets/_Scripts/Player/StateLogic/StateMachine/States/GroupRun/EvadeFrontState.cs`

```csharp
namespace SPPlayer
{
    /// <summary>
    /// EvadeFront 状态——角色前闪避状态（奔跑族成员）
    /// </summary>
    public class EvadeFrontState : BaseState
    {
        /// <summary>
        /// 创建 EvadeFront 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeFrontState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.EvadeFront;

        /// <summary>
        /// 进入 EvadeFront 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新——自身不处理转移，由 StopInterceptor 在动画播完后路由到 EvadeFrontEnd。
        /// </summary>
        protected override void UpdateStateLogic() { }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 EvadeFront 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
```

### 4.2 EvadeFrontEndState（行走族）

**文件**: `Assets/_Scripts/Player/StateLogic/StateMachine/States/GroupWalk/EvadeFrontEndState.cs`

```csharp
namespace SPPlayer
{
    /// <summary>
    /// EvadeFrontEnd 状态——前闪避后收招状态（行走族成员）
    /// </summary>
    public class EvadeFrontEndState : BaseState
    {
        /// <summary>
        /// 创建 EvadeFrontEnd 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeFrontEndState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.EvadeFrontEnd;

        /// <summary>
        /// 进入 EvadeFrontEnd 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新——自身不处理转移，由 StopInterceptor/WalkStartInterceptor 路由回常规状态。
        /// </summary>
        protected override void UpdateStateLogic() { }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 EvadeFrontEnd 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
```

### 4.3 EvadeBackState（行走族）

**文件**: `Assets/_Scripts/Player/StateLogic/StateMachine/States/GroupWalk/EvadeBackState.cs`

```csharp
namespace SPPlayer
{
    /// <summary>
    /// EvadeBack 状态——角色后闪避状态（行走族成员）
    /// </summary>
    public class EvadeBackState : BaseState
    {
        /// <summary>
        /// 创建 EvadeBack 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeBackState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.EvadeBack;

        /// <summary>
        /// 进入 EvadeBack 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新——族内链式过渡。
        /// 当 EvadeBack 动画播放完毕时，自然过渡到 EvadeBackEnd。
        /// </summary>
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.EvadeBackEnd));
            }
        }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 EvadeBack 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
```

### 4.4 EvadeBackEndState（行走族）

**文件**: `Assets/_Scripts/Player/StateLogic/StateMachine/States/GroupWalk/EvadeBackEndState.cs`

```csharp
namespace SPPlayer
{
    /// <summary>
    /// EvadeBackEnd 状态——后闪避后收招状态（行走族成员）
    /// </summary>
    public class EvadeBackEndState : BaseState
    {
        /// <summary>
        /// 创建 EvadeBackEnd 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeBackEndState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.EvadeBackEnd;

        /// <summary>
        /// 进入 EvadeBackEnd 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新——自身不处理转移，由 StopInterceptor/WalkStartInterceptor 路由回常规状态。
        /// </summary>
        protected override void UpdateStateLogic() { }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 EvadeBackEnd 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
```

---

## 第五步：修改 `StopInterceptorSO`

**文件**: `Assets/_Scripts/Player/StateLogic/Interceptor/SubInterceptors/StopInterceptorSO.cs`

将整段 `TryIntercept` 方法替换为：

```csharp
        /// <summary>
        /// 尝试拦截——检测玩家是否松开了移动输入，若是则从行走族/闪避收招路由到 Stop 或 EvadeEnd。
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <param name="currentState">当前激活的状态</param>
        /// <param name="nextState">输出参数 : 拦截成功后要切换到的目标状态</param>
        /// <returns>true = 拦截成功</returns>
        public override bool TryIntercept(PlayerController player, BaseState currentState, out BaseState nextState)
        {
            nextState = null;

            var blackboard = player.PlayerBrainBlackboard;
            if (blackboard == null) return false;

            var stateType = blackboard.CurrentPlayerState;

            // 豁免检查
            if (IsExempt(stateType)) return false;

            if (!blackboard.WantToMove)
            {
                // EvadeFront（奔跑族）→ EvadeFrontEnd（行走族）：跨族转移由 StopInterceptor 代理
                if (stateType == PlayerStateType.EvadeFront && blackboard.AnimationCompleted)
                {
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeFrontEnd);
                    return nextState != null;
                }

                nextState = player.StateMachine.GetState(PlayerStateType.Stop);
                return nextState != null;
            }

            return false;
        }
```

> **注意**：将 EvadeBack 加入 StopInterceptor 的豁免清单（在 Unity Editor 中操作 SO 资产），否则后闪会被误路由到 Stop。

---

## 第六步：注册 4 个新状态到 `StateMachine`

**文件**: `Assets/_Scripts/Player/StateLogic/StateMachine/StateMachine.cs`

将 `_states` 字典：
```csharp
            _states = new Dictionary<PlayerStateType, BaseState>
            {
                { PlayerStateType.Idle,new IdleState(_player) },
                { PlayerStateType.WalkStart, new WalkStartState(_player) },
                { PlayerStateType.WalkLoop, new WalkLoopState(_player) },
                { PlayerStateType.Stop, new StopState(_player) },
            };
```
修改为：
```csharp
            _states = new Dictionary<PlayerStateType, BaseState>
            {
                { PlayerStateType.Idle,new IdleState(_player) },
                { PlayerStateType.WalkStart, new WalkStartState(_player) },
                { PlayerStateType.WalkLoop, new WalkLoopState(_player) },
                { PlayerStateType.Stop, new StopState(_player) },
                { PlayerStateType.EvadeFront, new EvadeFrontState(_player) },
                { PlayerStateType.EvadeFrontEnd, new EvadeFrontEndState(_player) },
                { PlayerStateType.EvadeBack, new EvadeBackState(_player) },
                { PlayerStateType.EvadeBackEnd, new EvadeBackEndState(_player) },
            };
```

---

## 第七步：在 Unity Editor 中配置

1. 右键 `Assets/Data/Player/SubInterceptors/` → Create → Player → Interceptors → EvadeInterceptor，创建 `EvadeInterceptor.asset`
2. 选中 `PlayerInterceptorConfig.asset`，将 `EvadeInterceptor` 拖入 `_globalInterceptors` 数组**最前面**（优先级最高）
3. 配置各拦截器的豁免清单：

| 拦截器 SO | 豁免的状态 |
|-----------|-----------|
| EvadeInterceptor | EvadeFront, EvadeBack, EvadeFrontEnd, EvadeBackEnd |
| WalkStartInterceptor | EvadeFront, EvadeBack |
| StopInterceptor | EvadeBack, EvadeBackEnd（注：EvadeFront **不豁免**，EvadeFrontEnd **不豁免**） |

> `EvadeFront` 不豁免于 StopInterceptor，因为后者需要负责 EvadeFront→EvadeFrontEnd 的跨族路由。
> `EvadeFrontEnd` 不豁免于 StopInterceptor，因为后者需要负责 EvadeFrontEnd→Stop 的回归路由。

---

## 转移流程图

```
                    EvadeInterceptor
              (WantToEvade + Move → Front)
              (WantToEvade + !Move → Back)
                   /               \
            EvadeFront(奔)      EvadeBack(行)
                |                    | (族内直接)
         StopInterceptor       EvadeBackEnd(行)
    (AnimComplete + !Move)          |
                |         StopInterceptor/WalkStartInterceptor
         EvadeFrontEnd(行)          |
                |              Stop / WalkStart
    StopInterceptor/WalkStartInterceptor
                |
        Stop / WalkStart
```

## 文件清单

| 操作 | 文件 |
|------|------|
| 修改 | `PlayerStateType.cs` |
| 修改 | `InputMainProcessor.cs` |
| 修改 | `StopInterceptorSO.cs` |
| 修改 | `StateMachine.cs` |
| 新建 | `EvadeInputProcessor.cs` |
| 新建 | `EvadeInterceptorSO.cs` |
| 新建 | `EvadeFrontState.cs` |
| 新建 | `EvadeFrontEndState.cs` |
| 新建 | `EvadeBackState.cs` |
| 新建 | `EvadeBackEndState.cs` |
