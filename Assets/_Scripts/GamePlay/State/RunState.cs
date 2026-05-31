using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 跑步状态：仅通过 Evade 键进入。松手后播 RunEnd→Idle，停止期间检测到移动输入则切入 WalkState。
    /// 要重新跑必须再次触发 Evade（经 EvadeFrontState）。
    /// </summary>
    public class RunState : StateBase
    {
        private const float RotationSmoothTime = 0.1f;
        private const float CrossFadeDuration = 0.10f;
        private const float StopCrossFadeDuration = 0.15f;

        private Transform _cameraTransform;
        private float _rotationVelocity;
        private float _noInputTimer;
        private bool _isStopping;

        /// <inheritdoc/>
        public override void Enter(IStateContext context) 
        {
            Context = context;
            _cameraTransform = Context.MainCamera.transform;
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
            Vector2 direction = Context.MoveDirection;

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
                if (_noInputTimer >= Context.InputBufferTime)
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
            Vector2 direction = Context.MoveDirection;
            if (direction.sqrMagnitude < 0.0001f) return;

            Vector3 cameraForward = _cameraTransform.forward;
            Vector3 cameraRight = _cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            Vector3 worldMoveDir = (cameraForward * direction.y + cameraRight * direction.x).normalized;
            float targetAngle = Mathf.Atan2(worldMoveDir.x, worldMoveDir.z) * Mathf.Rad2Deg;
            Transform t = Context.Transform;
            float smoothedAngle = Mathf.SmoothDampAngle(
                t.eulerAngles.y,
                targetAngle,
                ref _rotationVelocity,
                RotationSmoothTime
            );
            t.eulerAngles = new Vector3(0f, smoothedAngle, 0f);
        }

    }
}
