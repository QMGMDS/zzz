using System;

using UnityEngine;

namespace SPTeam.Contract
{
    /// <summary>
    /// 队伍装配计划槽位 - 单个角色的装配需求
    /// </summary>
    public readonly struct TeamSlotPlan
    {
        /// <summary>
        /// 创建装配计划槽位
        /// </summary>
        /// <param name="characterId">角色唯一标识 与角色实例服务 Id 一致</param>
        /// <param name="resourceKey">角色资源键 与资源目录键完全一致</param>
        public TeamSlotPlan(string characterId, string resourceKey)
        {
            CharacterId = characterId;
            ResourceKey = resourceKey;
        }

        /// <summary>角色唯一标识</summary>
        public string CharacterId { get; }

        /// <summary>角色资源键</summary>
        public string ResourceKey { get; }
    }

    /// <summary>
    /// 队伍装配结果项 - 单个角色的实例化结果移交
    /// </summary>
    public readonly struct TeamAssemblyEntry
    {
        /// <summary>
        /// 创建装配结果项
        /// </summary>
        /// <param name="characterId">角色唯一标识</param>
        /// <param name="instance">角色实例对象</param>
        /// <param name="release">释放实例的委托</param>
        public TeamAssemblyEntry(string characterId, GameObject instance, Action release)
        {
            CharacterId = characterId;
            Instance = instance;
            Release = release;
        }

        /// <summary>角色唯一标识</summary>
        public string CharacterId { get; }

        /// <summary>角色实例对象</summary>
        public GameObject Instance { get; }

        /// <summary>释放实例的委托</summary>
        public Action Release { get; }
    }
}
