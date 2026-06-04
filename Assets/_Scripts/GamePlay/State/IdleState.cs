namespace GamePlay.State
{
    /// <summary>
    /// 待机状态：检测到输入时切换至 WalkState。
    /// 旋转锁定委托给 MotionDriver 管理。
    /// </summary>
    public class IdleState : StateBase
    {
        private const float CrossFadeDuration = 0.15f;

        /// <inheritdoc/>
        public override void Enter(IStateContext context)
        {
            Context = context;
            if (!IsInAnimatorState(Common.AnimationHashes.Idle))
                Context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.Idle, CrossFadeDuration);
            // Context.MotionDriver.SnapCurrentRotation();
        }

        /// <inheritdoc/>
        public override void Exit()
        {
        }

        /// <inheritdoc/>
        public override void Update()
        {
            if (Context.Blackboard.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState<WalkState>();
            }
        }

        /// <inheritdoc/>
        public override void LateUpdate()
        {
            //Context.MotionDriver.ApplyLockedRotation();
        }
    }
}
