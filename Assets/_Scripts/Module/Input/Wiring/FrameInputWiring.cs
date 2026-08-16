using UnityEngine;

using SPFramework.Service;
using SPInput.Contract;
using SPInput.Core;

namespace SPInput.Wiring
{
    /// <summary>
    /// 帧输入接线胶水 - 实现帧输入提供契约并注册到模块服务中心，访问转发给帧输入采集器
    /// </summary>
    [DefaultExecutionOrder(-390)]
    internal sealed class FrameInputWiring : MonoBehaviour, IProvideFrameInput
    {
        [Header("接线")]
        [Tooltip("帧输入采集器，通常与胶水挂于同一根物体")]
        [SerializeField] private FrameInputCollector _collector;

        private void OnEnable()
        {
            if (_collector != null)
                ModuleServiceHub.Register<IProvideFrameInput>(this);
        }

        private void OnDisable()
        {
            if (_collector != null)
                ModuleServiceHub.Unregister<IProvideFrameInput>(this);
        }

        /// <inheritdoc />
        public RawFrameInput CurrentFrame => _collector != null ? _collector.CurrentFrame : default;

        /// <inheritdoc />
        public ProcessedFrameInput CurrentProcessed => _collector != null ? _collector.CurrentProcessed : default;
    }
}
