using UnityEngine;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源实例化请求 - 描述预制体实例化参数
    /// </summary>
    public readonly struct ResourceLoadRequest
    {
        /// <summary>要加载的资源键</summary>
        public ResourceKey Key { get; }

        /// <summary>实例父节点</summary>
        public Transform Parent { get; }

        /// <summary>实例世界坐标</summary>
        public Vector3 WorldPosition { get; }

        /// <summary>实例世界旋转</summary>
        public Quaternion WorldRotation { get; }

        /// <summary>实例创建后是否设为激活</summary>
        public bool ShouldActivateAfterCreate { get; }

        /// <summary>
        /// 创建资源实例化请求
        /// </summary>
        /// <param name="key">要加载的资源键</param>
        /// <param name="parent">实例父节点</param>
        /// <param name="worldPosition">实例世界坐标</param>
        /// <param name="worldRotation">实例世界旋转</param>
        /// <param name="shouldActivateAfterCreate">实例创建后是否设为激活</param>
        public ResourceLoadRequest(
            ResourceKey key,
            Transform parent,
            Vector3 worldPosition,
            Quaternion worldRotation,
            bool shouldActivateAfterCreate = true)
        {
            Key = key;
            Parent = parent;
            WorldPosition = worldPosition;
            WorldRotation = worldRotation;
            ShouldActivateAfterCreate = shouldActivateAfterCreate;
        }
    }
}
