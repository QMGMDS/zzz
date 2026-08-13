using UnityEngine;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源句柄 - 管理实例释放生命周期
    /// </summary>
    public interface IResourceHandle
    {
        /// <summary>句柄对应的资源键</summary>
        ResourceKey Key { get; }

        /// <summary>句柄管理的实例</summary>
        GameObject Instance { get; }

        /// <summary>是否已经释放</summary>
        bool IsReleased { get; }

        /// <summary>
        /// 释放实例与相关资源引用
        /// </summary>
        void Release();
    }
}
