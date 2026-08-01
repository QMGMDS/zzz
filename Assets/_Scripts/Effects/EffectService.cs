using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEffects
{
    /// <summary>
    /// 默认特效服务 - 通过共享目录注册表查找预制体并管理实例生命周期。
    /// </summary>
    public sealed class EffectService : IEffectService
    {
        private readonly IReadOnlyDictionary<string, GameObject> _prefabs;
        private readonly List<TrackedEffect> _trackedEffects = new List<TrackedEffect>();

        /// <summary>
        /// 使用特效目录创建服务。
        /// </summary>
        /// <param name="catalog">特效目录资产</param>
        public EffectService(EffectCatalogSO catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            _prefabs = EffectCatalogRegistry.Get(catalog);
        }

        /// <inheritdoc />
        public IEffectInstance Play(in EffectPlayRequest request)
        {
            RemoveDestroyedEntries();

            if (string.IsNullOrWhiteSpace(request.EffectId))
            {
                Debug.LogError("特效播放请求的 EffectId 为空。");
                return null;
            }

            if (!_prefabs.TryGetValue(request.EffectId, out GameObject prefab))
            {
                Debug.LogError($"特效目录中不存在 ID \"{request.EffectId}\"。");
                return null;
            }

            if (request.DestroyPolicy != EffectDestroyPolicy.AutoDestroy &&
                request.DestroyPolicy != EffectDestroyPolicy.Manual)
            {
                Debug.LogError($"特效 ID \"{request.EffectId}\" 使用了未知销毁策略：{request.DestroyPolicy}。");
                return null;
            }

            if (request.DestroyPolicy == EffectDestroyPolicy.AutoDestroy && request.AutoDestroyDelay < 0f)
            {
                Debug.LogError($"特效 ID \"{request.EffectId}\" 的自动销毁延迟不能小于 0。");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(
                prefab,
                request.Position,
                request.Rotation,
                request.Parent);
            instance.transform.localScale = request.Scale;

            var effectInstance = new EffectInstance(instance);
            _trackedEffects.Add(new TrackedEffect(effectInstance));
            if (request.DestroyPolicy == EffectDestroyPolicy.AutoDestroy)
                UnityEngine.Object.Destroy(instance, request.AutoDestroyDelay);

            return effectInstance;
        }

        /// <inheritdoc />
        public void CleanupAll()
        {
            for (int i = 0; i < _trackedEffects.Count; i++)
                _trackedEffects[i].Instance.Destroy();
            _trackedEffects.Clear();
        }

        private void RemoveDestroyedEntries()
        {
            for (int i = _trackedEffects.Count - 1; i >= 0; i--)
            {
                if (!_trackedEffects[i].Instance.IsAlive)
                    _trackedEffects.RemoveAt(i);
            }
        }

        private readonly struct TrackedEffect
        {
            public EffectInstance Instance { get; }

            public TrackedEffect(EffectInstance instance)
            {
                Instance = instance;
            }
        }

        private sealed class EffectInstance : IEffectInstance
        {
            private GameObject _gameObject;

            public bool IsAlive => _gameObject != null;
            public GameObject GameObject => _gameObject;

            public EffectInstance(GameObject gameObject)
            {
                _gameObject = gameObject;
            }

            public void Destroy()
            {
                if (_gameObject == null) return;

                UnityEngine.Object.Destroy(_gameObject);
                _gameObject = null;
            }
        }
    }
}


