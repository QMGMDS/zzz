using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>传递到 TakeDamage 的伤害信息</summary>
    public struct DamageInfo
    {
        /// <summary>伤害数值</summary>
        public float Amount;

        /// <summary>受击点世界坐标</summary>
        public Vector3 HitPoint;

        /// <summary>伤害来源 GameObject</summary>
        public GameObject Source;
    }
}