using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{
    public class PoolManager
    {
        #region Singleton

        private static PoolManager _instance;
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PoolManager();
                }
                return _instance;
            }
        }

        #endregion

        /// <summary>所有池子的查询字典，通过池子名字查询对应池子</summary>
        private Dictionary<string, IPool> pools = new Dictionary<string, IPool>();

        /// <summary>
        /// 注册一个新对象池
        /// </summary>
        /// <typeparam name="T">池化对象的 MonoBehaviour 类型</typeparam>
        /// <param name="poolName">池名，全局唯一</param>
        /// <param name="prefab">池的预制体模板</param>
        /// <param name="parent">生成对象的默认父节点</param>
        /// <param name="prewarmCount">预热数量</param>
        public void RegisterPool<T>(string poolName, T prefab, Transform parent = null, int prewarmCount = 0)
            where T : MonoBehaviour, IPoolable
        {
            if (pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"PoolManager: Pool '{poolName}' already exists.");
                return;
            }
            pools[poolName] = new Pool<T>(prefab, parent, prewarmCount);
        }

        /// <summary>
        /// 从指定池中拿取一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="poolName">目标池名</param>
        /// <returns>池化对象，池不存在则返回 null</returns>
        public T Get<T>(string poolName) where T : MonoBehaviour, IPoolable
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                var obj = pool.GetGeneric() as T;
                if (obj != null)
                {
                    obj.PoolName = poolName;
                    return obj;
                }
                Debug.LogError($"PoolManager: Pool '{poolName}' type mismatch.");
                return null;
            }
            Debug.LogError($"PoolManager: Pool '{poolName}' not found.");
            return null;
        }

        /// <summary>
        /// 回收对象到其所属池中（通过 IPoolable.PoolName 反向查找）
        /// </summary>
        /// <param name="obj">被回收的池化对象</param>
        public void Recycle(IPoolable obj)
        {
            if (string.IsNullOrEmpty(obj.PoolName))
            {
                Debug.LogError("PoolManager: Object has no PoolName, cannot recycle.");
                return;
            }
            if (pools.TryGetValue(obj.PoolName, out var pool))
            {
                pool.RecycleGeneric(obj);
            }
            else
            {
                Debug.LogError($"PoolManager: Pool '{obj.PoolName}' not found, cannot recycle.");
            }
        }

        /// <summary>
        /// 移除指定池子（销毁其中所有对象）
        /// </summary>
        public void RemovePool(string poolName)
        {
            if (pools.TryGetValue(poolName, out var pool))
            {
                pool.Clear();
                pools.Remove(poolName);
            }
        }
    }
}