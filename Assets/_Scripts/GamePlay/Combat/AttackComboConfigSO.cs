using System;
using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>单段攻击的配置数据</summary>
    [Serializable]
    public struct AttackSegmentConfig
    {
        [Tooltip("本段攻击对应动画哈希值")]
        public int AnimationHash;

        [Tooltip("伤害值")]
        public float Damage;

        [Tooltip("击退力")]
        public float KnockbackForce;

        [Tooltip("判定窗口开始时间（归一化 0~1）")]
        [Range(0f, 1f)]
        public float HitboxActiveStart;

        [Tooltip("判定窗口结束时间（归一化 0~1）")]
        [Range(0f, 1f)]
        public float HitboxActiveEnd;
    }

    /// <summary>
    /// 连击攻击配置 ScriptableObject，按连击段顺序存储每段参数
    /// </summary>
    [CreateAssetMenu(menuName = "Combat/Attack Combo Config")]
    public class AttackComboConfigSO : ScriptableObject
    {
        [Tooltip("按连击段顺序排列的攻击配置")]
        public AttackSegmentConfig[] Segments;
    }
}
