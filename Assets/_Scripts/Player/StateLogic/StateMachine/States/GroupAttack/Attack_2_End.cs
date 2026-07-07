namespace SPPlayer
{
    /// <summary>
    /// Attack_2_End 状态
    /// </summary>
    public class Attack_2_End : BaseState
    {
        /// <summary>
        /// 创建 Attack_2_End 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_2_End(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_2_End;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.WantToAttack)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_3));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
