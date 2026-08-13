using UnityEngine;

using SPFramework.Service;
using SPResource.Contract;
using SPResource.Core;

namespace SPResource.Wiring
{
    /// <summary>
    /// 资源加载接线胶水 - 将内部加载服务注册到模块服务中心
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class ResourceLoaderWiring : MonoBehaviour
    {
        [Header("接线")]
        [SerializeField, Tooltip("资源加载服务")]
        private ResourceLoadService _loadService;

        private void Awake()
        {
            if (_loadService != null)
                ModuleServiceHub.Register<IInstantiateResource>(_loadService);
        }

        private void OnDestroy()
        {
            ModuleServiceHub.Unregister<IInstantiateResource>();
        }
    }
}
