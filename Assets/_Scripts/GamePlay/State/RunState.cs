using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 跑步状态：仅通过 Evade 键进入。松手后播 RunEnd→Idle，停止期间检测到移动输入则切入 WalkState。
    /// 要重新跑必须再次触发 Evade（经 EvadeFrontState）。
    /// </summary>
    public class RunState : StateBase
    {
        private const float CrossFadeDuration = 0.10f;
        private const float StopCrossFadeDuration = 0.15f;

        private float _noInputTimer;
        private bool _isStopping;

        /// <inheritdoc/>
        public override void Enter(IStateContext context) 
        {
            Context = context;
            Context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.RunStart, CrossFadeDuration);
            _noInputTimer = 0f;
            _isStopping = false;
        }

        /// <inheritdoc/>
        public override void Exit()
        {
        }

        /// <inheritdoc/>
        public override void Update()
        {
            Vector2 direction = Context.Blackboard.MoveDirection;

            if (_isStopping)
            {
                if (direction.sqrMagnitude > 0.0001f)
                {
                    Context.StateMachine.ChangeState<WalkState>();
                    return;
                }

                if (IsInAnimatorState(Common.AnimationHashes.Idle))
                {
                    Context.StateMachine.ChangeState<IdleState>();
                }

                return;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                _noInputTimer += Time.deltaTime;
                if (_noInputTimer >= Context.Blackboard.InputBufferTime)
                {
                    Context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.RunEnd, StopCrossFadeDuration, 0, 0f);
                    _isStopping = true;
                }
            }
            else
            {
                _noInputTimer = 0f;
            }
        }

        /// <inheritdoc/>
        public override void LateUpdate()
        {
            Context.MotionDriver.UpdateFreeLookRotation(Context.Blackboard.MoveDirection);
        }

    }
}
