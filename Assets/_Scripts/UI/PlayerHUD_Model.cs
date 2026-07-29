using System;
using SPCharacterController;
using SPTeam;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 数据模型 - 数据变更时广播事件。
    /// </summary>
    public class PlayerHUD_Model
    {
        private readonly TeamController _teamController;

        /// <summary>数据更新后广播的事件。</summary>
        public event Action DataUpdated;

        /// <summary>
        /// 创建模型实例并注入 TeamController 依赖。
        /// </summary>
        /// <param name="teamController">队伍控制器，提供队伍状态与角色运行时属性</param>
        public PlayerHUD_Model(TeamController teamController)
        {
            _teamController = teamController ?? throw new ArgumentNullException(nameof(teamController));
        }

        /// <summary>只读角色数据数组，委托自运行时 TeamInfoSO。</summary>
        public CharacterInfoSO[] Characters => _teamController.RuntimeTeamInfo.Characters;

        /// <summary>当前激活角色索引，委托自运行时 TeamInfoSO。</summary>
        public int ActiveCharacterIndex => _teamController.RuntimeTeamInfo.ActiveCharacterIndex;

        /// <summary>
        /// 获取指定索引角色的运行时属性副本。
        /// </summary>
        /// <param name="index">角色索引 0-2</param>
        /// <returns>对应角色的运行时属性</returns>
        public CharacterStats GetCharacterStats(int index) => _teamController.GetCharacterStats(index);

        /// <summary>
        /// 通知模型数据已变更，触发 UI 刷新。
        /// </summary>
        public void NotifyDataChanged() => DataUpdated?.Invoke();

        /// <summary>
        /// 响应角色切换，校验索引并触发 UI 刷新。
        /// </summary>
        /// <param name="index">新激活角色索引</param>
        public void SetActiveCharacter(int index)
        {
            if (index < 0 || index >= TeamInfoSO.CharacterCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            NotifyDataChanged();
        }
    }
}
