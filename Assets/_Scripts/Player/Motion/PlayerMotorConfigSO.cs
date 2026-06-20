using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家移动配置——定义动画根位移倍率和重力参数。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMotorConfig", menuName = "Player/PlayerMotorConfig")]
    public class PlayerMotorConfigSO : ScriptableObject
    {
        [Tooltip("默认动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _defaultRootMotionScale = 1f;

        [Tooltip("行走动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _walkRootMotionScale = 1f;

        [Tooltip("奔跑动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _runRootMotionScale = 1f;

        [Tooltip("停止动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _stopRootMotionScale = 1f;

        [Tooltip("前闪动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _evadeFrontRootMotionScale = 1f;

        [Tooltip("后闪动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _evadeBackRootMotionScale = 1f;

        [Tooltip("攻击动画根位移倍率")]
        [Min(0f)]
        [SerializeField] private float _attackRootMotionScale = 1f;

        [Tooltip("角色转向速度（度/秒）")]
        [Min(0f)]
        [SerializeField] private float _rotationSpeed = 1000f;

        [Tooltip("重力加速度")]
        [SerializeField] private float _gravity = -20f;

        [Tooltip("贴地速度，角色接地时施加轻微向下速度")]
        [Min(0f)]
        [SerializeField] private float _groundStickVelocity = 2f;

        /// <summary>默认动画根位移倍率。</summary>
        public float DefaultRootMotionScale => _defaultRootMotionScale;

        /// <summary>行走动画根位移倍率。</summary>
        public float WalkRootMotionScale => _walkRootMotionScale;

        /// <summary>奔跑动画根位移倍率。</summary>
        public float RunRootMotionScale => _runRootMotionScale;

        /// <summary>停止动画根位移倍率。</summary>
        public float StopRootMotionScale => _stopRootMotionScale;

        /// <summary>前闪动画根位移倍率。</summary>
        public float EvadeFrontRootMotionScale => _evadeFrontRootMotionScale;

        /// <summary>后闪动画根位移倍率。</summary>
        public float EvadeBackRootMotionScale => _evadeBackRootMotionScale;

        /// <summary>攻击动画根位移倍率。</summary>
        public float AttackRootMotionScale => _attackRootMotionScale;

        /// <summary>角色转向速度（度/秒）。</summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>重力加速度。</summary>
        public float Gravity => _gravity;

        /// <summary>贴地速度，角色接地时施加轻微向下速度。</summary>
        public float GroundStickVelocity => _groundStickVelocity;
    }
}
