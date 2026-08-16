using System;

using UnityEngine;

using SPResource.Contract;

namespace SPResource.Core
{
    /// <summary>
    /// 资源主入口 - 独立自主的资源实例化实体，通过资源目录同步实例化预制体
    /// </summary>
    internal sealed class SPResourceMain : MonoBehaviour
    {
        [Header("资源目录")]
        [SerializeField, Tooltip("资源键到预制体的运行时查询目录")]
        private ResourceCatalogSO _catalog;

        private void Awake()
        {
            if (_catalog == null)
                throw new InvalidOperationException(
                    $"SPResourceMain ({name}): ResourceCatalogSO 未设置，请检查 Inspector 配置");

            _catalog.Validate();
        }

        /// <summary>同步实例化资源 - 实例保持预制体自身姿态</summary>
        /// <param name="key">资源键</param>
        /// <param name="parent">实例父节点，留空时实例位于场景根</param>
        /// <param name="activate">实例创建后是否设为激活</param>
        /// <param name="error">失败原因</param>
        /// <returns>实例化出的游戏物体，失败时为 null</returns>
        public GameObject Instantiate(ResourceKey key, Transform parent, bool activate, out ResourceInstantiateError error)
        {
            if (!TryResolvePrefab(key, out GameObject prefab, out error))
                return null;

            try
            {
                GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.SetActive(activate);
                error = ResourceInstantiateError.None;
                return instance;
            }
            catch (Exception)
            {
                error = ResourceInstantiateError.InstantiateFailed;
                return null;
            }
        }

        /// <summary>同步实例化资源到指定世界位姿</summary>
        /// <param name="key">资源键</param>
        /// <param name="parent">实例父节点，留空时实例位于场景根</param>
        /// <param name="activate">实例创建后是否设为激活</param>
        /// <param name="error">失败原因</param>
        /// <returns>实例化出的游戏物体，失败时为 null</returns>
        public GameObject Instantiate(ResourceKey key, Vector3 worldPosition, Quaternion worldRotation, Transform parent, bool activate, out ResourceInstantiateError error)
        {
            if (!TryResolvePrefab(key, out GameObject prefab, out error))
                return null;

            try
            {
                GameObject instance = UnityEngine.Object.Instantiate(prefab, worldPosition, worldRotation, parent);
                instance.SetActive(activate);
                error = ResourceInstantiateError.None;
                return instance;
            }
            catch (Exception)
            {
                error = ResourceInstantiateError.InstantiateFailed;
                return null;
            }
        }

        /// <summary>
        /// 解析资源键对应的预制体 - 目录已经 Awake 校验，查不到即键不存在
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="prefab">查找到的预制体</param>
        /// <param name="error">失败原因</param>
        /// <returns>是否解析成功</returns>
        private bool TryResolvePrefab(ResourceKey key, out GameObject prefab, out ResourceInstantiateError error)
        {
            if (!key.IsValid)
            {
                prefab = null;
                error = ResourceInstantiateError.InvalidKey;
                return false;
            }

            if (!_catalog.TryGetPrefab(key, out prefab))
            {
                error = ResourceInstantiateError.KeyNotFound;
                return false;
            }

            error = ResourceInstantiateError.None;
            return true;
        }
    }
}
