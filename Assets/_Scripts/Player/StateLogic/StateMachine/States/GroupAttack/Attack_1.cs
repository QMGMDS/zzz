namespace SPPlayer
{
    /// <summary>
    /// Attack_1 状态——播放到 CancelThreshold 后可被下一攻击提前打断
    /// </summary>
    public class Attack_1 : AttackBaseState
    {
        /// <summary>
        /// 创建 Attack_1 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_1(PlayerController player) : base(player) { }

        #region BaseState

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_1;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (TryEarlyCancel()) return;
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_1_End));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }

        #endregion

        #region AttackBaseState

        /// <inheritdoc />
        protected override float AttackCancelThreshold => 0.5f;

        /// <inheritdoc />
        protected override PlayerStateType? CancelTargetStateType => PlayerStateType.Attack_2;

        #endregion
    }
}
