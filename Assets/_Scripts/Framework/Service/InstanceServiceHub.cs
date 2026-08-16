using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPFramework.Service
{
    /// <summary>
    /// 实例服务标记接口 - 统一约束实例级服务，供服务中心按契约类型与实例 id 存储
    /// </summary>
    public interface IInstanceService { }

    /// <summary>
    /// 实例服务中心 - 以契约接口类型与实例 id 为键，注册与获取实例级服务
    /// </summary>
    public static class InstanceServiceHub
    {
        private static readonly Dictionary<(Type, string), IInstanceService> ServiceDict = new();

        /// <summary>
        /// 注册实例服务
        /// </summary>
        /// <param name="id">实例 id 契约内须唯一</param>
        /// <param name="service">实例服务 不可为空</param>
        /// <returns>注册成功返回 true 重复 id 返回 false</returns>
        public static bool Register<T>(string id, T service) where T : class, IInstanceService
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var key = (typeof(T), id);

            if (ServiceDict.TryGetValue(key, out IInstanceService existing))
            {
                if (ReferenceEquals(existing, service))
                    return true;

                if (IsDestroyed(existing))
                {
                    ServiceDict[key] = service;
                    return true;
                }

                Debug.LogError(
                    $"[InstanceServiceHub] {typeof(T).Name} 实例 id \"{id}\" 重复注册 - 保留现有 {existing}，忽略 {service}");
                return false;
            }

            ServiceDict.Add(key, service);
            return true;
        }

        /// <summary>
        /// 注销实例服务，仅当 id 当前注册的正是该实例时生效
        /// </summary>
        /// <param name="id">实例 id</param>
        /// <param name="service">实例服务 不可为空</param>
        /// <returns>是否注销成功</returns>
        public static bool Unregister<T>(string id, T service) where T : class, IInstanceService
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var key = (typeof(T), id);

            if (!ServiceDict.TryGetValue(key, out IInstanceService existing))
                return false;

            if (IsDestroyed(existing))
            {
                ServiceDict.Remove(key);
                return false;
            }

            if (!ReferenceEquals(existing, service))
                return false;

            ServiceDict.Remove(key);
            return true;
        }

        /// <summary>
        /// 尝试获取实例服务
        /// </summary>
        /// <param name="id">实例 id</param>
        /// <param name="service">获取到的实例服务 未注册时为 null</param>
        /// <returns>是否获取成功</returns>
        public static bool TryGet<T>(string id, out T service) where T : class, IInstanceService
        {
            service = null;

            if (id == null)
                return false;

            var key = (typeof(T), id);

            if (!ServiceDict.TryGetValue(key, out IInstanceService raw))
                return false;

            if (IsDestroyed(raw))
            {
                ServiceDict.Remove(key);
                return false;
            }

            service = (T)raw;
            return true;
        }

        /// <summary>
        /// 清空全部已注册实例服务
        /// </summary>
        public static void Clear()
        {
            ServiceDict.Clear();
        }

        private static bool IsDestroyed(IInstanceService service)
        {
            return service is UnityEngine.Object unityObject && unityObject == null;
        }
    }
}
