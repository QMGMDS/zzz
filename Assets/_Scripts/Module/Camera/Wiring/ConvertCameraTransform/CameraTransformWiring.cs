using UnityEngine;

using SPCamera.Contract;
using SPCamera.Core;
using SPFramework.Service;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 相机坐标转换器接线胶水 - 将坐标转换器注册到模块服务中心
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class CameraTransformWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("坐标转换器，通常与胶水挂于同一根物体")]
        [SerializeField] private CameraTransformConverter _converter;

        private void Awake()
        {
            if (_converter != null)
                ModuleServiceHub.Register<IConvertCameraTransform>(_converter);
        }

        private void OnDestroy()
        {
            ModuleServiceHub.Unregister<IConvertCameraTransform>();
        }
    }
}
