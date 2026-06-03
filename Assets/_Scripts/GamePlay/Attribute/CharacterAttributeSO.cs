using UnityEngine;

namespace GamePlay.Attribute
{
    /// <summary>角色初始属性配置 ScriptableObject，定义基础属性值</summary>
    [CreateAssetMenu(menuName = "Character/Attribute Config")]
    public class CharacterAttributeSO : ScriptableObject
    {
        [Tooltip("最大生命值")]
        public float MaxHealth = 100f;

        [Tooltip("攻击力")]
        public float Attack = 10f;

        [Tooltip("防御力")]
        public float Defense = 5f;

        [Tooltip("移动速度")]
        public float MoveSpeed = 5f;
    }
}
