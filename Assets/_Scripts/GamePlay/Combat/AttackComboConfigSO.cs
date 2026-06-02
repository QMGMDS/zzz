using System;
using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>
    /// 单次特效生成配置：定义在动画的哪个时间点触发，以及该次特效的局部旋转角度。
    /// </summary>
    [Serializable]
    public struct EffectSpawnInfo
    {
        [Tooltip("特效触发时刻（归一化时间 0~1）")]
        [Range(0f, 1f)]
        public float NormalizedTime;

        [Tooltip("该特效相对于挂点的局部偏移位置")]
        public Vector3 LocalPosition;

        [Tooltip("该特效相对于挂点的局部旋转角度")]
        public Vector3 LocalRotation;
    }

    /// <summary>单次判定窗口：对应动画中一次挥剑的伤害窗口</summary>
    [Serializable]
    public struct HitWindow
    {
        [Tooltip("判定窗口开始时间（归一化 0~1）")]
        [Range(0f, 1f)]
        public float StartNormalizedTime;

        [Tooltip("判定窗口结束时间（归一化 0~1）")]
        [Range(0f, 1f)]
        public float EndNormalizedTime;

        [Tooltip("本次挥剑的伤害值")]
        public float Damage;

        [Tooltip("本次挥剑的击退力")]
        public float KnockbackForce;

        [Tooltip("本次判定触发时的震屏力度，0 为不震动")]
        [Range(0f, 5f)]
        public float ShakeForce;
    }

    /// <summary>单段攻击的配置数据</summary>
    [Serializable]
    public struct AttackSegmentConfig
    {
        [Tooltip("本段攻击对应动画哈希值")]
        public int AnimationHash;

        [Tooltip("判定窗口数组，每项对应一次挥剑")]
        public HitWindow[] HitWindows;

        [Tooltip("特效触发配置数组，每项定义一次特效的触发时刻和旋转角度")]
        public EffectSpawnInfo[] EffectSpawns;
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
