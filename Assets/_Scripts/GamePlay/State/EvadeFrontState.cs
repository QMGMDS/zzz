using GamePlay.Common;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 闪避状态：等待 EvadeFront 动画，期间检测到移动输入可随时打断并进入 RunState。
    /// 闪避触发（消费标记、SetTrigger）由 MovementStateMachine 层统一处理。
    /// </summary>
    public class EvadeFrontState : IState
    {
        private static readonly int EvadeFrontHash = Animator.StringToHash("EvadeFront");

        private IStateContext _context;
        private bool _hasEnteredAnimState;

        public void Enter(IStateContext context)
        {
            _context = context;
            _context.Animator.SetBool(AnimationHashes.HasInput, _context.MoveDirection.sqrMagnitude > 0.0001f);
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
                if (stateInfo.shortNameHash == EvadeFrontHash)
                {
                    _hasEnteredAnimState = true;
                }
                return;
            }

            if (_context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                _context.Animator.SetBool(AnimationHashes.HasInput, true);
                _context.StateMachine.ChangeState<RunState>();
                return;
            }

            if (stateInfo.shortNameHash != EvadeFrontHash || stateInfo.normalizedTime >= 1f)
            {
                _context.StateMachine.ChangeState<IdleState>();
            }
        }

        public void LateUpdate()
        {
        }

        public void PhysicsUpdate()
        {
        }
    }
}
