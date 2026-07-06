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

        #region 旋转更新

        /// <summary>
        /// 在 Update 中提前更新旋转，使 Animator 计算根运动时能基于新朝向
        /// </summary>
        public void ApplyRotation()
        {
            if (_characterController == null || _animator == null || _blackboard == null || _config == null || _transform == null) return;

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            UpdateRotation(deltaTime);
        }

        private void UpdateRotation(float deltaTime)
        {
            var currentState = _blackboard.CurrentPlayerState;
            switch (currentState) // 设置不宜旋转的状态
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

        #endregion

        #region 位移更新

        /// <summary>
        /// 在 OnAnimatorMove 中应用位置，此时 deltaPosition 已反映提前设定的朝向
        /// </summary>
        public void ApplyPosition()
        {
            if (_characterController == null || _animator == null || _blackboard == null || _config == null || _transform == null) return;

            var deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            UpdateVerticalVelocity(deltaTime);
            var verticalMove = Vector3.up * _verticalVelocity * deltaTime;
            var horizontalMove = ResolveHorizontalMove();
            _characterController.Move(horizontalMove + verticalMove);
        }

        private Vector3 ResolveHorizontalMove()
        {
            var RootDelta = _animator.deltaPosition; // deltaPosition 本帧根物体位移 基于上一帧位置建立的世界坐标系
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

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -_config.GroundStickVelocity;
                return;
            }

            _verticalVelocity += _config.Gravity * deltaTime;
        }

        #endregion
    }
}
