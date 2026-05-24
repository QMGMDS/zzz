using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 前闪避状态：播 EvadeFront 动画。CD 时间内不可被移动或其他状态打断，
    /// CD 结束后有输入则切入 RunState，无输入等动画播完（normalizedTime ≥ 0.95）后切 IdleState。
    /// </summary>
    public class EvadeFrontState : IState
    {
        private const float CrossFadeDuration = 0.1f;
        private const float NaturalExitThreshold = 0.95f;

        private IStateContext _context;
        private float _animEnterTime;
        private bool _hasEnteredAnimState;

        public void Enter(IStateContext context)
        {
            _context = context;
            _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.EvadeFront, CrossFadeDuration);
            _hasEnteredAnimState = false;
        }

        public void Exit()
        {
        }

        public void Update()
        {
            AnimatorStateInfo stateInfo = _context.Animator.GetCurrentAnimatorStateInfo(0);

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

            if (Time.time - _animEnterTime < _context.EvadeFrontCommitDuration)
                return;

            if (_context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                _context.StateMachine.ChangeState<RunState>();
                return;
            }

            if (stateInfo.normalizedTime >= NaturalExitThreshold)
                _context.StateMachine.ChangeState<IdleState>();
        }

        public void LateUpdate()
        {
        }

        public void PhysicsUpdate()
        {
        }
    }
}
