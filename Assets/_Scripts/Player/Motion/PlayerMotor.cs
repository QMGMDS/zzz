using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家移动器
    /// </summary>
    public class PlayerMotor
    {
        private readonly CharacterController _characterController;
        private readonly Animator _animator;
        private readonly PlayerBrain _blackboard;
        private readonly PlayerMotorConfigSO _config;

        private readonly Transform _transform;

        private float _verticalVelocity;

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
            _config = config;

            _transform = characterController.transform;
        }

        /// <summary>
        /// 应用本帧移动
        /// </summary>
        public void ApplyMove()
        {
            if (_characterController == null || _animator == null || _blackboard == null || _config == null || _transform == null) return;

            var DeltaTime = Time.deltaTime;
            if (DeltaTime <= 0f) return;

            UpdateRotation(DeltaTime);
            UpdateVerticalVelocity(DeltaTime);

            var HorizontalMove = ResolveHorizontalMove(); // XZ 平面上的移动
            var VerticalMove = Vector3.up * _verticalVelocity * DeltaTime; // Y轴上的移动 

            _characterController.Move(HorizontalMove + VerticalMove);
        }

        private Vector3 ResolveHorizontalMove()
        {
            var RootDelta = _animator.deltaPosition;
            RootDelta.y = 0f;
            return RootDelta * ResolveRootMotionScale();
        }

        private float ResolveRootMotionScale()
        {
            switch (_blackboard.CurrentPlayerState)
            {
                case PlayerStateType.WalkStart:
                case PlayerStateType.WalkLoop:
                    return _config.WalkRootMotionScale;

                case PlayerStateType.RunStart:
                case PlayerStateType.RunLoop:
                case PlayerStateType.RunTurn:
                    return _config.RunRootMotionScale;

                case PlayerStateType.Stop:
                    return _config.StopRootMotionScale;

                case PlayerStateType.EvadeFront:
                    return _config.EvadeFrontRootMotionScale;

                case PlayerStateType.EvadeBack:
                    return _config.EvadeBackRootMotionScale;

                case PlayerStateType.Attack:
                    return _config.AttackRootMotionScale;

                default:
                    return _config.DefaultRootMotionScale;
            }
        }

        private void UpdateRotation(float deltaTime)
        {
            var currentState = _blackboard.CurrentPlayerState;
            switch (currentState)
            {
                case PlayerStateType.RunTurn:
                case PlayerStateType.EvadeFront:
                case PlayerStateType.EvadeBack:
                    return;
            }

            var Direction = _blackboard.CurrentMoveDirection;
            if (Direction.sqrMagnitude <= 0.0001f || _config.RotationSpeed <= 0f) return;

            var TargetRotation = Quaternion.LookRotation(Direction, Vector3.up);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, TargetRotation, _config.RotationSpeed * deltaTime);
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
