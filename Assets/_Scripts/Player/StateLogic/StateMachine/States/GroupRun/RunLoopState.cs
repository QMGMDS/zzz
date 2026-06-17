namespace SPPlayer
{
    /// <summary>
    /// RunLoop 状态
    /// </summary>
    public class RunLoopState : BaseState
    {
        /// <summary>
        /// 创建 RunLoop 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public RunLoopState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.RunLoop;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic() { }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
