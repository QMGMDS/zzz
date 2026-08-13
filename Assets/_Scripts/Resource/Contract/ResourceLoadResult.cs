using UnityEngine;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源加载结果 - 返回实例与释放句柄
    /// </summary>
    public readonly struct ResourceLoadResult
    {
        /// <summary>请求使用的资源键</summary>
        public ResourceKey Key { get; }

        /// <summary>加载是否成功</summary>
        public bool IsSuccess { get; }

        /// <summary>加载出的实例</summary>
        public GameObject Instance { get; }

        /// <summary>实例释放句柄</summary>
        public IResourceHandle Handle { get; }

        /// <summary>失败原因</summary>
        public string ErrorMessage { get; }

        private ResourceLoadResult(
            ResourceKey key,
            bool isSuccess,
            GameObject instance,
            IResourceHandle handle,
            string errorMessage)
        {
            Key = key;
            IsSuccess = isSuccess;
            Instance = instance;
            Handle = handle;
            ErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="key">请求使用的资源键</param>
        /// <param name="instance">加载出的实例</param>
        /// <param name="handle">实例释放句柄</param>
        /// <returns>成功结果</returns>
        internal static ResourceLoadResult Success(ResourceKey key, GameObject instance, IResourceHandle handle)
        {
            return new ResourceLoadResult(key, true, instance, handle, string.Empty);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="key">请求使用的资源键</param>
        /// <param name="errorMessage">失败原因</param>
        /// <returns>失败结果</returns>
        internal static ResourceLoadResult Failure(ResourceKey key, string errorMessage)
        {
            return new ResourceLoadResult(key, false, null, null, errorMessage);
        }
    }
}
