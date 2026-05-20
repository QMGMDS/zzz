using GamePlay.Common;

namespace GamePlay.State
{
    /// <summary>
    /// 待机状态：检测到输入时切换至 WalkState
    /// </summary>
    public class IdleState : IState
    {
        private IStateContext _context;

        public void Enter(IStateContext context)
        {
            _context = context;
            _context.Animator.SetBool(AnimationHashes.HasInput, false);
        }

        public void Exit()
        {
        }

        public void Update()
        {
            if (_context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                _context.StateMachine.ChangeState<WalkState>();
            }
        }

        public void PhysicsUpdate()
        {
        }
    }
}
