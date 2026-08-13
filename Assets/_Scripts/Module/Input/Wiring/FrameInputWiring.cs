using UnityEngine;

using SPFramework.Service;
using SPInput.Contract;
using SPInput.Core;

namespace SPInput.Wiring
{
    /// <summary>
    /// 输入接线胶水 - 采集数据注册到模块服务中心
    /// </summary>
    [DefaultExecutionOrder(-390)]
    internal sealed class FrameInputWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("帧输入采集器，通常与胶水挂于同一根物体")]
        [SerializeField] private FrameInputCollector _collector;

        private void Awake()
        {
            if (_collector != null)
                ModuleServiceHub.Register<IProvideFrameInput>(_collector);
        }

        private void OnDestroy()
        {
            ModuleServiceHub.Unregister<IProvideFrameInput>();
        }
    }
}
