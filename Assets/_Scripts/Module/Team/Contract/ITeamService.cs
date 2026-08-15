using System.Collections.Generic;

using UnityEngine;

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

        /// <summary>
        /// 获取队伍装配计划 - 校验配置后返回按切换顺序排列的槽位清单
        /// </summary>
        /// <returns>槽位清单 配置无效时抛出异常</returns>
        IReadOnlyList<TeamSlotPlan> GetSlotPlan();

        /// <summary>
        /// 移交队伍装配结果 - 登记名册并激活初始角色
        /// </summary>
        /// <param name="entries">角色装配结果列表 必须与装配计划一一对应</param>
        void InitializeRoster(IReadOnlyList<TeamAssemblyEntry> entries);

        /// <summary>
        /// 获取指定角色的实例变换
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <returns>角色实例变换 名册未初始化或 Id 不存在时返回 null</returns>
        Transform GetCharacterTransform(string characterId);
    }
}