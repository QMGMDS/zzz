using SPFramework.Service;

namespace SPTeam.Contract
{
    /// <summary>
    /// 队伍服务 - 模块级契约 负责队伍角色切换请求与状态查询
    /// </summary>
    public interface ITeamService : IModuleService
    {
        /// <summary>当前上场角色 Id</summary>
        string ActiveCharacterId { get; }

        /// <summary>是否处于切换中 - 切换双锁未全开时为真</summary>
        bool IsSwitching { get; }

        /// <summary>是否锁定玩家操作 - 目标入场完成前为真</summary>
        bool IsOperationLocked { get; }

        /// <summary>
        /// 请求切换一次队伍角色
        /// </summary>
        /// <returns>切换是否成功发起</returns>
        bool TryRequestSwitch();
    }
}