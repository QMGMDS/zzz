using System;

using UnityEngine;

using SPFramework.Service;
using SPResource.Contract;
using SPResource.Core;

namespace SPResource.Wiring
{
    /// <summary>
    /// 资源实例化接线胶水 - 实现资源实例化契约并注册到模块服务中心，调用转发给资源主入口
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class ResourceInstantiateWiring : MonoBehaviour, IInstantiateResource
    {
        [Header("接线")]
        [SerializeField, Tooltip("资源主入口")]
        private SPResourceMain _main;

        private void OnEnable()
        {
            if (_main != null)
                ModuleServiceHub.Register<IInstantiateResource>(this);
        }

        private void OnDisable()
        {
            if (_main != null)
                ModuleServiceHub.Unregister<IInstantiateResource>(this);
        }

        /// <inheritdoc />
        public ResourceInstantiateResult Instantiate(ResourceKey key, Transform parent = null, bool activate = true)
        {
            // 主入口未接好线时降级为失败结果，与空源保护语义一致
            if (_main == null)
                return ResourceInstantiateResult.Failure(ResourceInstantiateError.ServiceUnavailable);

            GameObject instance = _main.Instantiate(key, parent, activate, out ResourceInstantiateError error);
            return ToResult(instance, error);
        }

        /// <inheritdoc />
        public ResourceInstantiateResult Instantiate(ResourceKey key, Vector3 worldPosition, Quaternion worldRotation, Transform parent = null, bool activate = true)
        {
            // 主入口未接好线时降级为失败结果，与空源保护语义一致
            if (_main == null)
                return ResourceInstantiateResult.Failure(ResourceInstantiateError.ServiceUnavailable);

            GameObject instance = _main.Instantiate(key, worldPosition, worldRotation, parent, activate, out ResourceInstantiateError error);
            return ToResult(instance, error);
        }

        /// <summary>
        /// 将 Core 产出签订为契约结果 - 成功时补发释放委托
        /// </summary>
        /// <param name="instance">实例化出的游戏物体，失败时为 null</param>
        /// <param name="error">失败原因</param>
        /// <returns>资源实例化结果</returns>
        private static ResourceInstantiateResult ToResult(GameObject instance, ResourceInstantiateError error)
        {
            if (instance == null)
                return ResourceInstantiateResult.Failure(error);

            return ResourceInstantiateResult.Success(instance, CreateRelease(instance));
        }

        /// <summary>
        /// 创建实例释放委托 - 实例已销毁时跳过，重复调用安全
        /// </summary>
        /// <param name="instance">待释放的实例</param>
        /// <returns>释放委托</returns>
        private static Action CreateRelease(GameObject instance)
        {
            return () =>
            {
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
            };
        }
    }
}
