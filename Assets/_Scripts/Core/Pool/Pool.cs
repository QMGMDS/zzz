using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{
    public class Pool<T> : IPool where T : MonoBehaviour, IPoolable
    {
        /// <summary> 池化对象的预制体模板 </summary>
        private readonly T prefab;
        /// <summary> 记录该池子内对象的父物体 </summary>
        private readonly Transform parent;
        private readonly Queue<T> pool = new Queue<T>();

        /// <summary>
        /// 将对应对象池化，可选是否预热池
        /// </summary>
        /// <param name="prefab">对应对象，要池化的对象</param>
        /// <param name="parent">池子内对象的父物体，便于场景中查看</param>
        /// <param name="prewarmCount">预热池子的个数</param>
        public Pool(T prefab, Transform parent = null, int prewarmCount = 0)
        {
            this.prefab = prefab;
            this.parent = parent;
            Prewarm(prewarmCount);
        }

        /// <summary> 从池中取出一个对象，池空则自动 Instantiate 一个池内对象 </summary>
        private T Get()
        {
            T obj;
            if (pool.Count > 0)
            {
                obj = pool.Dequeue();
            }
            else
            {
                obj = Object.Instantiate(prefab, parent);
            }
            obj.gameObject.SetActive(true);
            obj.OnSpawn();
            return obj;
        }

        /// <summary> 回收对象到池中（失活并入队） </summary>
        private void Recycle(T obj)
        {
            obj.OnDespawn();
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }

        /// <summary> 预热指定数量的实例（提前 Instantiate 入池，默认失活） </summary>
        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Object.Instantiate(prefab, parent);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
        }


        #region Interface

        /// <summary> 该池内对象的个数 </summary>
        public int Count => pool.Count;

        /// <summary>
        /// 销毁池中所有对象
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                Object.Destroy(pool.Dequeue().gameObject);
            }
        }

        IPoolable IPool.GetGeneric()
        {
            return Get();
        }

        void IPool.RecycleGeneric(IPoolable obj)
        {
            if (obj is T typedObj)
            {
                Recycle(typedObj);
            }
        }

        #endregion
    }
}