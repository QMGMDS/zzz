using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家移动配置——定义代码驱动位移时的基础速度、转向和重力参数。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMotorConfig", menuName = "Player/PlayerMotorConfig")]
    public class PlayerMotorConfigSO : ScriptableObject
    {
        [Tooltip("行走基础速度（米/秒）")]
        [Min(0f)]
        [SerializeField] private float _walkSpeed = 2.2f;

        [Tooltip("奔跑基础速度（米/秒）")]
        [Min(0f)]
        [SerializeField] private float _runSpeed = 4.8f;

        [Tooltip("停止动画保留的滑行速度（米/秒）")]
        [Min(0f)]
        [SerializeField] private float _stopSpeed = 1.2f;

        [Tooltip("根据动画水平 Root Delta 计算速度倍率时使用的参考速度")]
        [Min(0.01f)]
        [SerializeField] private float _referenceRootSpeed = 1.5f;

        [Tooltip("动画运动倍率下限，防止 Root Delta 过小导致角色完全不动")]
        [Min(0f)]
        [SerializeField] private float _minMotionScale = 0.35f;

        [Tooltip("动画运动倍率上限，防止 Root Delta 尖峰导致角色瞬移")]
        [Min(0f)]
        [SerializeField] private float _maxMotionScale = 1.4f;

        [Tooltip("动画运动倍率平滑速度")]
        [Min(0f)]
        [SerializeField] private float _motionScaleSmoothSpeed = 18f;

        [Tooltip("角色转向速度（度/秒）")]
        [Min(0f)]
        [SerializeField] private float _rotationSpeed = 720f;

        [Tooltip("重力加速度")]
        [SerializeField] private float _gravity = -20f;

        [Tooltip("贴地速度，角色接地时施加轻微向下速度")]
        [Min(0f)]
        [SerializeField] private float _groundStickVelocity = 2f;

        /// <summary>行走基础速度（米/秒）。</summary>
        public float WalkSpeed => _walkSpeed;

        /// <summary>奔跑基础速度（米/秒）。</summary>
        public float RunSpeed => _runSpeed;

        /// <summary>停止动画保留的滑行速度（米/秒）。</summary>
        public float StopSpeed => _stopSpeed;

        /// <summary>根据动画水平 Root Delta 计算速度倍率时使用的参考速度。</summary>
        public float ReferenceRootSpeed => _referenceRootSpeed;

        /// <summary>动画运动倍率下限，防止 Root Delta 过小导致角色完全不动。</summary>
        public float MinMotionScale => _minMotionScale;

        /// <summary>动画运动倍率上限，防止 Root Delta 尖峰导致角色瞬移。</summary>
        public float MaxMotionScale => _maxMotionScale;

        /// <summary>动画运动倍率平滑速度。</summary>
        public float MotionScaleSmoothSpeed => _motionScaleSmoothSpeed;

        /// <summary>角色转向速度（度/秒）。</summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>重力加速度。</summary>
        public float Gravity => _gravity;

        /// <summary>贴地速度，角色接地时施加轻微向下速度。</summary>
        public float GroundStickVelocity => _groundStickVelocity;
    }
}
