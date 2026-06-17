namespace SPPlayer
{
    /// <summary>
    /// WalkLoop 状态
    /// </summary>
    public class WalkLoopState : BaseState
    {
        /// <summary>
        /// 创建 WalkLoop 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public WalkLoopState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.WalkLoop;

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
