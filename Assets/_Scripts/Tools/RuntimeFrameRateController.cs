using UnityEngine;

namespace SPTools
{
    /// <summary>
    /// 运行时帧率控制器 - 设置目标帧率，并可关闭垂直同步以确保帧率限制生效
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10000)]
    public sealed class RuntimeFrameRateController : MonoBehaviour
    {
        private const int UnlimitedFrameRate = -1;

        [Header("帧率设置")]
        [SerializeField, Min(UnlimitedFrameRate), Tooltip("目标帧率；设为 -1 时不限制帧率")]
        private int _targetFrameRate = 60;

        [SerializeField, Tooltip("是否关闭垂直同步。关闭后由目标帧率控制运行时帧数。")]
        private bool _shouldDisableVSync = true;

        private void Awake()
        {
            ApplyFrameRate();
        }

        private void OnValidate()
        {
            _targetFrameRate = NormalizeTargetFrameRate(_targetFrameRate);

            if (Application.isPlaying)
                ApplyFrameRate();
        }

        /// <summary>
        /// 设置并立即应用目标帧率
        /// </summary>
        /// <param name="targetFrameRate">目标帧率；-1 表示不限制，其他值必须大于 0</param>
        public void SetTargetFrameRate(int targetFrameRate)
        {
            _targetFrameRate = NormalizeTargetFrameRate(targetFrameRate);
            ApplyFrameRate();
        }

        private void ApplyFrameRate()
        {
            if (_shouldDisableVSync)
                QualitySettings.vSyncCount = 0;

            Application.targetFrameRate = _targetFrameRate;
        }

        private static int NormalizeTargetFrameRate(int targetFrameRate)
        {
            return targetFrameRate < 1 ? UnlimitedFrameRate : targetFrameRate;
        }
    }
}
