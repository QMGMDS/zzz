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
        }

        private void OnEnable()
        {
            ModuleServiceHub.Register<ITeamService>(_orchestrator);

            _poseAppliedSubscription = EventBus.Subscribe(CharacterEvents.SwitchInPoseApplied, OnSwitchInPoseApplied);
            _switchInSubscription = EventBus.Subscribe(CharacterEvents.SwitchInCompleted, OnSwitchInCompleted);
            _switchOutSubscription = EventBus.Subscribe(CharacterEvents.SwitchOutCompleted, OnSwitchOutCompleted);
        }

        private void OnDisable()
        {
            _poseAppliedSubscription?.Dispose();
            _switchInSubscription?.Dispose();
            _switchOutSubscription?.Dispose();
            _poseAppliedSubscription = null;
            _switchInSubscription = null;
            _switchOutSubscription = null;

            ModuleServiceHub.Unregister<ITeamService>(_orchestrator);
        }

        private void Update()
        {
            _orchestrator.Tick();

            // 输入服务未注册时跳过本帧切换请求
            if (!ModuleServiceHub.TryGet<IProvideFrameInput>(out IProvideFrameInput provider))
                return;

            if (provider.CurrentProcessed.SwitchCharacter.IsPressed)
                _orchestrator.TryRequestSwitch();
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
