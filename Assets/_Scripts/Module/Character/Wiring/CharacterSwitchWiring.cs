using System;

using UnityEngine;

using SPCharacter.Contract;
using SPCharacter.Core;
using SPFramework.Event;
using SPFramework.Service;

namespace SPCharacter.Wiring
{
    /// <summary>
    /// 角色切换接线胶水 - 实现切换会话契约，经胶水扩展窗口提交切换意图并广播切换事实
    /// </summary>
    internal sealed class CharacterSwitchWiring : MonoBehaviour, ICCWiringExtension, ICharacterSwitchSession
    {
        [Header("切换配置")]
        [SerializeField, Tooltip("角色唯一标识 用于切换路由")]
        private string _characterId;

        [SerializeField, Tooltip("上场状态节点 Id 需与角色状态配置一致")]
        private string _switchInStateId;

        [SerializeField, Tooltip("退场状态节点 Id 需与角色状态配置一致")]
        private string _switchOutStateId;

        private CharacterController _characterController;
        private ICharacterInputGate _inputGate;
        private string _lastObservedStateId;
        private bool _hasPendingSwitchIn;
        private bool _hasPendingSwitchOut;
        private bool _hasAppliedSwitchInPose;
        private bool _hasReportedSwitchInCompleted;
        private bool _hasReportedSwitchOutCompleted;
        private Pose _pendingSwitchInPose;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(_characterId))
                throw new InvalidOperationException($"{name}: 未设置角色唯一标识");

            _characterController = GetComponent<CharacterController>();
            _inputGate = GetComponent<ICharacterInputGate>();
        }

        private void OnEnable()
        {
            InstanceServiceHub.Register<ICharacterSwitchSession>(_characterId, this);
        }

        private void OnDisable()
        {
            InstanceServiceHub.Unregister<ICharacterSwitchSession>(_characterId, this);
        }

        /// <inheritdoc />
        public void UpdateWiring(CCWiringContext context, IWriteIntention writer)
        {
            SyncObservedState(context.CurrentStateId);
            TrySubmitSwitchRequests(context.CurrentStateId, writer);
            TryReportSwitchCompleted(context);
        }

        /// <inheritdoc />
        public void BeginSwitchOut()
        {
            _hasPendingSwitchOut = true;
        }

        /// <inheritdoc />
        public void BeginSwitchIn(Pose pose)
        {
            _pendingSwitchInPose = pose;
            _hasPendingSwitchIn = true;
            _hasAppliedSwitchInPose = false;
        }

        /// <inheritdoc />
        public void SetOperationLocked(bool isLocked)
        {
            if (_inputGate != null)
                _inputGate.SetOperationLocked(isLocked);
        }

        private void SyncObservedState(string currentStateId)
        {
            if (_lastObservedStateId == currentStateId)
                return;

            _lastObservedStateId = currentStateId;
            _hasReportedSwitchInCompleted = false;
            _hasReportedSwitchOutCompleted = false;

            if (currentStateId == _switchInStateId)
            {
                _hasPendingSwitchIn = false;
                _hasAppliedSwitchInPose = false;
            }
            else if (currentStateId == _switchOutStateId)
            {
                _hasPendingSwitchOut = false;
            }
        }

        private void TrySubmitSwitchRequests(string currentStateId, IWriteIntention writer)
        {
            if (_hasPendingSwitchOut && currentStateId != _switchOutStateId)
            {
                writer.SetIntention(CCIntention.WantToSwitchOut, true);
                return;
            }

            if (_hasPendingSwitchIn && currentStateId != _switchInStateId)
            {
                if (!_hasAppliedSwitchInPose)
                {
                    ApplySwitchInPose();
                    _hasAppliedSwitchInPose = true;
                    EventBus.Publish(
                        CharacterEvents.SwitchInPoseApplied,
                        new CharacterSwitchInPoseAppliedEvent(_characterId));
                }

                writer.SetIntention(CCIntention.WantToSwitchIn, true);
            }
        }

        private void ApplySwitchInPose()
        {
            // 传送前禁用 CharacterController 避免启用状态下瞬移产生的穿插修正
            if (_characterController != null)
                _characterController.enabled = false;

            transform.SetPositionAndRotation(_pendingSwitchInPose.position, _pendingSwitchInPose.rotation);

            if (_characterController != null)
                _characterController.enabled = true;
        }

        private void TryReportSwitchCompleted(CCWiringContext context)
        {
            if (string.Equals(_lastObservedStateId, _switchInStateId, StringComparison.Ordinal)
                && !_hasReportedSwitchInCompleted
                && context.AnimationNormalizedTime >= 1f)
            {
                _hasReportedSwitchInCompleted = true;
                _hasPendingSwitchIn = false;
                EventBus.Publish(
                    CharacterEvents.SwitchInCompleted,
                    new CharacterSwitchInCompletedEvent(_characterId));
                return;
            }

            if (string.Equals(_lastObservedStateId, _switchOutStateId, StringComparison.Ordinal)
                && !_hasReportedSwitchOutCompleted
                && context.AnimationNormalizedTime >= 1f)
            {
                _hasReportedSwitchOutCompleted = true;
                _hasPendingSwitchOut = false;
                EventBus.Publish(
                    CharacterEvents.SwitchOutCompleted,
                    new CharacterSwitchOutCompletedEvent(_characterId));
            }
        }
    }
}
