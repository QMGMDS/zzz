using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPTeam.Core
{
    /// <summary>
    /// 队伍名册 - 登记角色实例并管理其生命周期
    /// </summary>
    internal sealed class TeamRoster
    {
        private readonly List<string> _orderedCharacterIds = new();
        private readonly Dictionary<string, GameObject> _characterObjects = new();
        private readonly List<Action> _releaseActions = new();

        /// <summary>按切换顺序排列的角色 Id 列表</summary>
        public IReadOnlyList<string> OrderedCharacterIds => _orderedCharacterIds;

        /// <summary>
        /// 获取指定索引的角色 Id
        /// </summary>
        /// <param name="index">角色索引</param>
        /// <returns>角色 Id</returns>
        public string GetCharacterIdAt(int index)
        {
            return _orderedCharacterIds[index];
        }

        /// <summary>
        /// 获取角色实例对象
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <returns>角色实例对象</returns>
        public GameObject GetCharacterObject(string characterId)
        {
            return _characterObjects[characterId];
        }

        /// <summary>
        /// 尝试获取角色实例对象
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <param name="instance">角色实例对象</param>
        /// <returns>Id 是否已登记</returns>
        public bool TryGetCharacterObject(string characterId, out GameObject instance)
        {
            return _characterObjects.TryGetValue(characterId, out instance);
        }

        /// <summary>
        /// 获取指定索引的角色实例对象
        /// </summary>
        /// <param name="index">角色索引</param>
        /// <returns>角色实例对象</returns>
        public GameObject GetCharacterObjectAt(int index)
        {
            return _characterObjects[_orderedCharacterIds[index]];
        }

        /// <summary>
        /// 登记角色实例
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <param name="instance">角色实例对象</param>
        /// <param name="release">释放实例的委托</param>
        public void Register(string characterId, GameObject instance, Action release)
        {
            _orderedCharacterIds.Add(characterId);
            _characterObjects.Add(characterId, instance);
            _releaseActions.Add(release);
        }

        /// <summary>
        /// 释放全部角色实例
        /// </summary>
        public void Release()
        {
            foreach (Action release in _releaseActions)
                release();

            _releaseActions.Clear();
        }
    }
}
