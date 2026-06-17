namespace SPPlayer
{
    /// <summary>
    /// Idle 状态
    /// </summary>
    public class IdleState : BaseState
    {
        /// <summary>
        /// 创建 Idle 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public IdleState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Idle;

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
