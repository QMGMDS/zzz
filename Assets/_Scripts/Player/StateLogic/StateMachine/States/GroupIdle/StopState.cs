namespace SPPlayer
{
    /// <summary>
    /// Stop 状态——角色行走/奔跑下停止过渡状态
    /// </summary>
    public class StopState : BaseState
    {
        /// <summary>
        /// 创建 Stop 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public StopState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.Stop;

        /// <summary>
        /// 进入 Stop 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新——族内链式过渡。
        /// 当 Stop 动画播放完毕时，自然过渡到 Idle。
        /// </summary>
        protected override void UpdateStateLogic()
        {
            // 族内过渡：停止动画播完 → 回到 Idle
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Idle));
            }
        }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 Stop 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
