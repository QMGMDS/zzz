using System;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色移动配置 - 保存代码驱动移动、转向和重力参数。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/Motion/MotionConfig", fileName = "CharacterMotionConfig")]
    public class CharacterMotionConfigSO : ScriptableObject
    {
        [Header("平面移动")]
        [Tooltip("角色最大平面移动速度（米/秒）")]
        [Min(0f)]
        [SerializeField] private float _maxMoveSpeed = 3.5f;

        [Tooltip("角色向目标速度加速的速率（米/秒平方）")]
        [Min(0f)]
        [SerializeField] private float _acceleration = 16f;

        [Tooltip("角色停止输入后减速的速率（米/秒平方）")]
        [Min(0f)]
        [SerializeField] private float _deceleration = 22f;

        [Header("转向")]
        [Tooltip("角色每秒最大转向角度")]
        [Min(0f)]
        [SerializeField] private float _rotationSpeed = 720f;

        [Header("垂直移动")]
        [Tooltip("角色向下加速的绝对值（米/秒平方）")]
        [Min(0f)]
        [SerializeField] private float _gravityAcceleration = 25f;

        [Tooltip("角色着地时用于保持贴地的垂直速度")]
        [SerializeField] private float _groundedVerticalSpeed = -2f;

        [Tooltip("角色最大下落速度的绝对值（米/秒）")]
        [Min(0f)]
        [SerializeField] private float _maxFallSpeed = 35f;

        /// <summary>角色最大平面移动速度。</summary>
        public float MaxMoveSpeed => _maxMoveSpeed;

        /// <summary>角色加速度。</summary>
        public float Acceleration => _acceleration;

        /// <summary>角色减速度。</summary>
        public float Deceleration => _deceleration;

        /// <summary>角色每秒最大转向角度。</summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>角色重力加速度绝对值。</summary>
        public float GravityAcceleration => _gravityAcceleration;

        /// <summary>角色着地贴地速度。</summary>
        public float GroundedVerticalSpeed => _groundedVerticalSpeed;

        /// <summary>角色最大下落速度绝对值。</summary>
        public float MaxFallSpeed => _maxFallSpeed;

        /// <summary>
        /// 校验移动配置是否可用于运行时。
        /// </summary>
        /// <exception cref="InvalidOperationException">配置值不合法时抛出。</exception>
        public void Validate()
        {
            if (_maxMoveSpeed < 0f) throw new InvalidOperationException("最大移动速度不能小于 0。");
            if (_acceleration < 0f) throw new InvalidOperationException("加速度不能小于 0。");
            if (_deceleration < 0f) throw new InvalidOperationException("减速度不能小于 0。");
            if (_rotationSpeed < 0f) throw new InvalidOperationException("转向速度不能小于 0。");
            if (_gravityAcceleration < 0f) throw new InvalidOperationException("重力加速度不能小于 0。");
            if (_groundedVerticalSpeed > 0f) throw new InvalidOperationException("着地垂直速度不能大于 0。");
            if (_maxFallSpeed < 0f) throw new InvalidOperationException("最大下落速度不能小于 0。");
        }
    }
}
