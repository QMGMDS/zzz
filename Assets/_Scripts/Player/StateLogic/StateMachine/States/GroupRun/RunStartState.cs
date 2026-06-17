namespace SPPlayer
{
    /// <summary>
    /// RunStart 状态
    /// </summary>
    public class RunStartState : BaseState
    {
        /// <summary>
        /// 创建 RunStart 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public RunStartState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.RunStart;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            // 族内过渡：RunStart -> RunLoop
            if (PlayerBrainBlackboard.AnimationCompleted && PlayerBrainBlackboard.WantToMove)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.RunLoop));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
