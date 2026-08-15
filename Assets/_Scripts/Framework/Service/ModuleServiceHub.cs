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
        /// <typeparam name="T">契约类型</typeparam>
        /// <param name="service">服务实例 不可为空</param>
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
        /// 注销模块服务，仅当当前注册的正是该实例时生效
        /// </summary>
        /// <typeparam name="T">契约类型 须与注册时一致</typeparam>
        /// <param name="service">服务实例 不可为空</param>
        public static void Unregister<T>(T service) where T : class, IModuleService
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            Type serviceType = typeof(T);

            if (!ServiceDict.TryGetValue(serviceType, out IModuleService current))
            {
                Debug.LogWarning(
                    $"[ModuleServiceHub] {serviceType.Name} 注销被忽略 - 该契约未注册服务，请检查注册与注销是否成对且契约类型一致");
                return;
            }

            if (!ReferenceEquals(current, service))
            {
                Debug.LogWarning(
                    $"[ModuleServiceHub] {serviceType.Name} 注销被忽略 - 当前注册 {current} 不是传入实例 {service}");
                return;
            }

            ServiceDict.Remove(serviceType);
        }

        /// <summary>
        /// 尝试获取模块服务
        /// </summary>
        /// <typeparam name="T">契约类型</typeparam>
        /// <param name="service">获取到的服务 未注册或已销毁时为 null</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGet<T>(out T service) where T : class, IModuleService
        {
            service = null;

            Type serviceType = typeof(T);

            if (!ServiceDict.TryGetValue(serviceType, out IModuleService raw))
                return false;

            if (IsDestroyed(raw))
            {
                ServiceDict.Remove(serviceType);
                return false;
            }

            service = (T)raw;
            return true;
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
