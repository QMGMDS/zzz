using System.Collections.Generic;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源实例化能力 - 根据资源键同步创建预制体实例
    /// </summary>
    public interface IInstantiateResource
    {
        /// <summary>
        /// 同步实例化单个资源
        /// </summary>
        /// <param name="request">资源实例化请求</param>
        /// <returns>资源加载结果</returns>
        ResourceLoadResult Instantiate(ResourceLoadRequest request);

        /// <summary>
        /// 同步批量实例化资源
        /// </summary>
        /// <param name="requests">资源实例化请求列表</param>
        /// <returns>资源加载结果列表</returns>
        IReadOnlyList<ResourceLoadResult> InstantiateBatch(IReadOnlyList<ResourceLoadRequest> requests);
    }
}
