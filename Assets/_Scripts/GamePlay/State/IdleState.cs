using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 待机状态：检测到输入时切换至 WalkState，LateUpdate 中锁定旋转
    /// </summary>
    public class IdleState : StateBase
    {
        private const float CrossFadeDuration = 0.15f;

        private Quaternion _lockedRotation;

        /// <inheritdoc/>
        public override void Enter(IStateContext context)
        {
            Context = context;
            if (!IsInAnimatorState(Common.AnimationHashes.Idle))
                Context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.Idle, CrossFadeDuration);
            _lockedRotation = Context.Transform.rotation;
        }

        /// <inheritdoc/>
        public override void Exit()
        {
        }

        /// <inheritdoc/>
        public override void Update()
        {
            if (Context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState<WalkState>();
            }
        }

        /// <inheritdoc/>
        public override void LateUpdate()
        {
            Context.Transform.rotation = _lockedRotation;
        }
    }
}
