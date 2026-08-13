using System;
using System.Collections.Generic;
using UnityEngine;

using SPResource.Contract;

namespace SPResource.Core
{
    /// <summary>
    /// 资源加载服务 - 通过资源目录同步实例化预制体
    /// </summary>
    internal sealed class ResourceLoadService : MonoBehaviour, IInstantiateResource
    {
        [Header("资源目录")]
        [SerializeField, Tooltip("资源键到预制体的运行时查询目录")]
        private ResourceCatalogSO _catalog;

        [Header("诊断")]
        [SerializeField, Tooltip("加载失败时是否输出警告日志")]
        private bool _shouldLogFailures = true;

        /// <inheritdoc />
        public ResourceLoadResult Instantiate(ResourceLoadRequest request)
        {
            return InstantiateInternal(request);
        }

        /// <inheritdoc />
        public IReadOnlyList<ResourceLoadResult> InstantiateBatch(IReadOnlyList<ResourceLoadRequest> requests)
        {
            if (requests == null)
                return Array.Empty<ResourceLoadResult>();

            List<ResourceLoadResult> results = new List<ResourceLoadResult>(requests.Count);
            for (int i = 0; i < requests.Count; i++)
            {
                ResourceLoadResult result = InstantiateInternal(requests[i]);
                results.Add(result);
            }

            return results;
        }

        private ResourceLoadResult InstantiateInternal(ResourceLoadRequest request)
        {
            if (!request.Key.IsValid)
                return Fail(request.Key, "资源键为空");

            if (_catalog == null)
                return Fail(request.Key, "资源目录未配置");

            if (!_catalog.TryGetPrefab(request.Key, out GameObject prefab))
                return Fail(request.Key, $"资源目录中找不到可用预制体 Key={request.Key}");

            try
            {
                GameObject instance = UnityEngine.Object.Instantiate(
                    prefab,
                    request.WorldPosition,
                    request.WorldRotation,
                    request.Parent);

                instance.SetActive(request.ShouldActivateAfterCreate);
                PrefabResourceHandle handle = new PrefabResourceHandle(request.Key, instance);
                return ResourceLoadResult.Success(request.Key, instance, handle);
            }
            catch (Exception exception)
            {
                return Fail(request.Key, exception.Message);
            }
        }

        private ResourceLoadResult Fail(ResourceKey key, string errorMessage)
        {
            if (_shouldLogFailures)
                Debug.LogWarning($"ResourceLoadService: {errorMessage}", this);

            return ResourceLoadResult.Failure(key, errorMessage);
        }
    }
}
