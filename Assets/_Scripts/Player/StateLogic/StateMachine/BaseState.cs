namespace SPPlayer
{
    /// <summary>
    /// 状态抽象基类——统一执行顺序：先强制拦截转移，再执行状态自身逻辑。
    /// 动画层通过 IntentionBlackboard 感知状态变化并自行播放对应动画。
    /// </summary>
    public abstract class BaseState
    {
        private readonly PlayerController _player;
        protected readonly PlayerBrain PlayerBrainBlackboard;

        /// <summary>
        /// 创建状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        protected BaseState(PlayerController player)
        {
            _player = player;
            PlayerBrainBlackboard = player.PlayerBrainBlackboard;
        }

        /// <summary>当前状态的枚举类型——子类必须覆写以在切换时通知黑板</summary>
        protected abstract PlayerStateType StateType { get; }

        /// <summary>
        /// 统一的 Enter 流程：
        /// 1) 将自身状态类型写入黑板（动画层据此自行切换动画）
        /// 2) 调用子类的 OnEnter 初始化逻辑
        /// </summary>
        public void Enter()
        {
            PlayerBrainBlackboard.CurrentPlayerState = StateType;
            OnEnter();
        }

        /// <summary>子类在此实现状态进入时的初始化逻辑（不包含动画播放）</summary>
        protected abstract void OnEnter();

        /// <summary>
        /// 统一的 LogicUpdate 流程：
        /// 1) 先检查全局强制转移（高优先级拦截器）
        /// 2) 再执行状态自身的逻辑
        /// </summary>
        public void LogicUpdate()
        {
            if (CheckInterrupts()) return;
            UpdateStateLogic();
        }

        /// <summary>
        /// 全局强制转移检测——通过拦截器管线解耦状态之间的硬依赖。
        /// </summary>
        private bool CheckInterrupts()
        {
            if (_player.MainInterceptor == null) return false;
            return _player.MainInterceptor.TryProcessInterrupts(this);
        }

        /// <summary>
        /// 状态自身的正常逻辑——子类在此实现核心行为。
        /// </summary>
        protected abstract void UpdateStateLogic();

        public abstract void PhysicsUpdate();
        public abstract void Exit();
    }
}
