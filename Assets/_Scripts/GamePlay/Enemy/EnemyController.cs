using GamePlay.Attribute;
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
        [Tooltip("角色初始属性配置 SO")]
        [SerializeField] private CharacterAttributeSO _attributeConfig;

        private CharacterAttributes _attributes;
        private Animator _animator;
        private float _currentHealth;

        /// <summary>供行为树读取的受击标记，TakeDamage 时设为 true，PlayHitAnimation 播完后清 false</summary>
        public bool isHitRequested { get; set; }

        /// <summary>角色属性只读接口，供战斗系统等外部模块读取</summary>
        public IAttributeProvider Attributes => _attributes;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _attributes = new CharacterAttributes(_attributeConfig);
            _currentHealth = _attributes.GetAttribute(AttributeType.MaxHealth);
        }

        #region IDamageable

        /// <inheritdoc cref="IDamageable.Transform"/>
        public Transform Transform => transform;

        /// <inheritdoc cref="IDamageable.TakeDamage"/>
        public void TakeDamage(DamageInfo damageInfo)
        {
            float maxHealth = _attributes.GetAttribute(AttributeType.MaxHealth);
            _currentHealth -= damageInfo.Amount;
            _animator.SetTrigger("Hit");
            isHitRequested = true;
            Debug.Log($"[{name}] 受到 {damageInfo.Amount} 点伤害，剩余 {_currentHealth}/{maxHealth} HP");

            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                Die();
            }
        }

        #endregion

        private void Die()
        {
            Debug.Log($"[{name}] 死亡");
        }
    }
}
