using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家移动器——使用代码控制最终位移，同时采样动画 Root Delta 作为运动节奏倍率。
    /// </summary>
    public class PlayerMotor
    {
        private readonly CharacterController _characterController;
        private readonly Animator _animator;
        private readonly PlayerBrain _blackboard;
        private readonly Transform _transform;
        private readonly PlayerMotorConfigSO _config;

        private Vector3 _lastMoveDirection;
        private float _verticalVelocity;
        private float _motionScale = 1f;

        /// <summary>
        /// 创建玩家移动器
        /// </summary>
        /// <param name="characterController">角色控制器组件</param>
        /// <param name="animator">动画组件</param>
        /// <param name="blackboard">玩家大脑黑板</param>
        /// <param name="config">移动配置</param>
        public PlayerMotor(CharacterController characterController, Animator animator, PlayerBrain blackboard, PlayerMotorConfigSO config)
        {
            _characterController = characterController;
            _animator = animator;
            _blackboard = blackboard;
            _transform = characterController != null ? characterController.transform : null;
            _config = config;
        }

        /// <summary>
        /// 应用本帧移动
        /// </summary>
        public void ApplyMove()
        {
            if (_characterController == null || _animator == null || _blackboard == null || _transform == null || _config == null) return;

            var DeltaTime = Time.deltaTime;
            if (DeltaTime <= 0f) return;

            var Direction = ResolveInputMoveDirection();
            var BaseSpeed = ResolveBaseSpeed();
            UpdateMotionScale(DeltaTime);
            UpdateRotation(Direction, DeltaTime);
            UpdateVerticalVelocity(DeltaTime);

            var HorizontalMove = Direction * BaseSpeed * _motionScale * DeltaTime;
            var VerticalMove = Vector3.up * _verticalVelocity * DeltaTime;

            _characterController.Move(HorizontalMove + VerticalMove);
        }

        private Vector3 ResolveInputMoveDirection()
        {
            var MoveInput = _blackboard.MoveInput;
            var Direction = new Vector3(MoveInput.x, 0f, MoveInput.y);

            if (Direction.sqrMagnitude > 1f) // 斜向速度限制
                Direction.Normalize();

            if (Direction.sqrMagnitude > 0.0001f)
            {
                _lastMoveDirection = Direction.normalized;
                return _lastMoveDirection;
            }

            return _blackboard.CurrentPlayerState == PlayerStateType.Stop ? _lastMoveDirection : Vector3.zero;
        }

        private float ResolveBaseSpeed()
        {
            switch (_blackboard.CurrentPlayerState)
            {
                case PlayerStateType.RunStart:
                case PlayerStateType.RunLoop:
                    return _config.RunSpeed;

                case PlayerStateType.Stop:
                    return _config.StopSpeed;

                case PlayerStateType.WalkStart:
                case PlayerStateType.WalkLoop:
                    return _config.WalkSpeed;

                default:
                    return 0f;
            }
        }

        private void UpdateMotionScale(float deltaTime)
        {
            var HorizontalRootDelta = _animator.deltaPosition;
            HorizontalRootDelta.y = 0f;

            var RootSpeed = HorizontalRootDelta.magnitude / deltaTime; // 动画根运动速度 XZ
            var ReferenceSpeed = Mathf.Max(0.01f, _config.ReferenceRootSpeed); // 参照配置速度
            var TargetScale = RootSpeed > 0.0001f ? RootSpeed / ReferenceSpeed : 1f; // 动画运动倍率

            TargetScale = Mathf.Clamp(TargetScale, _config.MinMotionScale, _config.MaxMotionScale);

            var Smooth = 1f - Mathf.Exp(-_config.MotionScaleSmoothSpeed * deltaTime);
            _motionScale = Mathf.Lerp(_motionScale, TargetScale, Smooth);
        }

        private void UpdateRotation(Vector3 direction, float deltaTime)
        {
            if (direction.sqrMagnitude <= 0.0001f || _config.RotationSpeed <= 0f) return;

            var TargetRotation = Quaternion.LookRotation(direction, Vector3.up);
            _transform.rotation = Quaternion.RotateTowards(
                _transform.rotation,
                TargetRotation,
                _config.RotationSpeed * deltaTime);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -_config.GroundStickVelocity;
                return;
            }

            _verticalVelocity += _config.Gravity * deltaTime;
        }
    }
}
