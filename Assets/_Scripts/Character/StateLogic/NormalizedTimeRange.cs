using System;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 归一化时间闭区间 - 保证构造值位于 0 到 1，并提供区间包含判断。
    /// </summary>
    [Serializable]
    public struct NormalizedTimeRange
    {
        [Tooltip("闭区间起点")]
        [SerializeField] private float _start;

        [Tooltip("闭区间终点")]
        [SerializeField] private float _end;

        /// <summary>闭区间起点</summary>
        public float Start => _start;

        /// <summary>闭区间终点</summary>
        public float End => _end;

        /// <summary>
        /// 创建归一化时间闭区间。
        /// </summary>
        /// <param name="start">闭区间起点</param>
        /// <param name="end">闭区间终点</param>
        public NormalizedTimeRange(float start, float end)
        {
            _start = float.IsNaN(start) ? 0f : Mathf.Clamp01(start);
            _end = float.IsNaN(end) ? _start : Mathf.Clamp(end, _start, 1f);
        }

        /// <summary>
        /// 判断归一化时间是否位于闭区间内。
        /// </summary>
        /// <param name="normalizedTime">待判断的归一化时间</param>
        /// <returns>位于闭区间内时返回 true</returns>
        public bool Contains(float normalizedTime)
        {
            return normalizedTime >= _start && normalizedTime <= _end;
        }
    }
}
