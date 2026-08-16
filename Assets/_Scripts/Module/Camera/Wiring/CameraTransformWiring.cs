using UnityEngine;

using SPCamera.Contract;
using SPCamera.Core;
using SPFramework.Service;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 相机坐标转换接线胶水 - 实现坐标转换契约并注册到模块服务中心，调用转发给摄像机主入口
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class CameraTransformWiring : MonoBehaviour, IConvertCameraTransform
    {
        [Header("接线")]
        [Tooltip("摄像机主入口")]
        [SerializeField] private SPCameraMain _main;

        private void OnEnable()
        {
            ModuleServiceHub.Register<IConvertCameraTransform>(this);
        }

        private void OnDisable()
        {
            ModuleServiceHub.Unregister<IConvertCameraTransform>(this);
        }

        /// <inheritdoc />
        public Vector2 ConvertCameraTransform(Vector2 inputDirection)
        {
            // 主入口未接好线时静默降级：直接返回输入方向，与空源保护语义一致
            return _main != null
                ? _main.ConvertCameraTransform(inputDirection)
                : inputDirection;
        }
    }
}
