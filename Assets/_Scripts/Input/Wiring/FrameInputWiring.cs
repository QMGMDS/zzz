using UnityEngine;

using SPInput.Core;

namespace SPInput.Wiring
{
    /// <summary>
    /// 输入接线胶水 - 采集数据写入信箱
    /// </summary>
    [DefaultExecutionOrder(-390)]
    internal sealed class FrameInputWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("帧输入采集器，通常与胶水挂于同一根物体")]
        [SerializeField] private FrameInputCollector _collector;

        [Tooltip("存放信箱")]
        [SerializeField] private FrameInputProviderSO _providerSO;

        private void Awake()
        {
            if (_collector != null && _providerSO != null)
                _providerSO.Bind(_collector);   // 隐式转为 IProvideFrameInput
        }

        private void OnDestroy()
        {
            if (_providerSO != null) _providerSO.Clear();
        }
    }
}