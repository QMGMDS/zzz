namespace SPPlayer
{
    /// <summary>
    /// EvadeBack 状态
    /// </summary>
    public class EvadeBackState : BaseState
    {
        /// <summary>
        /// 创建 EvadeBack 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeBackState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.EvadeBack;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            // 族内过渡：EvadeBack -> EvadeBackEnd
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.EvadeBackEnd));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
