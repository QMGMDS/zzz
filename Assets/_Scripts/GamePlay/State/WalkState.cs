using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 行走状态：处理摄像机朝向旋转。连续无输入超缓冲时间后进入 RunEnd→Idle 停止动画链，
    /// 停止过程中检测到输入则取消停止并切回 WalkStart。
    /// </summary>
    public class WalkState : IState
    {
        private const float RotationSmoothTime = 0.1f;
        private const float CrossFadeDuration = 0.10f;
        private const float StopCrossFadeDuration = 0.15f;

        private IStateContext _context;
        private Transform _cameraTransform;
        private float _rotationVelocity;
        private float _noInputTimer;
        private bool _isStopping;

        public void Enter(IStateContext context)
        {
            _context = context;
            _cameraTransform = _context.MainCamera.transform;
            _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.WalkStart, CrossFadeDuration);
            _noInputTimer = 0f;
            _isStopping = false;
        }

        public void Exit()
        {
        }

        public void Update()
        {
            Vector2 direction = _context.MoveDirection;

            if (_isStopping)
            {
                if (direction.sqrMagnitude > 0.0001f)
                {
                    _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.WalkStart, CrossFadeDuration);
                    _isStopping = false;
                    _noInputTimer = 0f;
                    return;
                }

                if (IsInAnimatorState(Common.AnimationHashes.Idle))
                {
                    _context.StateMachine.ChangeState<IdleState>();
                }

                return;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                _noInputTimer += Time.deltaTime;
                if (_noInputTimer >= _context.InputBufferTime)
                {
                    _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.RunEnd, StopCrossFadeDuration);
                    _isStopping = true;
                }
            }
            else
            {
                _noInputTimer = 0f;
            }
        }

        /// <summary>在 Animator 更新后执行旋转，确保覆盖骨架动画曲线</summary>
        public void LateUpdate()
        {
            Vector2 direction = _context.MoveDirection;
            if (direction.sqrMagnitude < 0.0001f) return;

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

        private bool IsInAnimatorState(int stateHash)
        {
            return _context.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
        }
    }
}