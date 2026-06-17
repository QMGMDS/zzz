namespace SPPlayer
{
    /// <summary>
    /// Stop 状态
    /// </summary>
    public class StopState : BaseState
    {
        /// <summary>
        /// 创建 Stop 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public StopState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Stop;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            // 族内过渡：Stop -> Idle
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Idle));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
