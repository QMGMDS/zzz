using UnityEngine;

using SPResource.Contract;

namespace SPResource.Core
{
    /// <summary>
    /// 预制体资源句柄 - 释放实例化出的游戏物体
    /// </summary>
    internal sealed class PrefabResourceHandle : IResourceHandle
    {
        private readonly ResourceKey _key;
        private GameObject _instance;
        private bool _isReleased;

        /// <summary>
        /// 创建预制体资源句柄
        /// </summary>
        /// <param name="key">句柄对应的资源键</param>
        /// <param name="instance">句柄管理的实例</param>
        public PrefabResourceHandle(ResourceKey key, GameObject instance)
        {
            _key = key;
            _instance = instance;
        }

        /// <inheritdoc />
        public ResourceKey Key => _key;

        /// <inheritdoc />
        public GameObject Instance => _instance;

        /// <inheritdoc />
        public bool IsReleased => _isReleased;

        /// <inheritdoc />
        public void Release()
        {
            if (_isReleased) return;

            if (_instance != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_instance);
                else
                    Object.DestroyImmediate(_instance);
            }

            _instance = null;
            _isReleased = true;
        }
    }
}
