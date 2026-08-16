using System;
using System.Collections.Generic;

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
    /// 队伍切换编排器 - 实现队伍服务契约 联合角色与相机模块编排队伍角色切换
    /// </summary>
    internal sealed class TeamSwitchOrchestrator : ITeamService
    {
        private readonly TeamService _team;
        private float _sessionBeganTime = -1f;
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
        public string ActiveCharacterId => _team.IsInitialized ? _team.ActiveCharacterId : null;

        /// <inheritdoc />
        public bool IsSwitching => _team.IsInitialized && _team.IsSwitchLocked;

        /// <inheritdoc />
        public bool IsOperationLocked => _team.IsInitialized && _team.IsOperationLocked;

        /// <inheritdoc />
        public bool TryRequestSwitch()
        {
            if (!_team.IsInitialized || !_team.CanRequestSwitch)
                return false;

            string previousCharacterId = _team.ActiveCharacterId;
            string nextCharacterId = _team.ResolveNextCharacterId();

            if (!TryGetCharacterRoot(nextCharacterId, out GameObject nextRoot))
                return false;

            // 先激活目标角色 其切换会话才会注册到实例服务中心
            nextRoot.SetActive(true);

            if (!CanBeginSwitch(previousCharacterId, nextCharacterId)
                || !_team.TryCommitSwitch(previousCharacterId, nextCharacterId))
            {
                nextRoot.SetActive(false);
                return false;
            }

            _sessionBeganTime = Time.time;

            GameObject previousRoot = _team.GetCharacterObject(previousCharacterId);
            Pose pose = new Pose(previousRoot.transform.position, previousRoot.transform.rotation);

            BeginSwitch(previousCharacterId, nextCharacterId, pose);
            PublishActiveCharacterChanged(previousCharacterId, nextCharacterId);
            PublishSwitchLockChanged();
            return true;
        }

        /// <inheritdoc />
        public IReadOnlyList<TeamSlotPlan> GetSlotPlan()
        {
            List<TeamSlotPlan> plan = new List<TeamSlotPlan>();

            foreach (TeamCharacterSlot slot in _team.GetSlots())
                plan.Add(new TeamSlotPlan(slot.CharacterId, slot.ResourceKey));

            return plan;
        }

        /// <inheritdoc />
        public void InitializeRoster(IReadOnlyList<TeamAssemblyEntry> entries)
        {
            List<TeamCharacterHandover> characters = new List<TeamCharacterHandover>(entries.Count);

            foreach (TeamAssemblyEntry entry in entries)
                characters.Add(new TeamCharacterHandover(entry.CharacterId, entry.Instance, entry.Release));

            _team.Initialize(characters);
        }

        /// <inheritdoc />
        public Transform GetCharacterTransform(string characterId)
        {
            return TryGetCharacterRoot(characterId, out GameObject instance)
                ? instance.transform
                : null;
        }

        /// <summary>
        /// 推进切换会话 - 检测会话超时并强制收尾
        /// </summary>
        public void Tick()
        {
            if (!_team.IsInitialized)
                return;

            if (!_team.IsSwitchLocked)
            {
                _sessionBeganTime = -1f;
                return;
            }

            if (_sessionBeganTime < 0f)
                return;

            if (Time.time - _sessionBeganTime <= _team.SwitchTimeout)
                return;

            _sessionBeganTime = -1f;
            ForceAbortSession();
        }

        /// <summary>
        /// 角色上场位姿已应用通知 - 由接线胶水转发 相机跟随切换至此
        /// </summary>
        /// <param name="characterId">完成落位的角色 Id</param>
        public void NotifySwitchInPoseApplied(string characterId)
        {
            if (!_team.IsInitialized
                || !string.Equals(_team.SwitchInCharacterId, characterId, StringComparison.Ordinal))
                return;

            if (TryGetCharacterRoot(characterId, out GameObject instance))
                SetCameraFollowTarget(instance.transform);
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

            if (TryGetCharacterRoot(characterId, out GameObject instance))
                instance.SetActive(false);

            PublishSwitchLockChanged();
        }

        private void ForceAbortSession()
        {
            // 先取回会话双方 Id 再强制收尾 收尾后会话字段即被清空
            string switchInCharacterId = _team.SwitchInCharacterId;
            string switchOutCharacterId = _team.SwitchOutCharacterId;

            _team.ForceCompleteSession();

            if (switchInCharacterId != null)
                SetOperationLocked(switchInCharacterId, false);

            if (switchOutCharacterId != null && TryGetCharacterRoot(switchOutCharacterId, out GameObject instance))
                instance.SetActive(false);

            Debug.LogWarning("[TeamSwitchOrchestrator] 切换会话超时 已强制收尾");
            PublishSwitchLockChanged();
        }

        private void PublishSwitchLockChanged()
        {
            bool isOperationLocked = _team.IsOperationLocked;
            bool isSwitchLocked = _team.IsSwitchLocked;

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

        private bool TryGetCharacterRoot(string characterId, out GameObject instance)
        {
            instance = null;
            return _team.IsInitialized
                   && _team.TryGetCharacterObject(characterId, out instance)
                   && instance != null;
        }

        private static bool CanBeginSwitch(string previousCharacterId, string nextCharacterId)
        {
            return InstanceServiceHub.TryGet<ICharacterSwitchSession>(previousCharacterId, out _)
                   && InstanceServiceHub.TryGet<ICharacterSwitchSession>(nextCharacterId, out _);
        }

        private static void BeginSwitch(string previousCharacterId, string nextCharacterId, Pose pose)
        {
            // 可用性已由 CanBeginSwitch 校验 此处失败为异常路径 静默跳过
            if (!InstanceServiceHub.TryGet<ICharacterSwitchSession>(nextCharacterId, out ICharacterSwitchSession nextSession))
                return;

            if (!InstanceServiceHub.TryGet<ICharacterSwitchSession>(previousCharacterId, out ICharacterSwitchSession previousSession))
                return;

            previousSession.SetOperationLocked(true);
            nextSession.SetOperationLocked(true);
            previousSession.BeginSwitchOut();
            nextSession.BeginSwitchIn(pose);
        }

        private static void SetOperationLocked(string characterId, bool isLocked)
        {
            if (InstanceServiceHub.TryGet<ICharacterSwitchSession>(characterId, out ICharacterSwitchSession service))
                service.SetOperationLocked(isLocked);
        }

        private static void SetCameraFollowTarget(Transform target)
        {
            if (ModuleServiceHub.TryGet<ISetCameraFollowTarget>(out ISetCameraFollowTarget setter))
                setter.SetCameraFollowTarget(target);
        }
    }
}
