using GamePlay.Common;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 行走状态：处理摄像机朝向旋转，无输入时切回 IdleState
    /// </summary>
    public class WalkState : IState
    {
        private const float RotationSmoothTime = 0.1f;

        private IStateContext _context;
        private Transform _cameraTransform;
        private float _rotationVelocity;

        public void Enter(IStateContext context)
        {
            _context = context;
            _cameraTransform = _context.MainCamera.transform;
            _context.Animator.SetBool(AnimationHashes.HasInput, true);
        }

        public void Exit()
        {
        }

        public void Update()
        {
            Vector2 direction = _context.MoveDirection;

            if (direction.sqrMagnitude < 0.0001f)
            {
                _context.StateMachine.ChangeState<IdleState>();
                return;
            }

            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            Vector3 worldMoveDir = (cameraForward * direction.y + cameraRight * direction.x).normalized;
            float targetAngle = Mathf.Atan2(worldMoveDir.x, worldMoveDir.z) * Mathf.Rad2Deg;
            Transform t = _context.Transform;
            float smoothedAngle = Mathf.SmoothDampAngle(
                t.eulerAngles.y,
                targetAngle,
                ref _rotationVelocity,
                RotationSmoothTime
            );
            t.eulerAngles = new Vector3(0f, smoothedAngle, 0f);
        }

        public void PhysicsUpdate()
        {
        }
    }
}
