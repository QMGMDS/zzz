using SPInput_Core;
using UnityEngine;

namespace SPInput_Wiring
{
    /// <summary>
    /// 输入接线胶水 - 让下游系统跨场景 pull 采集数据
    /// </summary>
    [DefaultExecutionOrder(-390)]
    public class InputFrameWiring : MonoBehaviour
    {
        [SerializeField] private FrameInputCollector _collector;
        [SerializeField] private FrameInputProviderSO _providerSO;

        private void Awake()
        {
            if (_collector != null && _providerSO != null)
                _providerSO.Bind(_collector);   // 隐式转为 IFrameInputProvider
        }

        private void OnDestroy()
        {
            if (_providerSO != null) _providerSO.Clear();
        }
    }
}
