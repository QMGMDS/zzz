namespace SPPlayer
{
    /// <summary>
    /// EvadeFront 状态
    /// </summary>
    public class EvadeFrontState : BaseState
    {
        /// <summary>
        /// 创建 EvadeFront 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeFrontState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.EvadeFront;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            // 族内过渡：EvadeFront -> EvadeFrontEnd
            if (PlayerBrainBlackboard.AnimationCompleted && !PlayerBrainBlackboard.WantToMove)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.EvadeFrontEnd));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
