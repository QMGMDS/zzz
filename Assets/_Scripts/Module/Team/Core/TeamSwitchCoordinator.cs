using System;
using System.Collections.Generic;

namespace SPTeam.Core
{
    /// <summary>
    /// 队伍切换协调器 - 管理切换双锁与顺序推进
    /// </summary>
    internal sealed class TeamSwitchCoordinator
    {
        private readonly IReadOnlyList<string> _orderedCharacterIds;
        private int _activeIndex;
        private string _switchOutCharacterId;
        private string _switchInCharacterId;
        private bool _hasSwitchOutCompleted;
        private bool _hasSwitchInCompleted;

        /// <summary>
        /// 创建队伍切换协调器
        /// </summary>
        /// <param name="orderedCharacterIds">按切换顺序排列的角色 Id 列表</param>
        /// <param name="initialIndex">初始上场角色索引</param>
        public TeamSwitchCoordinator(IReadOnlyList<string> orderedCharacterIds, int initialIndex)
        {
            if (orderedCharacterIds == null)
                throw new ArgumentNullException(nameof(orderedCharacterIds));

            if (orderedCharacterIds.Count == 0)
                throw new ArgumentException("角色列表不能为空", nameof(orderedCharacterIds));

            if (initialIndex < 0 || initialIndex >= orderedCharacterIds.Count)
                throw new ArgumentOutOfRangeException(nameof(initialIndex));

            _orderedCharacterIds = orderedCharacterIds;
            _activeIndex = initialIndex;
            _hasSwitchOutCompleted = true;
            _hasSwitchInCompleted = true;
        }

        /// <summary>当前上场角色 Id</summary>
        public string ActiveCharacterId => _orderedCharacterIds[_activeIndex];

        /// <summary>切换锁 - 双完成标记齐备后解锁</summary>
        public bool IsSwitchLocked => !(_hasSwitchOutCompleted && _hasSwitchInCompleted);

        /// <summary>操作锁 - 入场完成后解锁</summary>
        public bool IsOperationLocked => _switchInCharacterId != null;

        /// <summary>待退场角色 Id - 无切换时为空</summary>
        public string SwitchOutCharacterId => _switchOutCharacterId;

        /// <summary>待入场角色 Id - 无切换时为空</summary>
        public string SwitchInCharacterId => _switchInCharacterId;

        /// <summary>当前是否允许发起切换</summary>
        public bool CanRequestSwitch => _orderedCharacterIds.Count > 1 && !IsSwitchLocked;

        /// <summary>计算顺序切换的下一个角色 Id</summary>
        public string ResolveNextCharacterId()
        {
            return _orderedCharacterIds[(_activeIndex + 1) % _orderedCharacterIds.Count];
        }

        /// <summary>
        /// 提交一次切换 - 校验通过后进入切换状态
        /// </summary>
        /// <param name="activeCharacterId">当前上场角色 Id</param>
        /// <param name="targetCharacterId">目标角色 Id</param>
        /// <returns>是否提交成功</returns>
        public bool TryCommitSwitch(string activeCharacterId, string targetCharacterId)
        {
            if (!CanRequestSwitch)
                return false;

            if (!string.Equals(ActiveCharacterId, activeCharacterId, StringComparison.Ordinal))
                return false;

            if (string.Equals(activeCharacterId, targetCharacterId, StringComparison.Ordinal))
                return false;

            if (FindIndex(_orderedCharacterIds, targetCharacterId) < 0)
                return false;

            _switchOutCharacterId = activeCharacterId;
            _switchInCharacterId = targetCharacterId;
            _hasSwitchOutCompleted = false;
            _hasSwitchInCompleted = false;
            return true;
        }

        /// <summary>
        /// 回推退场完成
        /// </summary>
        /// <param name="characterId">完成退场的角色 Id</param>
        /// <returns>是否匹配当前切换</returns>
        public bool CompleteSwitchOut(string characterId)
        {
            if (!string.Equals(_switchOutCharacterId, characterId, StringComparison.Ordinal))
                return false;

            _switchOutCharacterId = null;
            _hasSwitchOutCompleted = true;
            return true;
        }

        /// <summary>
        /// 回推入场完成 - 完成后切换当前上场角色
        /// </summary>
        /// <param name="characterId">完成入场的角色 Id</param>
        /// <returns>是否匹配当前切换</returns>
        public bool CompleteSwitchIn(string characterId)
        {
            if (!string.Equals(_switchInCharacterId, characterId, StringComparison.Ordinal))
                return false;

            int index = FindIndex(_orderedCharacterIds, characterId);
            if (index < 0)
                return false;

            _switchInCharacterId = null;
            _hasSwitchInCompleted = true;
            _activeIndex = index;
            return true;
        }

        private static int FindIndex(IReadOnlyList<string> ids, string target)
        {
            for (int i = 0; i < ids.Count; i++)
            {
                if (string.Equals(ids[i], target, StringComparison.Ordinal))
                    return i;
            }

            return -1;
        }
    }
}