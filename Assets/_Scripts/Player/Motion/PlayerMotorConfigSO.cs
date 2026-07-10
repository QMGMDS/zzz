using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家移动配置——定义旋转速度和重力参数。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMotorConfig", menuName = "Player/PlayerMotorConfig")]
    public class PlayerMotorConfigSO : ScriptableObject
    {
        [Tooltip("角色转向速度（度/秒）")]
        [Min(0f)]
        [SerializeField] private float _rotationSpeed = 1000f;

        [Tooltip("重力加速度")]
        [SerializeField] private float _gravity = -20f;

        [Tooltip("贴地速度，角色接地时施加轻微向下速度")]
        [Min(0f)]
        [SerializeField] private float _groundStickVelocity = 2f;

        /// <summary>角色转向速度（度/秒）。</summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>重力加速度。</summary>
        public float Gravity => _gravity;

        /// <summary>贴地速度，角色接地时施加轻微向下速度。</summary>
        public float GroundStickVelocity => _groundStickVelocity;
    }
}
