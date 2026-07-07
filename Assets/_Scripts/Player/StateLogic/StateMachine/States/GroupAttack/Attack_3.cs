namespace SPPlayer
{
    /// <summary>
    /// Attack_3 状态
    /// </summary>
    public class Attack_3 : BaseState
    {
        /// <summary>
        /// 创建 Attack_3 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_3(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_3;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_3_End));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
