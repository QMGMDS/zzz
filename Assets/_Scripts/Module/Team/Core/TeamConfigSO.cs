using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPTeam.Core
{
    /// <summary>
    /// 队伍配置资产 - 定义 1 至 3 名角色槽位与初始上场索引
    /// </summary>
    [CreateAssetMenu(menuName = "SPTeam/Team Config", fileName = "TeamConfig")]
    internal sealed class TeamConfigSO : ScriptableObject
    {
        [Header("队伍配置")]
        [SerializeField, Range(1, 3), Tooltip("队伍角色数量 范围 1 至 3")]
        private int _slotCount = 1;

        [SerializeField, Tooltip("初始上场角色槽位索引 从 0 开始")]
        private int _initialIndex;

        [SerializeField, Tooltip("角色槽位列表 数量必须等于队伍角色数量")]
        private List<TeamCharacterSlot> _slots = new();

        /// <summary>角色槽位列表</summary>
        public IReadOnlyList<TeamCharacterSlot> Slots => _slots;

        /// <summary>初始上场角色槽位索引</summary>
        public int InitialIndex => _initialIndex;

        /// <summary>
        /// 校验队伍配置
        /// </summary>
        /// <param name="errorMessage">校验失败原因 成功时为空字符串</param>
        /// <returns>配置是否有效</returns>
        public bool TryValidate(out string errorMessage)
        {
            if (_slots == null || _slots.Count != _slotCount)
            {
                errorMessage = $"槽位数量必须等于队伍角色数量 {_slotCount}";
                return false;
            }

            if (_initialIndex < 0 || _initialIndex >= _slotCount)
            {
                errorMessage = $"初始上场索引必须在 0 至 {_slotCount - 1} 之间";
                return false;
            }

            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (TeamCharacterSlot slot in _slots)
            {
                if (string.IsNullOrWhiteSpace(slot.CharacterId))
                {
                    errorMessage = "角色 Id 不能为空";
                    return false;
                }

                if (!seenIds.Add(slot.CharacterId))
                {
                    errorMessage = $"角色 Id 重复 {slot.CharacterId}";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(slot.ResourceKey))
                {
                    errorMessage = $"角色 {slot.CharacterId} 未配置资源键";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// 队伍角色槽位 - 保存角色 Id 与对应预制体资源键
    /// </summary>
    [Serializable]
    internal sealed class TeamCharacterSlot
    {
        [SerializeField, Tooltip("角色唯一标识 与角色实例服务 Id 一致")]
        private string _characterId;

        [SerializeField, Tooltip("角色资源键 与资源目录键完全一致")]
        private string _resourceKey;

        /// <summary>角色唯一标识</summary>
        public string CharacterId => _characterId;

        /// <summary>角色资源键</summary>
        public string ResourceKey => _resourceKey;
    }
}
