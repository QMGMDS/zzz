using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色阵营。
    /// </summary>
    public enum CharacterFaction
    {
        Player,
        Monster
    }

    /// <summary>
    /// 角色基础信息 - 保存角色标识与阵营。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/Info/CharacterInfo", fileName = "CharacterInfo")]
    public class CharacterInfoSO : ScriptableObject
    {
        [Tooltip("角色的唯一标识")]
        [SerializeField] private string _characterId;

        [Tooltip("角色所属阵营")]
        [SerializeField] private CharacterFaction _faction;

        /// <summary>角色的唯一标识。</summary>
        public string CharacterId => _characterId;

        /// <summary>角色所属阵营。</summary>
        public CharacterFaction Faction => _faction;
    }
}
