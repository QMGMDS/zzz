using System;
using System.Collections.Generic;

using UnityEngine;

using SPCharacter.Contract;
using SPFramework.Event;
using SPFramework.Service;
using SPInput.Contract;
using SPResource.Contract;
using SPTeam.Contract;
using SPTeam.Core;

namespace SPTeam.Wiring
{
    /// <summary>
    /// 队伍接线胶水 - 装配队伍数据层、注册切换编排器并转交外部信号
    /// </summary>
    [DefaultExecutionOrder(-350)]
    internal sealed class TeamWiring : MonoBehaviour
    {
        [Header("接线")]
        [SerializeField, Tooltip("队伍数据层")]
        private TeamService _service;

        private TeamSwitchOrchestrator _orchestrator;
        private IDisposable _poseAppliedSubscription;
        private IDisposable _switchInSubscription;
        private IDisposable _switchOutSubscription;

        private void Awake()
        {
            if (_service == null)
                throw new InvalidOperationException($"{name}: 未配置队伍数据层");

            _orchestrator = new TeamSwitchOrchestrator(_service);
            ModuleServiceHub.Register<ITeamService>(_orchestrator);
        }

        private void OnEnable()
        {
            _poseAppliedSubscription = EventBus.Subscribe(CharacterSwitchEvents.SwitchInPoseApplied, OnSwitchInPoseApplied);
            _switchInSubscription = EventBus.Subscribe(CharacterSwitchEvents.SwitchInCompleted, OnSwitchInCompleted);
            _switchOutSubscription = EventBus.Subscribe(CharacterSwitchEvents.SwitchOutCompleted, OnSwitchOutCompleted);
        }

        private void OnDisable()
        {
            _poseAppliedSubscription?.Dispose();
            _switchInSubscription?.Dispose();
            _switchOutSubscription?.Dispose();
            _poseAppliedSubscription = null;
            _switchInSubscription = null;
            _switchOutSubscription = null;
        }

        private void Start()
        {
            AssembleRoster();
            _orchestrator.ApplyInitialCameraFollow();
        }

        private void Update()
        {
            IProvideFrameInput provider = ModuleServiceHub.Get<IProvideFrameInput>();
            if (provider.CurrentProcessed.SwitchCharacter.IsPressed)
                _orchestrator.TryRequestSwitch();
        }

        private void OnDestroy()
        {
            ModuleServiceHub.Unregister<ITeamService>();
        }

        private void AssembleRoster()
        {
            TeamConfigSO config = _service.Config;
            if (config == null)
                throw new InvalidOperationException($"{name}: 未配置队伍资产");

            IInstantiateResource provider = ModuleServiceHub.Get<IInstantiateResource>();
            var entries = new List<TeamRosterEntry>(config.Slots.Count);

            foreach (TeamCharacterSlot slot in config.Slots)
            {
                ResourceLoadResult result = provider.Instantiate(new ResourceLoadRequest(
                    new ResourceKey(slot.ResourceKey),
                    parent: transform,
                    worldPosition: transform.position,
                    worldRotation: transform.rotation,
                    shouldActivateAfterCreate: false));

                if (!result.IsSuccess)
                {
                    ReleaseEntries(entries);
                    throw new InvalidOperationException($"{name}: 角色 {slot.CharacterId} 实例化失败");
                }

                entries.Add(new TeamRosterEntry(slot.CharacterId, result.Instance, result.Handle.Release));
            }

            _service.Initialize(entries);
        }

        private static void ReleaseEntries(IReadOnlyList<TeamRosterEntry> entries)
        {
            foreach (TeamRosterEntry entry in entries)
                entry.Release();
        }

        private void OnSwitchInPoseApplied(CharacterSwitchInPoseAppliedEvent payload)
        {
            _orchestrator.NotifySwitchInPoseApplied(payload.CharacterId);
        }

        private void OnSwitchInCompleted(CharacterSwitchInCompletedEvent payload)
        {
            _orchestrator.NotifySwitchInCompleted(payload.CharacterId);
        }

        private void OnSwitchOutCompleted(CharacterSwitchOutCompletedEvent payload)
        {
            _orchestrator.NotifySwitchOutCompleted(payload.CharacterId);
        }
    }
}