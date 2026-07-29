using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色属性静态数据 - 纯数据资产，定义角色基础生命值与基础攻击力。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/Stats/CharacterStats", fileName = "CharacterStats")]
    public class CharacterStatsSO : ScriptableObject
    {
        [Tooltip("基础生命值")]
        [SerializeField] private int _maxHP;

        [Tooltip("基础攻击力")]
        [SerializeField] private int _attack;

        /// <summary>基础生命值。</summary>
        public int MaxHP => _maxHP;

        /// <summary>基础攻击力。</summary>
        public int Attack => _attack;
    }
}