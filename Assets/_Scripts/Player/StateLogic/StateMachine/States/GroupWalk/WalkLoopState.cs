namespace SPPlayer
{
    /// <summary>
    /// WalkLoop 状态——角色行走循环状态
    /// </summary>
    public class WalkLoopState : BaseState
    {
        /// <summary>
        /// 创建 WalkLoop 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public WalkLoopState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.WalkLoop;

        /// <summary>
        /// 进入 WalkLoop 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新
        /// </summary>
        protected override void UpdateStateLogic() { }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 WalkLoop 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
