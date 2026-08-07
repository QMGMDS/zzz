using SPCamera.Core;
using UnityEngine;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 相机坐标转换器接线胶水 - 得到相机系移动方向。
    /// </summary>
    [DefaultExecutionOrder(-380)]
    public class CameraCoordinateConverterWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("坐标转换器，实现 ICoordinateConverter，通常与胶水挂于同一相机根物体。")]
        [SerializeField] private CameraMoveAxisConverter _converter;

        [Tooltip("坐标转换器提供者槽位 SO，运行时信箱，下游据此 pull。")]
        [SerializeField] private CoordinateConverterProviderSO _providerSO;

        private void Awake()
        {
            if (_converter != null && _providerSO != null)
                _providerSO.Bind(_converter);   // 隐式转为 ICoordinateConverter
        }

        private void OnDestroy()
        {
            if (_providerSO != null) _providerSO.Clear();
        }
    }
}
