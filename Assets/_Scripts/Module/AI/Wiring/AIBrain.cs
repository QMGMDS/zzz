using System;

using UnityEngine;

using SPCharacter.Contract;
using SPFramework.Service;
using SPAI.Core;
using SPTeam.Contract;

namespace SPAI.Wiring
{
    /// <summary>
    /// AI 大脑 - 敌人感知与角色接线中枢 持有代码黑板供行为树任务访问
    /// 挂在敌人根物体上 出生位置即巡逻锚点
    /// </summary>
    internal sealed class AIBrain : MonoBehaviour
    {
        [Header("AI 配置")]
        [SerializeField, Tooltip("敌人 AI 决策参数配置资产")]
        private EnemyConfigSO _config;

        [SerializeField, Tooltip("角色唯一标识 与角色侧代理会话 Id 一致")]
        private string _characterId;

        private readonly AIRuntimeBlackboard _blackboard = new AIRuntimeBlackboard();

        /// <summary>AI 决策参数配置</summary>
        public EnemyConfigSO Config => _config;

        /// <summary>当前 AI 实例的运行时黑板</summary>
        public AIRuntimeBlackboard Blackboard => _blackboard;

        private void Awake()
        {
            if (_config == null)
                throw new InvalidOperationException($"{name}: 未设置敌人 AI 配置资产");
            if (string.IsNullOrWhiteSpace(_characterId))
                throw new InvalidOperationException($"{name}: 未设置角色唯一标识");

            _blackboard.Initialize(transform.position);
        }

        private void Update()
        {
            UpdatePerception();
        }

        /// <summary>
        /// 尝试获取角色代理会话 - 按角色 Id 按需获取 不长期缓存
        /// </summary>
        /// <param name="session">获取到的代理会话 未注册时为 null</param>
        /// <returns>是否获取成功 失败时消费方应降级为不驱动角色</returns>
        public bool TryGetAgentSession(out ICharacterAgentSession session)
            => InstanceServiceHub.TryGet(_characterId, out session);

        private void UpdatePerception()
        {
            if (TrySenseTarget(out Vector3 targetPosition))
            {
                _blackboard.SetVisibleTarget(targetPosition);
            }
            else
            {
                // 丢失视野仅置不可见 目标与最后目击位置保留 由追击动作负责抵达后的停留与清空
                _blackboard.MarkTargetNotVisible();
            }

            if (_blackboard.HasTarget
                && PerceptionUtility.DistanceXZ(transform.position, _blackboard.AnchorPosition) > _config.MaxChaseDistance)
                _blackboard.ClearTargetAndBeginReturning();
        }

        private bool TrySenseTarget(out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;

            // 队伍服务未注册或名册未初始化时降级为无目标
            if (!ModuleServiceHub.TryGet<ITeamService>(out ITeamService teamService))
                return false;

            string activeCharacterId = teamService.ActiveCharacterId;
            if (string.IsNullOrEmpty(activeCharacterId))
                return false;

            Transform targetTransform = teamService.GetCharacterTransform(activeCharacterId);
            if (targetTransform == null)
                return false;

            targetPosition = targetTransform.position;
            return PerceptionUtility.IsInViewCone(
                transform.position,
                transform.forward,
                targetPosition,
                _config.ViewDistance,
                _config.ViewAngle);
        }

    }
}
