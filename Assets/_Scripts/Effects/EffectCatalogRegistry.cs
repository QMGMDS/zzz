using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEffects
{
    /// <summary>
    /// 特效目录注册表 - 全局唯一的特效 ID 到预制体映射，同一目录资产只解析一次。
    /// </summary>
    public static class EffectCatalogRegistry
    {
        private static readonly Dictionary<EffectCatalogSO, IReadOnlyDictionary<string, GameObject>> s_cachedCatalogs =
            new Dictionary<EffectCatalogSO, IReadOnlyDictionary<string, GameObject>>();

        /// <summary>
        /// 获取目录资产对应的特效 ID 映射，首次访问时解析并缓存。
        /// </summary>
        /// <param name="catalog">特效目录资产</param>
        /// <returns>特效 ID 到预制体的只读映射</returns>
        public static IReadOnlyDictionary<string, GameObject> Get(EffectCatalogSO catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));

            if (s_cachedCatalogs.TryGetValue(catalog, out IReadOnlyDictionary<string, GameObject> cached))
                return cached;

            IReadOnlyDictionary<string, GameObject> parsed = Parse(catalog);
            s_cachedCatalogs.Add(catalog, parsed);
            return parsed;
        }

        /// <summary>
        /// 清空目录解析缓存，供目录资产变更或编辑器热重载后重新解析。
        /// </summary>
        public static void Clear() => s_cachedCatalogs.Clear();

        private static IReadOnlyDictionary<string, GameObject> Parse(EffectCatalogSO catalog)
        {
            var result = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            IReadOnlyList<EffectCatalogEntry> entries = catalog.Entries;
            for (int i = 0; i < entries.Count; i++)
            {
                EffectCatalogEntry entry = entries[i];
                if (entry == null)
                    throw new InvalidOperationException($"特效目录 {catalog.name} 的 Entries[{i}] 为空。");
                if (string.IsNullOrWhiteSpace(entry.EffectId))
                    throw new InvalidOperationException($"特效目录 {catalog.name} 的 Entries[{i}] 未设置 EffectId。");
                if (entry.Prefab == null)
                    throw new InvalidOperationException($"特效目录 {catalog.name} 的 ID \"{entry.EffectId}\" 未设置预制体。");
                if (!result.TryAdd(entry.EffectId, entry.Prefab))
                    throw new InvalidOperationException($"特效目录 {catalog.name} 存在重复 ID：\"{entry.EffectId}\"。");
            }

            return result;
        }
    }
}
