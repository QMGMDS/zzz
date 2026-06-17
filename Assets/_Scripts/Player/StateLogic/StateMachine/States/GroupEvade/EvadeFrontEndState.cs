namespace SPPlayer
{
    /// <summary>
    /// EvadeFrontEnd 状态
    /// </summary>
    public class EvadeFrontEndState : BaseState
    {
        /// <summary>
        /// 创建 EvadeFrontEnd 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public EvadeFrontEndState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.EvadeFrontEnd;

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
