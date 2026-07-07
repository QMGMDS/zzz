namespace SPPlayer
{
    /// <summary>
    /// Attack_2 状态
    /// </summary>
    public class Attack_2 : BaseState
    {
        /// <summary>
        /// 创建 Attack_2 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_2(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_2;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_2_End));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
