using System;

using UnityEngine;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源实例化错误 - 描述一次实例化失败的原因
    /// </summary>
    public enum ResourceInstantiateError
    {
        /// <summary>无错误 - 实例化成功</summary>
        None,

        /// <summary>资源键为空</summary>
        InvalidKey,

        /// <summary>资源目录中不存在该资源键</summary>
        KeyNotFound,

        /// <summary>实例化过程发生异常</summary>
        InstantiateFailed,

        /// <summary>资源主入口未接线或已销毁</summary>
        ServiceUnavailable,
    }

    /// <summary>
    /// 资源实例化结果 - 返回实例、失败原因与释放委托
    /// </summary>
    public readonly struct ResourceInstantiateResult
    {
        /// <summary>实例化出的游戏物体，失败时为 null</summary>
        public GameObject Instance { get; }

        /// <summary>失败原因，成功时为 None</summary>
        public ResourceInstantiateError Error { get; }

        /// <summary>释放实例的委托，失败时为 null，重复调用安全</summary>
        public Action Release { get; }

        /// <summary>实例化是否成功</summary>
        public bool IsSuccess => Error == ResourceInstantiateError.None;

        private ResourceInstantiateResult(
            GameObject instance,
            ResourceInstantiateError error,
            Action release)
        {
            Instance = instance;
            Error = error;
            Release = release;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="instance">实例化出的游戏物体</param>
        /// <param name="release">释放实例的委托</param>
        /// <returns>成功结果</returns>
        internal static ResourceInstantiateResult Success(GameObject instance, Action release)
        {
            return new ResourceInstantiateResult(instance, ResourceInstantiateError.None, release);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="error">失败原因</param>
        /// <returns>失败结果</returns>
        internal static ResourceInstantiateResult Failure(ResourceInstantiateError error)
        {
            return new ResourceInstantiateResult(null, error, null);
        }
    }
}
