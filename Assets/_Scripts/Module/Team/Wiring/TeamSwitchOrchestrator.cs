using System;

using UnityEngine;

using SPCamera.Contract;
using SPCharacter.Contract;
using SPFramework.Event;
using SPFramework.Service;
using SPTeam.Contract;
using SPTeam.Core;

namespace SPTeam.Wiring
{
    /// <summary>
    /// 队伍切换编排器 - 联合角色与相机模块实现队伍角色切换
    /// </summary>
    internal sealed class TeamSwitchOrchestrator : ITeamService
    {
        private readonly TeamService _team;
        private bool _isLastPublishedOperationLocked;
        private bool _isLastPublishedSwitchLocked;

        /// <summary>
        /// 创建队伍切换编排器
        /// </summary>
        /// <param name="team">队伍数据层 不可为空</param>
        public TeamSwitchOrchestrator(TeamService team)
        {
            _team = team ?? throw new ArgumentNullException(nameof(team));
        }

        /// <inheritdoc />
        public string ActiveCharacterId => _team.ActiveCharacterId;

        /// <inheritdoc />
        public bool IsSwitching => _team.IsSwitching;

        /// <inheritdoc />
        public bool IsOperationLocked => _team.IsOperationLocked;

        /// <inheritdoc />
        public bool TryRequestSwitch()
        {
            if (!_team.CanRequestSwitch)
                return false;

            string previousCharacterId = _team.ActiveCharacterId;
            string nextCharacterId = _team.ResolveNextCharacterId();

            GameObject previousRoot = _team.GetCharacterObject(previousCharacterId);
            GameObject nextRoot = _team.GetCharacterObject(nextCharacterId);
            nextRoot.SetActive(true);

            if (!CanBeginSwitch(previousCharacterId, nextCharacterId))
            {
                nextRoot.SetActive(false);
                return false;
            }

            if (!_team.TryCommitSwitch(previousCharacterId, nextCharacterId))
            {
                nextRoot.SetActive(false);
                return false;
            }

            Pose pose = new Pose(previousRoot.transform.position, previousRoot.transform.rotation);

            BeginSwitch(previousCharacterId, nextCharacterId, pose);
            PublishActiveCharacterChanged(previousCharacterId, nextCharacterId);
            PublishSwitchLockChanged();
            return true;
        }

        /// <summary>
        /// 应用初始相机跟随
        /// </summary>
        public void ApplyInitialCameraFollow()
        {
            SetCameraFollowTarget(_team.GetCharacterObject(_team.InitialCharacterId).transform);
        }

        /// <summary>
        /// 角色上场位姿已应用通知 - 由接线胶水转发 相机跟随切换至此
        /// </summary>
        /// <param name="characterId">完成落位的角色 Id</param>
        public void NotifySwitchInPoseApplied(string characterId)
        {
            if (!string.Equals(_team.SwitchInCharacterId, characterId, StringComparison.Ordinal))
                return;

            SetCameraFollowTarget(_team.GetCharacterObject(characterId).transform);
        }

        /// <summary>
        /// 角色上场完成通知 - 由接线胶水转发
        /// </summary>
        /// <param name="characterId">完成上场的角色 Id</param>
        public void NotifySwitchInCompleted(string characterId)
        {
            if (!_team.CompleteSwitchIn(characterId))
                return;

            SetOperationLocked(characterId, false);
            PublishSwitchLockChanged();
        }

        /// <summary>
        /// 角色退场完成通知 - 由接线胶水转发
        /// </summary>
        /// <param name="characterId">完成退场的角色 Id</param>
        public void NotifySwitchOutCompleted(string characterId)
        {
            if (!_team.CompleteSwitchOut(characterId))
                return;

            _team.GetCharacterObject(characterId).SetActive(false);
            PublishSwitchLockChanged();
        }

        private void PublishSwitchLockChanged()
        {
            bool isOperationLocked = _team.IsOperationLocked;
            bool isSwitchLocked = _team.IsSwitching;

            if (_isLastPublishedOperationLocked == isOperationLocked
                && _isLastPublishedSwitchLocked == isSwitchLocked)
                return;

            _isLastPublishedOperationLocked = isOperationLocked;
            _isLastPublishedSwitchLocked = isSwitchLocked;

            EventBus.Publish(
                TeamEvents.SwitchLockChanged,
                new TeamSwitchLockChangedEvent(isOperationLocked, isSwitchLocked));
        }

        private void PublishActiveCharacterChanged(string previousCharacterId, string currentCharacterId)
        {
            EventBus.Publish(
                TeamEvents.ActiveCharacterChanged,
                new TeamActiveCharacterChangedEvent(previousCharacterId, currentCharacterId));
        }

        private static bool CanBeginSwitch(string previousCharacterId, string nextCharacterId)
        {
            return InstanceServiceHub.TryGet<ICharacterSwitchService>(previousCharacterId, out _)
                && InstanceServiceHub.TryGet<ICharacterSwitchService>(nextCharacterId, out _);
        }

        private static void BeginSwitch(string previousCharacterId, string nextCharacterId, Pose pose)
        {
            // 可用性已由 CanBeginSwitch 校验 此处失败为异常路径 静默跳过
            if (!InstanceServiceHub.TryGet<ICharacterSwitchService>(nextCharacterId, out ICharacterSwitchService nextService))
                return;

            if (!InstanceServiceHub.TryGet<ICharacterSwitchService>(previousCharacterId, out ICharacterSwitchService previousService))
                return;

            previousService.SetOperationLocked(true);
            nextService.SetOperationLocked(true);
            previousService.BeginSwitchOut();
            nextService.BeginSwitchIn(pose);
        }

        private static void SetOperationLocked(string characterId, bool isLocked)
        {
            if (InstanceServiceHub.TryGet<ICharacterSwitchService>(characterId, out ICharacterSwitchService service))
                service.SetOperationLocked(isLocked);
        }

        private static void SetCameraFollowTarget(Transform target)
        {
            if (ModuleServiceHub.TryGet<ISetCameraFollowTarget>(out ISetCameraFollowTarget setter))
                setter.SetCameraFollowTarget(target);
        }
    }
}