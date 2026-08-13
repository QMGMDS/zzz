using UnityEngine;

using SPResource.Contract;

namespace SPResource.Wiring
{
    /// <summary>
    /// 资源加载信箱 - 存储资源实例化能力提供者
    /// </summary>
    [CreateAssetMenu(menuName = "SPResource/Resource Loader Provider", fileName = "ResourceLoaderProvider")]
    public sealed class ResourceLoaderProviderSO : ScriptableObject
    {
        private IInstantiateResource _provider;

        /// <summary>当前注入的资源实例化能力提供者</summary>
        public IInstantiateResource Provider => _provider;

        /// <summary>
        /// 接线胶水专用 - 注入资源实例化能力提供者
        /// </summary>
        /// <param name="provider">资源实例化能力提供者</param>
        internal void Bind(IInstantiateResource provider)
        {
            if (provider == null) return;
            if (_provider != null && !ReferenceEquals(_provider, provider))
                Debug.LogWarning(
                    $"ResourceLoaderProviderSO: 已注入提供者 [{_provider}]，现又被覆盖为 [{provider}]" +
                    "本槽位仅支持单加载器注入，请避免多实例接线同一份 SO 资产");

            _provider = provider;
        }

        /// <summary>
        /// 接线胶水专用 - 清空资源实例化能力提供者
        /// </summary>
        internal void Clear()
        {
            _provider = null;
        }
    }
}
