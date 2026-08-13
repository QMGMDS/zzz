using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 运动驱动器 - 根据黑板中的目标方向更新朝向，并按动画归一化进度采样烘焙根运动曲线应用位移
    /// </summary>
    internal sealed class MotionDriver
    {
        private readonly CCRunTimeBlackboard _blackboard;
        private readonly IReadOnlyDictionary<string, StateNodeSO> _nodesById;
        private readonly Transform _bodyTransform;
        private readonly CharacterController _characterController;
        private uint _observedStateVersion;
        private RootMotionProfileSO _currentProfile;
        private Vector3 _previousLocalSample;
        private float _previousNormalizedTime;
        private bool _hasPreviousLocalSample;
        private bool _isCurrentStateLooping;

        /// <summary>
        /// 创建运动驱动器
        /// </summary>
        /// <param name="blackboard">角色运行时黑板</param>
        /// <param name="nodesById">状态机提供的只读节点解析表</param>
        /// <param name="bodyTransform">角色本体 Transform</param>
        /// <param name="characterController">角色 CharacterController 组件</param>
        public MotionDriver(
            CCRunTimeBlackboard blackboard,
            IReadOnlyDictionary<string, StateNodeSO> nodesById,
            Transform bodyTransform,
            CharacterController characterController)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _nodesById = nodesById ?? throw new ArgumentNullException(nameof(nodesById));
            _bodyTransform = bodyTransform ?? throw new ArgumentNullException(nameof(bodyTransform));
            _characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        }

        #region 旋转更新

        /// <summary>
        /// 旋转更新
        /// </summary>
        public void RotationUpdate()
        {
            ApplyContinuousRotation();
        }

        private void ApplyContinuousRotation()
        {
            Vector2 moveInput = _blackboard.MoveInput;

            StateNodeSO node = ResolveCurrentNode();
            if (node.TurnSpeedDegreesPerSecond <= 0f)
                return;

            float targetYaw = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
            float currentYaw = _bodyTransform.eulerAngles.y;
            float nextYaw = Mathf.MoveTowardsAngle(
                currentYaw,
                targetYaw,
                node.TurnSpeedDegreesPerSecond * Time.deltaTime);

            _bodyTransform.rotation = Quaternion.Euler(0f, nextYaw, 0f);
        }

        #endregion

        #region 位移更新

        /// <summary>
        /// 位移更新
        /// </summary>
        public void PositionUpdate()
        {
            if (_observedStateVersion != _blackboard.StateVersion)
            {
                OnStateChanged();
            }

            if (_currentProfile == null)
                return;

            ApplyDisplacement();
        }

        private void OnStateChanged()
        {
            _observedStateVersion = _blackboard.StateVersion;

            StateNodeSO node = ResolveCurrentNode();
            _currentProfile = node.RootMotionProfile;
            _isCurrentStateLooping = node.IsLooping;

            if (_currentProfile == null)
            {
                _previousNormalizedTime = 0f;
                _previousLocalSample = Vector3.zero;
                _hasPreviousLocalSample = false;
                return;
            }

            _previousNormalizedTime = _blackboard.AnimationEntryNormalizedTime;
            _previousLocalSample = SampleLocalDisplacement(_previousNormalizedTime);
            _hasPreviousLocalSample = true;
        }

        private void ApplyDisplacement()
        {
            // RootMotionProfile 曲线存储的是从动画起点到当前归一化时间的累计本地位移
            // 运行时必须用当前采样值减上一帧采样值，得到本帧增量后再落位，避免重复累加累计位移
            float normalizedTime = _blackboard.AnimationNormalizedTime;
            Vector3 currentLocalSample = SampleLocalDisplacement(normalizedTime);

            if (!_hasPreviousLocalSample)
            {
                _previousNormalizedTime = normalizedTime;
                _previousLocalSample = currentLocalSample;
                _hasPreviousLocalSample = true;
                return;
            }

            Vector3 localDelta = CalculateLocalDelta(normalizedTime, currentLocalSample);
            _previousNormalizedTime = normalizedTime;
            _previousLocalSample = currentLocalSample;

            Vector3 worldDelta = _bodyTransform.rotation * localDelta;
            _characterController.Move(worldDelta);
        }

        private Vector3 CalculateLocalDelta(float normalizedTime, Vector3 currentLocalSample)
        {
            bool hasLoopWrapped = _isCurrentStateLooping && normalizedTime < _previousNormalizedTime;
            if (!hasLoopWrapped)
                return currentLocalSample - _previousLocalSample;

            Vector3 startSample = SampleLocalDisplacement(0f);
            Vector3 endSample = SampleLocalDisplacement(1f);
            return endSample - _previousLocalSample + currentLocalSample - startSample;
        }

        private Vector3 SampleLocalDisplacement(float normalizedTime)
        {
            return new Vector3(
                _currentProfile.LocalX.Evaluate(normalizedTime),
                0f,
                _currentProfile.LocalZ.Evaluate(normalizedTime));
        }

        #endregion

        private StateNodeSO ResolveCurrentNode()
        {
            string id = _blackboard.CurrentStateId;
            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException("黑板没有当前状态 Id。");
            if (!_nodesById.TryGetValue(id, out StateNodeSO node) || node == null)
                throw new InvalidOperationException($"黑板当前状态 Id 无对应节点：{id}");
            return node;
        }
    }
}
