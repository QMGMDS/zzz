using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>角色子拓展资产的有序列表。</summary>
    [CreateAssetMenu(menuName = "SPCharacter/Expansion/Expander List", fileName = "CharacterExpanderList")]
    public sealed class CharacterExpanderListSO : ScriptableObject
    {
        [SerializeField, Tooltip("按顺序执行的角色子拓展资产。")]
        private CharacterSubExpanderSO[] _subExpanders = Array.Empty<CharacterSubExpanderSO>();

        /// <summary>获取按配置顺序排列的角色子拓展。</summary>
        internal IReadOnlyList<CharacterSubExpanderSO> SubExpanders => _subExpanders;
    }
}
