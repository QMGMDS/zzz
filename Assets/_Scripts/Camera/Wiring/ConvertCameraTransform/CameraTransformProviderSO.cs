using UnityEngine;

using SPCamera.Contract;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 信箱 - 存储坐标转换器 Provider，供外部模块获取
    /// </summary>
    [CreateAssetMenu(menuName = "SPCamera/CameraTransform Provider", fileName = "CameraTransformProvider")]
    public sealed class CameraTransformProviderSO : ScriptableObject
    {
        private IConvertCameraTransform _provider;

        /// <summary>当前注入的坐标转换器，未注入时为 null，供外部模块拿取</summary>
        public IConvertCameraTransform Provider => _provider;

        /// <summary>
        /// 接线胶水专用 - 注入坐标转换器
        /// 重复注入不同实例会告警，防止多实例串改
        /// </summary>
        /// <param name="provider">坐标转换器</param>
        internal void Bind(IConvertCameraTransform provider)
        {
            if (provider == null) return;
            if (_provider != null && !ReferenceEquals(_provider, provider))
                Debug.LogWarning(
                    $"CameraTransformProviderSO: 已注入提供者 [{_provider}]，现又被覆盖为 [{provider}]。" +
                    "本槽位仅支持单转换器注入，请避免多实例接线同一份 SO 资产。");

            _provider = provider;
        }

        /// <summary>
        /// 接线胶水专用 - 在转换器销毁时清空槽位，避免悬空引用
        /// </summary>
        internal void Clear() => _provider = null;
    }
}
