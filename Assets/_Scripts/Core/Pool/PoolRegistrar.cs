using Core.Pool;
using GamePlay.Effects;
using UnityEngine;

namespace GamePlay
{
    /// <summary>
    /// 挂载在场景 "Pool" 物体上
    /// Awake 时自动创建层级容器并注册到 PoolManager
    /// </summary>
    public class PoolRegistrar : MonoBehaviour
    {
        [Header("Effects")]
        [SerializeField] private SlashEffect _slashEffectPrefab;

        private void Awake()
        {
            CreatePool("FX_Slash", _slashEffectPrefab, 7);
        }

        private void CreatePool<T>(string poolName, T prefab, int prewarmCount) where T : MonoBehaviour, IPoolable
        {
            var container = new GameObject("Pool_" + poolName);
            container.transform.SetParent(transform);
            PoolManager.Instance.RegisterPool(poolName, prefab, container.transform, prewarmCount);
        }
    }
}