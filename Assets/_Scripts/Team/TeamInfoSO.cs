using System;
using SPCharacterController;
using UnityEngine;

namespace SPTeam
{
    /// <summary>
    /// 队伍数据 - 持有三个角色信息与当前激活索引，作为队伍状态的单一数据源。
    /// </summary>
    [CreateAssetMenu(menuName = "SPTeam/TeamInfo", fileName = "TeamInfo")]
    public class TeamInfoSO : ScriptableObject
    {
        /// <summary>队伍角色数量。</summary>
        public const int CharacterCount = 3;

        [Header("队伍构成")]
        [Tooltip("队伍中三个角色的 CharacterInfoSO，按索引 0-2 对应。")]
        [SerializeField] private CharacterInfoSO[] _characters = new CharacterInfoSO[CharacterCount];

        [Tooltip("当前激活角色的索引。")]
        [SerializeField] private int _activeCharacterIndex;

        /// <summary>队伍中三个角色的信息数组。</summary>
        public CharacterInfoSO[] Characters => _characters;

        /// <summary>当前激活角色的索引。</summary>
        public int ActiveCharacterIndex => _activeCharacterIndex;

        /// <summary>
        /// 顺序切换到下一个角色（0→1→2→0→...）。
        /// </summary>
        public int SwitchCharacter()
        {
            _activeCharacterIndex = (_activeCharacterIndex + 1) % CharacterCount;
            return _activeCharacterIndex;
        }

        /// <summary>
        /// 按队伍内索引获取角色预制体。
        /// </summary>
        /// <param name="index">角色索引 0-2</param>
        /// <returns>对应角色的预制体，若 CharacterInfoSO 未配置 prefab 则返回 null</returns>
        public GameObject GetPrefab(int index)
        {
            if (index < 0 || index >= CharacterCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _characters[index]?.Prefab;
        }

        /// <summary>
        /// 按角色 ID 查找预制体。
        /// </summary>
        /// <param name="characterId">角色唯一标识</param>
        /// <returns>匹配角色的预制体，未找到返回 null</returns>
        public GameObject GetPrefab(string characterId)
        {
            for (int i = 0; i < CharacterCount; i++)
            {
                if (_characters[i] != null && _characters[i].CharacterId == characterId)
                    return _characters[i].Prefab;
            }
            return null;
        }
    }
}
