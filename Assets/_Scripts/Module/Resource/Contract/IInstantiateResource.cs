using UnityEngine;

using SPFramework.Service;

namespace SPResource.Contract
{
    /// <summary>
    /// 资源实例化能力 - 根据资源键同步创建预制体实例
    /// </summary>
    public interface IInstantiateResource : IModuleService
    {
        /// <summary>
        /// 同步实例化资源 - 实例保持预制体自身姿态
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="parent">实例父节点，留空时实例位于场景根</param>
        /// <param name="activate">实例创建后是否设为激活</param>
        /// <returns>资源实例化结果</returns>
        ResourceInstantiateResult Instantiate(ResourceKey key, Transform parent = null, bool activate = true);

        /// <summary>
        /// 同步实例化资源到指定世界位姿
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="worldPosition">实例世界坐标</param>
        /// <param name="worldRotation">实例世界旋转</param>
        /// <param name="parent">实例父节点，留空时实例位于场景根</param>
        /// <param name="activate">实例创建后是否设为激活</param>
        /// <returns>资源实例化结果</returns>
        ResourceInstantiateResult Instantiate(ResourceKey key, Vector3 worldPosition, Quaternion worldRotation, Transform parent = null, bool activate = true);
    }
}
