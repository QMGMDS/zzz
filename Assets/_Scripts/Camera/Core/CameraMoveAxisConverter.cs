using SPCamera.Contract;
using UnityEngine;

namespace SPCamera.Core
{
    /// <summary>
    /// 相机坐标系移动方向转换器 - 以参考物体 yaw 为基准
    /// </summary>
    public class CameraMoveAxisConverter : MonoBehaviour, ICoordinateConverter
    {
        private const float DirectionEpsilon = 1e-6f;

        [Header("参考系")]
        [Tooltip("持 yaw 的参考物体（通常为相机根或其父级）。留空时使用本物体自身。")]
        [SerializeField] private Transform _reference;

        private Transform Reference => _reference != null ? _reference : transform;

        /// <inheritdoc />
        public Vector2 ConvertToWorldMoveDirection(Vector2 inputDirection)
        {
            if (inputDirection.sqrMagnitude <= DirectionEpsilon)
                return Vector2.zero;

            Vector3 forward = Reference.forward;
            Vector3 right = Reference.right;
            forward.y = 0f;
            right.y = 0f;

            Vector3 world = forward * inputDirection.y + right * inputDirection.x;
            Vector2 xz = new Vector2(world.x, world.z);
            return xz.sqrMagnitude > DirectionEpsilon ? xz.normalized : Vector2.zero;
        }
    }
}
