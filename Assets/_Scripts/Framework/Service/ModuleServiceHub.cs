using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPFramework.Service
{
    /// <summary>
    /// 模块服务标记接口 - 统一约束模块级服务，供服务中心按契约类型存储
    /// </summary>
    public interface IModuleService { }

    /// <summary>
    /// 模块服务中心 - 以契约接口类型为键，注册与获取模块级单例服务
    /// </summary>
    public static class ModuleServiceHub
    {
        private static readonly Dictionary<Type, IModuleService> ServiceDict = new();

        /// <summary>
        /// 注册模块服务
        /// </summary>
        public static void Register<T>(T service) where T : class, IModuleService
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            Type serviceType = typeof(T);

            if (ServiceDict.TryGetValue(serviceType, out IModuleService existing)
                && !IsDestroyed(existing)
                && !ReferenceEquals(existing, service))
            {
                Debug.LogWarning(
                    $"[ModuleServiceHub] {serviceType.Name} 已注册 {existing}，现被 {service} 覆盖 - 请检查是否多实例接线同一契约");
            }

            ServiceDict[serviceType] = service;
        }

        /// <summary>
        /// 获取模块服务，未注册时返回 null
        /// </summary>
        public static T Get<T>() where T : class, IModuleService
        {
            Type serviceType = typeof(T);

            if (!ServiceDict.TryGetValue(serviceType, out IModuleService service))
                return null;

            if (IsDestroyed(service))
            {
                ServiceDict.Remove(serviceType);
                return null;
            }

            return (T)service;
        }

        /// <summary>
        /// 尝试获取模块服务
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class, IModuleService
        {
            service = Get<T>();
            return service != null;
        }

        /// <summary>
        /// 反注册模块服务
        /// </summary>
        public static bool Unregister<T>() where T : class, IModuleService
        {
            return ServiceDict.Remove(typeof(T));
        }

        /// <summary>
        /// 清空全部已注册服务
        /// </summary>
        public static void Clear()
        {
            ServiceDict.Clear();
        }

        private static bool IsDestroyed(IModuleService service)
        {
            return service is UnityEngine.Object unityObject && unityObject == null;
        }
    }
}
