using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEffects
{
    /// <summary>
    /// 特效目录资产 - 维护特效 ID 到预制体的只读映射。
    /// </summary>
    [CreateAssetMenu(menuName = "SPEffects/Effect Catalog", fileName = "EffectCatalog")]
    public sealed class EffectCatalogSO : ScriptableObject
    {
        [SerializeField, Tooltip("特效 ID 与预制体映射")]
        private EffectCatalogEntry[] _entries = Array.Empty<EffectCatalogEntry>();

        /// <summary>特效映射条目</summary>
        public IReadOnlyList<EffectCatalogEntry> Entries => _entries;
    }

    /// <summary>
    /// 特效目录条目 - 描述一个 ID 对应的预制体。
    /// </summary>
    [Serializable]
    public sealed class EffectCatalogEntry
    {
        [SerializeField, Tooltip("特效唯一 ID")]
        private string _effectId;

        [SerializeField, Tooltip("特效预制体")]
        private GameObject _prefab;

        /// <summary>特效唯一 ID</summary>
        public string EffectId => _effectId;

        /// <summary>特效预制体</summary>
        public GameObject Prefab => _prefab;
    }
}
