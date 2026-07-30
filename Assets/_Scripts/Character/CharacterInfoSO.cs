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
    /// 角色基础信息
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/Info/CharacterInfo", fileName = "CharacterInfo")]
    public class CharacterInfoSO : ScriptableObject
    {
        [Tooltip("角色的唯一标识")]
        [SerializeField] private string _characterId;

        [Tooltip("角色所属阵营")]
        [SerializeField] private CharacterFaction _faction;

        [Tooltip("角色头像")]
        [SerializeField] private Sprite _avatar;

        [Tooltip("角色人物属性资产")]
        [SerializeField] private CharacterStatsSO _stats;

        [Tooltip("角色对应的预制体。")]
        [SerializeField] private GameObject _prefab;

        /// <summary>角色的唯一标识。</summary>
        public string CharacterId => _characterId;

        /// <summary>角色所属阵营。</summary>
        public CharacterFaction Faction => _faction;

        /// <summary>角色头像。</summary>
        public Sprite Avatar => _avatar;

        /// <summary>角色人物属性资产。</summary>
        public CharacterStatsSO Stats => _stats;

        /// <summary>角色对应的预制体。</summary>
        public GameObject Prefab => _prefab;
    }
}
