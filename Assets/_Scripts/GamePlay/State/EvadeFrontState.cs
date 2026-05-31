using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 前闪避状态：播 EvadeFront 动画。CD 时间内不可被移动或其他状态打断，
    /// CD 结束后有输入则切入 RunState，无输入等动画播完（normalizedTime ≥ 0.95）后切 IdleState。
    /// </summary>
    public class EvadeFrontState : StateBase
    {
        private const float CrossFadeDuration = 0.1f;
        private const float NaturalExitThreshold = 0.95f;

        private float _animEnterTime;
        private bool _hasEnteredAnimState;

        /// <inheritdoc/>
        public override void Enter(IStateContext context)
        {
            Context = context;
            Context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.EvadeFront, CrossFadeDuration);
            _hasEnteredAnimState = false;
        }

        /// <inheritdoc/>
        public override void Exit()
        {
        }

        /// <inheritdoc/>
        public override void Update()
        {
            AnimatorStateInfo stateInfo = Context.Animator.GetCurrentAnimatorStateInfo(0);

            if (!_hasEnteredAnimState)
            {
                if (stateInfo.shortNameHash == Common.AnimationHashes.EvadeFront)
                {
                    _hasEnteredAnimState = true;
                    _animEnterTime = Time.time;
                }

                return;
            }

            if (stateInfo.shortNameHash != Common.AnimationHashes.EvadeFront)
                return;

            if (Time.time - _animEnterTime < Context.EvadeFrontCommitDuration)
                return;

            if (Context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState<RunState>();
                return;
            }

            if (stateInfo.normalizedTime >= NaturalExitThreshold)
                Context.StateMachine.ChangeState<IdleState>();
        }
    }
}
