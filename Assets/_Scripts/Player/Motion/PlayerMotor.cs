using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家移动器——从 StateNodeSO 读取元数据驱动旋转和位移
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
            var node = _blackboard.CurrentStateNode;
            if (node == null || !node.AllowRotation) return;

            var direction = _blackboard.CurrentMoveDirection;
            if (direction.sqrMagnitude <= 0.0001f || _config.RotationSpeed <= 0f) return;

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRotation, _config.RotationSpeed * deltaTime);
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
            var rootDelta = _animator.deltaPosition;
            rootDelta.y = 0f;
            return rootDelta * (_blackboard.CurrentStateNode?.RootMotionScale ?? 1f);
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
