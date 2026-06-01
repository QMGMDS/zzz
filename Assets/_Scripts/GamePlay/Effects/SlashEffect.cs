using System.Collections;
using Core.Pool;
using UnityEngine;

namespace GamePlay.Effects
{
    /// <summary>
    /// 挥砍特效，挂载在 FX_slash.prefab 上。
    /// 实现 IPoolable 接口，通过对象池管理生命周期，粒子播放完毕后自动回收。
    /// </summary>
    public class SlashEffect : MonoBehaviour, IPoolable
    {
        public string PoolName { get; set; }

        private ParticleSystem _particleSystem;
        private float _duration;

        private void Awake()
        {
            _particleSystem = GetComponentInChildren<ParticleSystem>();
            if (_particleSystem != null)
            {
                _duration = _particleSystem.main.duration;
            }
        }

        /// <summary>
        /// 从池中取出时调用，播放粒子并启动自动回收协程。
        /// </summary>
        public void OnSpawn()
        {
            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
            StartCoroutine(AutoRecycle());
        }

        /// <summary>
        /// 回收时调用，停止粒子并终止自动回收协程。
        /// </summary>
        public void OnDespawn()
        {
            StopAllCoroutines();
            if (_particleSystem != null)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private IEnumerator AutoRecycle()
        {
            yield return new WaitForSeconds(_duration);
            PoolManager.Instance.Recycle(this);
        }
    }
}
