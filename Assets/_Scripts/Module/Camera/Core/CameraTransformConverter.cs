using UnityEngine;

using SPCamera.Contract;

namespace SPCamera.Core
{
    /// <summary>
    /// 相机坐标系移动方向转换器 - 以参考物体 yaw 为基准
    /// 契约方法由外部显示调用，无需考虑脚本时序问题
    /// </summary>
    internal sealed class CameraTransformConverter : MonoBehaviour, IConvertCameraTransform
    {
        [Header("参考系")]
        [Tooltip("持 yaw 的参考物体（通常为相机根或其父级），留空时使用本物体自身")]
        [SerializeField] private Transform _reference;

        private Transform Reference => _reference != null ? _reference : transform;

        /// <inheritdoc />
        public Vector2 ConvertCameraTransform(Vector2 inputDirection)
        {
            // 不对 inputDirection 进行防御性处理，方法的调用者需保证其合法

            Quaternion yawRotation = Quaternion.Euler(0f, Reference.eulerAngles.y, 0f);

            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;

            Vector3 world = forward * inputDirection.y + right * inputDirection.x;
            Vector2 xz = new Vector2(world.x, world.z);
            return xz.normalized;
        }
    }
}
