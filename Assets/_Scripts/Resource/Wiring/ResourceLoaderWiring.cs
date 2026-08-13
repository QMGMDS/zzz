using UnityEngine;

using SPResource.Core;

namespace SPResource.Wiring
{
    /// <summary>
    /// 资源加载接线胶水 - 将内部加载服务注入对外信箱
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class ResourceLoaderWiring : MonoBehaviour
    {
        [Header("接线")]
        [SerializeField, Tooltip("资源加载服务")]
        private ResourceLoadService _loadService;

        [SerializeField, Tooltip("存放资源实例化能力的信箱")]
        private ResourceLoaderProviderSO _providerSO;

        private void Awake()
        {
            if (_loadService != null && _providerSO != null)
                _providerSO.Bind(_loadService);
        }

        private void OnDestroy()
        {
            if (_providerSO != null)
                _providerSO.Clear();
        }
    }
}
