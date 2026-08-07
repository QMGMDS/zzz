using UnityEngine;

namespace SPInput.Core
{
    /// <summary>
    /// 输入后处理配置 SO - 持有按键长按阈值与轴归零缓冲时长等可调参数。
    /// 为静态配置资产，运行时只读，可被多个采集器共享引用；禁止写入运行时状态。
    /// </summary>
    [CreateAssetMenu(menuName = "SPInput/Processing Config", fileName = "InputProcessingConfig")]
    public class InputProcessingConfigSO : ScriptableObject
    {
        [Header("按键形 - 长按判定")]
        [Tooltip("按键被持续按压超过此时长（秒）即判定为长按；所有按键形共用同一阈值。")]
        [SerializeField, Range(0f, 2f)] private float _holdThreshold = 0.3f;

        [Header("轴输入形 - 归零缓冲")]
        [Tooltip("轴输入归零后，在此时长（秒）内沿用上一帧非零方向，用于补偿 A->D 中间几帧空隙。")]
        [SerializeField, Range(0f, 0.5f)] private float _releaseBuffer = 0.1f;

        /// <summary>按键长按判定阈值（秒）</summary>
        public float HoldThreshold => _holdThreshold;

        /// <summary>轴输入归零缓冲时长（秒）</summary>
        public float ReleaseBuffer => _releaseBuffer;
    }
}
