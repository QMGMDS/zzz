using GamePlay.Combat;
using UnityEngine;

namespace GamePlay.Enemy
{
    /// <summary>
    /// 敌人控制器，实现 IDamageable 以响应受击，预留行为树挂载点供后续 AI 驱动
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Animator))]
    public class EnemyController : MonoBehaviour, IDamageable
    {
        [Tooltip("最大生命值")]
        [SerializeField] private float _maxHealth = 100f;

        [Tooltip("当前生命值")]
        [SerializeField] private float _currentHealth = 100f;

        private Animator _animator;

        /// <summary>供行为树读取的受击标记，TakeDamage 时设为 true，PlayHitAnimation 播完后清 false</summary>
        public bool isHitRequested { get; set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        #region IDamageable

        /// <inheritdoc cref="IDamageable.Transform"/>
        public Transform Transform => transform;

        /// <inheritdoc cref="IDamageable.TakeDamage"/>
        public void TakeDamage(DamageInfo damageInfo)
        {
            _currentHealth -= damageInfo.Amount;
            _animator.SetTrigger("Hit");
            isHitRequested = true;
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
