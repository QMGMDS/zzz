using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 后撤步状态：播 EvadeBack 动画。有输入时在延迟窗口后可打断切入 WalkState，
    /// 无输入时等动画播完（normalizedTime ≥ 0.95）后自然过渡到 IdleState。
    /// </summary>
    public class EvadeBackState : IState
    {
        private const float InputDetectionDelay = 0.15f;
        private const float CrossFadeDuration = 0.05f;
        private const float NaturalExitThreshold = 0.95f;

        private IStateContext _context;
        private float _animEnterTime;
        private bool _hasEnteredAnimState;

        public void Enter(IStateContext context)
        {
            _context = context;
            _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.EvadeBack, CrossFadeDuration);
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
                if (stateInfo.shortNameHash == Common.AnimationHashes.EvadeBack)
                {
                    _hasEnteredAnimState = true;
                    _animEnterTime = Time.time;
                }

                return;
            }

            if (stateInfo.shortNameHash != Common.AnimationHashes.EvadeBack)
                return;

            if (Time.time - _animEnterTime < InputDetectionDelay)
                return;

            if (_context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                _context.StateMachine.ChangeState<WalkState>();
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
