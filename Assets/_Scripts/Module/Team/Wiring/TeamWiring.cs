using System;

using UnityEngine;

using SPCharacter.Contract;
using SPFramework.Event;
using SPFramework.Service;
using SPInput.Contract;
using SPTeam.Contract;
using SPTeam.Core;

namespace SPTeam.Wiring
{
    /// <summary>
    /// 队伍接线胶水 - 创建切换编排器、注册队伍服务并转交外部信号
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