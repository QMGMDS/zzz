using System;
using System.Collections.Generic;

using UnityEngine;

using SPTeam.Contract;

namespace SPTeam.Core
{
    /// <summary>
    /// 队伍数据层 - 维护队伍名册与切换状态 不参与任何跨模块协调
    /// </summary>
    [DefaultExecutionOrder(-350)]
    internal sealed class TeamService : MonoBehaviour
    {
        [Header("队伍配置")]
        [SerializeField, Tooltip("队伍配置资产 包含角色槽位与初始上场索引")]
        private TeamConfigSO _config;

        private TeamRoster _roster;
        private TeamSwitchCoordinator _coordinator;
        private bool _isInitialized;

        /// <summary>名册是否已初始化 - 初始化前不响应切换请求与状态查询</summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>按切换顺序排列的角色 Id 列表</summary>
        public IReadOnlyList<string> OrderedCharacterIds => _roster.OrderedCharacterIds;

        /// <summary>当前上场角色 Id</summary>
        public string ActiveCharacterId => _coordinator.ActiveCharacterId;

        /// <summary>是否处于切换中 - 切换双锁未全开时为真</summary>
        public bool IsSwitching => _coordinator.IsSwitchLocked;

        /// <summary>是否锁定玩家操作 - 目标入场完成前为真</summary>
        public bool IsOperationLocked => _coordinator.IsOperationLocked;

        /// <summary>当前是否允许发起切换</summary>
        public bool CanRequestSwitch => _coordinator.CanRequestSwitch;

        /// <summary>待入场角色 Id - 无切换时为空</summary>
        public string SwitchInCharacterId => _coordinator.SwitchInCharacterId;

        /// <summary>
        /// 获取队伍装配计划 - 校验配置后返回按切换顺序排列的槽位清单
        /// </summary>
        /// <returns>槽位清单 配置无效时抛出异常</returns>
        public IReadOnlyList<TeamSlotPlan> GetSlotPlan()
        {
            ValidateConfig();

            var plans = new List<TeamSlotPlan>(_config.Slots.Count);

            foreach (TeamCharacterSlot slot in _config.Slots)
                plans.Add(new TeamSlotPlan(slot.CharacterId, slot.ResourceKey));

            return plans;
        }

        /// <summary>
        /// 获取指定角色的实例变换
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <returns>角色实例变换 名册未初始化或 Id 不存在时返回 null</returns>
        public Transform GetCharacterTransform(string characterId)
        {
            if (_roster == null || !_roster.TryGetCharacterObject(characterId, out GameObject instance))
                return null;

            return instance.transform;
        }

        /// <summary>
        /// 计算顺序切换的下一个角色 Id
        /// </summary>
        /// <returns>下一个角色 Id</returns>
        public string ResolveNextCharacterId()
        {
            return _coordinator.ResolveNextCharacterId();
        }

        /// <summary>
        /// 获取角色实例对象
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <returns>角色实例对象</returns>
        public GameObject GetCharacterObject(string characterId)
        {
            return _roster.GetCharacterObject(characterId);
        }

        /// <summary>
        /// 提交一次切换 - 校验通过后进入切换状态
        /// </summary>
        /// <param name="activeCharacterId">当前上场角色 Id</param>
        /// <param name="targetCharacterId">目标角色 Id</param>
        /// <returns>是否提交成功</returns>
        public bool TryCommitSwitch(string activeCharacterId, string targetCharacterId)
        {
            return _coordinator.TryCommitSwitch(activeCharacterId, targetCharacterId);
        }

        /// <summary>
        /// 回推退场完成
        /// </summary>
        /// <param name="characterId">完成退场的角色 Id</param>
        /// <returns>是否匹配当前切换</returns>
        public bool CompleteSwitchOut(string characterId)
        {
            return _coordinator.CompleteSwitchOut(characterId);
        }

        /// <summary>
        /// 回推入场完成 - 完成后切换当前上场角色
        /// </summary>
        /// <param name="characterId">完成入场的角色 Id</param>
        /// <returns>是否匹配当前切换</returns>
        public bool CompleteSwitchIn(string characterId)
        {
            return _coordinator.CompleteSwitchIn(characterId);
        }

        /// <summary>
        /// 初始化队伍数据 - 登记装配结果并激活初始角色 配置无效时抛出异常
        /// </summary>
        /// <param name="entries">角色装配结果列表 必须与装配计划一一对应</param>
        public void Initialize(IReadOnlyList<TeamAssemblyEntry> entries)
        {
            ValidateConfig();

            if (entries == null || entries.Count != _config.Slots.Count)
                throw new InvalidOperationException($"{name}: 装配结果数量与队伍配置不符");

            for (int i = 0; i < entries.Count; i++)
            {
                if (!string.Equals(entries[i].CharacterId, _config.Slots[i].CharacterId, StringComparison.Ordinal))
                    throw new InvalidOperationException($"{name}: 装配结果顺序与队伍配置不符");
            }

            _roster = new TeamRoster();

            foreach (TeamAssemblyEntry entry in entries)
                _roster.Register(entry.CharacterId, entry.Instance, entry.Release);

            _coordinator = new TeamSwitchCoordinator(_roster.OrderedCharacterIds, _config.InitialIndex);
            _roster.GetCharacterObjectAt(_config.InitialIndex).SetActive(true);
            _isInitialized = true;
        }

        private void ValidateConfig()
        {
            if (_config == null)
                throw new InvalidOperationException($"{name}: 未配置队伍资产");

            if (!_config.TryValidate(out string errorMessage))
                throw new InvalidOperationException($"{name}: 队伍配置无效 - {errorMessage}");
        }

        private void OnDestroy()
        {
            // 装配失败时名册可能未创建 资源释放仍需兜底
            _roster?.Release();
        }
    }
}