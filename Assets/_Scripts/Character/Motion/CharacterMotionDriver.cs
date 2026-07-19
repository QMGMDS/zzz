using System;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色移动驱动器 - 根据输入、状态运动政策和重力生成唯一运动指令。
    /// </summary>
    public class CharacterMotionDriver
    {
        private readonly CharacterRunTimeData _blackboard;
        private readonly CharacterMotionConfigSO _config;
        private readonly CharacterMotor _motor;
        private readonly Transform _characterTransform;
        private readonly Transform _movementReference;
        private Vector3 _desiredMoveDirection;
        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private float _pendingVerticalDisplacement;
        private StateNodeSO _previousStateNode;
        private uint _observedStateVersion;
        private Vector3 _pendingSnapDirection;
        private bool _hasPendingSnapDirection;

        /// <summary>
        /// 创建角色移动驱动器。
        /// </summary>
        /// <param name="blackboard">角色运行时黑板</param>
        /// <param name="config">角色移动配置</param>
        /// <param name="motor">角色运动电机</param>
        /// <param name="characterTransform">角色根 Transform</param>
        /// <param name="movementReference">相机相对移动方向参考</param>
        public CharacterMotionDriver(CharacterRunTimeData blackboard, CharacterMotionConfigSO config, CharacterMotor motor, Transform characterTransform, Transform movementReference)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _config = config != null ? config : throw new ArgumentNullException(nameof(config));
            _motor = motor ?? throw new ArgumentNullException(nameof(motor));
            _characterTransform = characterTransform != null ? characterTransform : throw new ArgumentNullException(nameof(characterTransform));
            _movementReference = movementReference != null ? movementReference : throw new ArgumentNullException(nameof(movementReference));
        }

        /// <summary>当前状态是否等待动画根运动提交位移。</summary>
        public bool UsesRootMotion => _blackboard.CurrentStateNode.MotionMode == CharacterMotionMode.RootMotion;

        /// <summary>
        /// 根据输入和当前状态选择运动策略；
        /// 1) 代码驱动状态: 直接提交移动。
        /// 2) 根运动状态: 准备方向、旋转和垂直位移。
        /// </summary>
        /// <param name="deltaTime">本帧时间间隔</param>
        public void LogicUpdate(float deltaTime)
        {
            StateNodeSO stateNode = _blackboard.CurrentStateNode;

            // 算相对相机的角色移动方向
            _desiredMoveDirection = ResolveDesiredDirection(_blackboard.MoveInput);

            // 进入标记状态时锁定目标方向，切出后再同步根朝向。
            bool stateChanged = _observedStateVersion != _blackboard.StateVersion;
            if (stateChanged)
            {
                if (stateNode.SnapRotationOnExit && _desiredMoveDirection.sqrMagnitude > 0.0001f)
                {
                    _pendingSnapDirection = _desiredMoveDirection;
                    _hasPendingSnapDirection = true;
                }

                if (_previousStateNode != null && _previousStateNode.SnapRotationOnExit)
                {
                    if (stateNode.AllowRotation && _hasPendingSnapDirection)
                        _characterTransform.rotation = Quaternion.LookRotation(_pendingSnapDirection, Vector3.up);

                    _hasPendingSnapDirection = false;
                }
            }

            _observedStateVersion = _blackboard.StateVersion;
            _previousStateNode = stateNode;

            UpdateVerticalVelocity(deltaTime);
            Quaternion targetRotation = ResolveInputRotation(stateNode, _desiredMoveDirection, deltaTime);

            if (stateNode.MotionMode == CharacterMotionMode.RootMotion)
            {
                _pendingVerticalDisplacement = _verticalVelocity * deltaTime;
                return;
            }

            Vector3 planarDisplacement = UpdatePlanarVelocity(stateNode, _desiredMoveDirection, deltaTime) * deltaTime;
            Move(planarDisplacement + Vector3.up * (_verticalVelocity * deltaTime), targetRotation);
        }

        /// <summary>
        /// 仅在根运动状态下，消费 Animator 生成的根位移和根旋转。
        /// </summary>
        /// <param name="positionDelta">Animator 世界空间根位移</param>
        /// <param name="rotationDelta">Animator 根旋转</param>
        /// <param name="deltaTime">本帧时间间隔</param>
        public void ApplyRootMotion(Vector3 positionDelta, Quaternion rotationDelta, float deltaTime)
        {
            if (deltaTime <= 0f) return;

            StateNodeSO stateNode = _blackboard.CurrentStateNode;
            Vector3 planarDelta = Vector3.ProjectOnPlane(positionDelta, Vector3.up) * stateNode.RootMotionScale;
            Quaternion rotation = stateNode.UseRootMotionRotation
                ? _characterTransform.rotation * Quaternion.SlerpUnclamped(Quaternion.identity, rotationDelta, stateNode.RootMotionRotationScale)
                : ResolveInputRotation(stateNode, _desiredMoveDirection, deltaTime);

            _planarVelocity = planarDelta / deltaTime;
            Move(planarDelta + Vector3.up * _pendingVerticalDisplacement, rotation);
            _pendingVerticalDisplacement = 0f;
        }

        private Vector3 ResolveDesiredDirection(Vector2 moveInput)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_movementReference.forward, Vector3.up);

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            return (forward * moveInput.y + right * moveInput.x).normalized;
        }

        /// <summary>
        /// 根据当前状态的运动模式和玩家输入，平滑更新角色的水平面移动速度。
        /// </summary>
        private Vector3 UpdatePlanarVelocity(StateNodeSO stateNode, Vector3 desiredDirection, float deltaTime)
        {
            Vector3 targetVelocity = stateNode.MotionMode == CharacterMotionMode.CodeDriven
                ? desiredDirection * (_config.MaxMoveSpeed * _blackboard.MoveInputMagnitude)
                : Vector3.zero;
            float changeRate = targetVelocity.sqrMagnitude > 0f ? _config.Acceleration : _config.Deceleration;
            _planarVelocity = Vector3.MoveTowards(
                _planarVelocity,
                targetVelocity,
                changeRate * deltaTime);
            return _planarVelocity;
        }

        /// <summary>
        /// 根据接地状态和重力更新垂直速度。
        /// </summary>
        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_motor.IsGrounded && _verticalVelocity < 0f)
                _verticalVelocity = _config.GroundedVerticalSpeed;
            else
                _verticalVelocity = Mathf.Max(
                    _verticalVelocity - _config.GravityAcceleration * deltaTime,
                    -_config.MaxFallSpeed);
        }

        /// <summary>
        /// 根据输入方向，平滑地计算角色本帧应该朝向的旋转角度
        /// 只返回计算结果，不直接修改角色旋转。
        /// </summary>
        private Quaternion ResolveInputRotation(StateNodeSO stateNode, Vector3 desiredDirection, float deltaTime)
        {
            Quaternion currentRotation = _characterTransform.rotation;
            if (!stateNode.AllowRotation || desiredDirection.sqrMagnitude < 0.0001f)
                return currentRotation;

            Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
            return Quaternion.RotateTowards(currentRotation, desiredRotation, _config.RotationSpeed * deltaTime);
        }

        /// <summary>
        /// 提交角色本帧的位移和旋转，根据移动后的碰撞结果更新垂直速度与接地状态。
        /// </summary>
        private void Move(Vector3 displacement, Quaternion rotation)
        {
            CollisionFlags collisionFlags = _motor.Move(displacement, rotation);
            bool isGrounded = (collisionFlags & CollisionFlags.Below) != 0 || _motor.IsGrounded;

            if (isGrounded && _verticalVelocity < 0f) _verticalVelocity = _config.GroundedVerticalSpeed;

            _blackboard.PublishGrounded(isGrounded);
        }
    }
}
