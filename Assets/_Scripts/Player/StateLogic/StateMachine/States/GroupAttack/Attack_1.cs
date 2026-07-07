namespace SPPlayer
{
    /// <summary>
    /// Attack_1 状态
    /// </summary>
    public class Attack_1 : BaseState
    {
        /// <summary>
        /// 创建 Attack_1 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_1(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_1;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_1_End));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
