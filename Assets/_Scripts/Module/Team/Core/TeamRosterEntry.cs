using System;

using UnityEngine;

namespace SPTeam.Core
{
    /// <summary>
    /// 队伍名册登记项 - 单个角色的装配结果
    /// </summary>
    internal readonly struct TeamRosterEntry
    {
        /// <summary>
        /// 创建队伍名册登记项
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <param name="instance">角色实例对象</param>
        /// <param name="release">释放实例的委托</param>
        public TeamRosterEntry(string characterId, GameObject instance, Action release)
        {
            CharacterId = characterId;
            Instance = instance;
            Release = release;
        }

        /// <summary>角色 Id</summary>
        public string CharacterId { get; }

        /// <summary>角色实例对象</summary>
        public GameObject Instance { get; }

        /// <summary>释放实例的委托</summary>
        public Action Release { get; }
    }
}