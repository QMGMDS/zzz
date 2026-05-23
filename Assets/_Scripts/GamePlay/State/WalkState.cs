using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 行走状态，三阶段驱动：
    /// Entering — 输入后等待判定短/长按，不播动画；Walking — 长按确认，正常移动；
    /// Stopping — 短按/松手，播 RunEnd 并重置到 time=0。
    /// RunEnd 播放期间检测到输入则回到 Entering 重新判定；再次短按则打断并重播 RunEnd。
    /// </summary>
    public class WalkState : IState
    {
        private enum Phase
        {
            Entering,
            Walking,
            Stopping
        }

        private const float RotationSmoothTime = 0.1f;
        private const float CrossFadeDuration = 0.10f;
        private const float StopCrossFadeDuration = 0.15f;
        /// <summary>长/短按判定阈值</summary>
        private const float ShortPressThreshold = 0.15f;

        private IStateContext _context;
        private Transform _cameraTransform;
        private float _rotationVelocity;
        private float _noInputTimer;
        private float _holdTimer;
        private Phase _phase;

        public void Enter(IStateContext context)
        {
            _context = context;
            _cameraTransform = _context.MainCamera.transform;
            _noInputTimer = 0f;
            _holdTimer = 0f;
            _phase = Phase.Entering;
        }

        public void Exit()
        {
        }

        public void Update()
        {
            Vector2 direction = _context.MoveDirection;
            bool hasInput = direction.sqrMagnitude > 0.0001f;

            switch (_phase)
            {
                case Phase.Entering:
                    UpdateEntering(hasInput);
                    break;
                case Phase.Walking:
                    UpdateWalking(hasInput);
                    break;
                case Phase.Stopping:
                    UpdateStopping(hasInput);
                    break;
            }
        }

        private void UpdateEntering(bool hasInput)
        {
            if (!hasInput)
            {
                _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.RunEnd, StopCrossFadeDuration, 0, 0f);
                _phase = Phase.Stopping;
                return;
            }

            _holdTimer += Time.deltaTime;
            if (_holdTimer >= ShortPressThreshold)
            {
                _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.WalkStart, CrossFadeDuration);
                _phase = Phase.Walking;
            }
        }

        private void UpdateWalking(bool hasInput)
        {
            if (!hasInput)
            {
                _noInputTimer += Time.deltaTime;
                if (_noInputTimer >= _context.InputBufferTime)
                {
                    _context.Animator.CrossFadeInFixedTime(Common.AnimationHashes.RunEnd, StopCrossFadeDuration, 0, 0f);
                    _phase = Phase.Stopping;
                }
            }
            else
            {
                _noInputTimer = 0f;
            }
        }

        private void UpdateStopping(bool hasInput)
        {
            if (hasInput)
            {
                _holdTimer = 0f;
                _noInputTimer = 0f;
                _phase = Phase.Entering;
                return;
            }

            if (IsInAnimatorState(Common.AnimationHashes.Idle))
            {
                _context.StateMachine.ChangeState<IdleState>();
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
