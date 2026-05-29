using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>可受击接口，由敌人、可破坏物等实现</summary>
    public interface IDamageable
    {
        Transform Transform { get; }

        void TakeDamage(DamageInfo damageInfo);
    }
}
