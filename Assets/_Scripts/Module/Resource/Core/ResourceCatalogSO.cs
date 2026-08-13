using System;
using System.Collections.Generic;

using UnityEngine;

using SPResource.Contract;

namespace SPResource.Core
{
    /// <summary>
    /// 资源目录项 - 保存资源键与预制体映射
    /// </summary>
    [Serializable]
    internal sealed class ResourceCatalogEntry
    {
        [SerializeField, Tooltip("资源定位键")]
        private string _key;

        [SerializeField, Tooltip("实例化用预制体")]
        private GameObject _prefab;

        /// <summary>资源定位键字符串</summary>
        public string Key => _key;

        /// <summary>实例化用预制体</summary>
        public GameObject Prefab => _prefab;
    }

    /// <summary>
    /// 资源目录 - 根据资源键查找预制体
    /// </summary>
    [CreateAssetMenu(menuName = "SPResource/Resource Catalog", fileName = "ResourceCatalog")]
    internal sealed class ResourceCatalogSO : ScriptableObject
    {
        [SerializeField, Tooltip("资源键到预制体的映射表")]
        private List<ResourceCatalogEntry> _entries = new List<ResourceCatalogEntry>();

        /// <summary>
        /// 根据资源键查找预制体
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="prefab">查找到的预制体</param>
        /// <returns>是否找到可用预制体</returns>
        public bool TryGetPrefab(ResourceKey key, out GameObject prefab)
        {
            prefab = null;

            if (!key.IsValid || _entries == null) return false;

            string keyValue = key.Value;
            for (int i = 0; i < _entries.Count; i++)
            {
                ResourceCatalogEntry entry = _entries[i];
                if (entry == null) continue;
                if (!string.Equals(entry.Key, keyValue, StringComparison.Ordinal)) continue;

                prefab = entry.Prefab;
                return prefab != null;
            }

            return false;
        }
    }
}
