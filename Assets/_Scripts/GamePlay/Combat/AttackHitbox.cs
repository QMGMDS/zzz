using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Combat
{
    /// <summary>攻击碰撞体，单次激活周期内对同一目标仅命中一次</summary>
    [RequireComponent(typeof(Collider))]
    public class AttackHitbox : MonoBehaviour
    {
        [Tooltip("默认伤害值，运行时可由 SO 配置覆盖")]
        [SerializeField] private float _damage = 10f;

        private Collider _collider;
        private readonly HashSet<IDamageable> _hitTargets = new();
        private Transform _sourceRoot;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
            _collider.enabled = false;
            _sourceRoot = transform.root;
        }

        /// <summary>覆盖当前段伤害数据</summary>
        /// <param name="damage">伤害数值</param>
        public void SetDamage(float damage)
        {
            _damage = damage;
        }

        /// <summary>启用碰撞体并清空去重集合，进入判定窗口时调用</summary>
        public void Enable()
        {
            _hitTargets.Clear();
            _collider.enabled = true;
        }

        /// <summary>关闭碰撞体，退出判定窗口时调用</summary>
        public void Disable()
        {
            _collider.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
            if (!_hitTargets.Add(damageable)) return;
            if (damageable.Transform.root == _sourceRoot) return;

            Vector3 hitPoint = _collider.ClosestPoint(other.transform.position);

            var info = new DamageInfo
            {
                Amount = _damage,
                HitPoint = hitPoint,
                Source = _sourceRoot.gameObject
            };

            damageable.TakeDamage(info);
        }
    }
}
