using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 待机状态：检测到输入时切换至 WalkState，LateUpdate 中锁定旋转
    /// </summary>
    public class IdleState : IState
    {
        private const float CrossFadeDuration = 0.15f;

        private IStateContext _context;
        private Quaternion _lockedRotation;

        public void Enter(IStateContext context)
        {
            _context = context;
            _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.Idle, CrossFadeDuration);
            _lockedRotation = _context.Transform.rotation;
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

        /// <summary>在 Animator 更新后强制锁定旋转，覆盖骨架动画曲线</summary>
        public void LateUpdate()
        {
            _context.Transform.rotation = _lockedRotation;
        }

        public void PhysicsUpdate()
        {
        }
    }
}
