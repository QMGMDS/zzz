using System;
using SPTeam;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 数据模型 - 数据变更时广播事件。
    /// </summary>
    public class PlayerHUD_Model
    {
        private readonly TeamInfoSO _teamInfo;

        /// <summary>数据更新后广播的事件。</summary>
        public event Action DataUpdated;

        /// <summary>
        /// 创建模型实例并注入 TeamInfoSO 依赖。
        /// </summary>
        /// <param name="teamInfo">运行时队伍数据副本</param>
        public PlayerHUD_Model(TeamInfoSO teamInfo)
        {
            _teamInfo = teamInfo;
        }

        /// <summary>只读角色数据数组，委托自 TeamInfoSO。</summary>
        public SPCharacterController.CharacterInfoSO[] Characters => _teamInfo.Characters;

        /// <summary>当前激活角色索引，委托自运行时 TeamInfoSO。</summary>
        public int ActiveCharacterIndex => _teamInfo.ActiveCharacterIndex;

        /// <summary>
        /// 更新指定角色的当前生命值。
        /// </summary>
        /// <param name="index">角色索引 0-2</param>
        /// <param name="newHP">新的当前生命值</param>
        public void UpdateCharacterHP(int index, int newHP)
        {
            if (index < 0 || index >= TeamInfoSO.CharacterCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            DataUpdated?.Invoke();
        }

        /// <summary>
        /// 通知模型数据已变更，触发 UI 刷新。
        /// </summary>
        /// <param name="index">新激活角色索引（运行时副本已更新，参数仅保留兼容）。</param>
        public void SetActiveCharacter(int index)
        {
            if (index < 0 || index >= TeamInfoSO.CharacterCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            DataUpdated?.Invoke();
        }
    }
}
