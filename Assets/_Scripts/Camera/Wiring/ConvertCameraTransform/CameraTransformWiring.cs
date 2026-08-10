using UnityEngine;

using SPCamera.Core;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 相机坐标转换器接线胶水 - 得到关联相机系的世界移动方向
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class CameraTransformWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("坐标转换器，通常与胶水挂于同一根物体")]
        [SerializeField] private CameraTransformConverter _converter;

        [Tooltip("存放信箱")]
        [SerializeField] private CameraTransformProviderSO _providerSO;

        private void Awake()
        {
            if (_converter != null && _providerSO != null)
                _providerSO.Bind(_converter);   // 隐式转为 IConvertCameraTransform
        }

        private void OnDestroy()
        {
            if (_providerSO != null) _providerSO.Clear();
        }
    }
}
