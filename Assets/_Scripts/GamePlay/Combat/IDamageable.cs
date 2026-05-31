using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>可受击接口，由敌人、可破坏物等实现</summary>
    public interface IDamageable
    {
        /// <summary>受击对象的 Transform</summary>
        Transform Transform { get; }

        /// <summary>处理受击逻辑</summary>
        /// <param name="damageInfo">伤害信息，包含数值、位置、击退方向与来源</param>
        void TakeDamage(DamageInfo damageInfo);
    }
}
