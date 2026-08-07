using SPInput.Core;
using UnityEngine;

namespace SPInput.Wiring
{
    /// <summary>
    /// 输入接线胶水 - 让下游系统跨场景 pull 采集数据。
    /// 在 Awake 将 Collector 注入 ProviderSO 槽位，OnDestroy 清空。
    /// </summary>
    [DefaultExecutionOrder(-390)]
    public class InputFrameWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("帧输入采集器，实现 IFrameInputProvider，运行时只读引用。")]
        [SerializeField] private FrameInputCollector _collector;

        [Tooltip("帧输入提供者槽位 SO，运行时信箱，下游据此 pull 当前帧。")]
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