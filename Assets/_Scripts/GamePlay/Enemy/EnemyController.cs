using GamePlay.Combat;
using UnityEngine;

namespace GamePlay.Enemy
{
    /// <summary>
    /// 敌人控制器，实现 IDamageable 以响应受击，预留行为树挂载点供后续 AI 驱动
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [Tooltip("最大生命值")]
        [SerializeField] private float _maxHealth = 100f;

        [Tooltip("当前生命值")]
        [SerializeField] private float _currentHealth = 100f;

        [Tooltip("行为树组件引用（预留，当前为空）")]
        [SerializeField] private MonoBehaviour _behaviourTree;

        #region IDamageable

        /// <inheritdoc cref="IDamageable.Transform"/>
        public Transform Transform => transform;

        /// <inheritdoc cref="IDamageable.TakeDamage"/>
        public void TakeDamage(DamageInfo damageInfo)
        {
            _currentHealth -= damageInfo.Amount;
            Debug.Log($"[{name}] 受到 {damageInfo.Amount} 点伤害，剩余 {_currentHealth}/{_maxHealth} HP");

            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                Die();
            }
        }

        #endregion

        private void OnValidate()
        {
            if (_currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
        }

        private void Die()
        {
            Debug.Log($"[{name}] 死亡");
        }
    }
}
