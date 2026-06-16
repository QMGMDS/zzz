namespace SPPlayer
{
    /// <summary>
    /// WalkStart 状态——角色行走起步过渡状态
    /// </summary>
    public class WalkStartState : BaseState
    {
        /// <summary>
        /// 创建 WalkStart 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public WalkStartState(PlayerController player) : base(player) { }

        /// <summary>当前状态的枚举类型</summary>
        protected override PlayerStateType StateType => PlayerStateType.WalkStart;

        /// <summary>
        /// 进入 WalkStart 状态时的初始化逻辑
        /// </summary>
        protected override void OnEnter() { }

        /// <summary>
        /// 每帧状态逻辑更新——族内链式过渡。
        /// 当 WalkStart 动画播放完毕且玩家仍想移动时，自然过渡到 WalkLoop。
        /// </summary>
        protected override void UpdateStateLogic()
        {
            // 族内过渡：起步动画播完 + 仍想移动 → 行走循环
            if (PlayerBrainBlackboard.AnimationCompleted && PlayerBrainBlackboard.WantToMove)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.WalkLoop));
            }
        }

        /// <summary>
        /// 物理更新
        /// </summary>
        public override void PhysicsUpdate() { }

        /// <summary>
        /// 退出 WalkStart 状态时的清理逻辑
        /// </summary>
        public override void Exit() { }
    }
}
