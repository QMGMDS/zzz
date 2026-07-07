namespace SPPlayer
{
    /// <summary>
    /// Attack_4_Normal 状态——播放到 CancelThreshold 后可被下一攻击提前打断
    /// </summary>
    public class Attack_4_Normal : AttackBaseState
    {
        /// <summary>
        /// 创建 Attack_4_Normal 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_4_Normal(PlayerController player) : base(player) { }

        #region BaseState

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_4_Normal;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (TryEarlyCancel()) return;
            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_4_Normal_End));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }

        #endregion

        #region AttackBaseState

        /// <inheritdoc />
        protected override float AttackCancelThreshold => 0.9f;

        /// <inheritdoc />
        protected override PlayerStateType? CancelTargetStateType => PlayerStateType.Attack_1;

        #endregion
    }
}
