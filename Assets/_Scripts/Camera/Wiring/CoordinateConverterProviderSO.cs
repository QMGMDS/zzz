using SPCamera.Contract;
using UnityEngine;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 摄像机坐标转换器槽位 SO - 运行时信箱。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCamera/Coordinate Converter Provider", fileName = "CoordinateConverterProvider")]
    public class CoordinateConverterProviderSO : ScriptableObject
    {
        private ICoordinateConverter _provider;

        /// <summary>当前注入的坐标转换器；未注入时为 null。</summary>
        public ICoordinateConverter Provider => _provider;

        /// <summary>
        /// 接线胶水专用 - 注入坐标转换器。
        /// 重复注入不同实例会告警，防止多实例串改。
        /// </summary>
        /// <param name="provider">坐标转换器，通常由 Camera 模块运行时组件提供</param>
        internal void Bind(ICoordinateConverter provider)
        {
            if (provider == null) return;
            if (_provider != null && !ReferenceEquals(_provider, provider))
                Debug.LogWarning(
                    $"CoordinateConverterProviderSO: 已注入提供者 [{_provider}]，现又被覆盖为 [{provider}]。" +
                    "本槽位仅支持单转换器注入，请避免多实例接线同一份 SO 资产。");

            _provider = provider;
        }

        /// <summary>
        /// 接线胶水专用 - 在转换器销毁时清空槽位，避免悬空引用。
        /// </summary>
        internal void Clear() => _provider = null;
    }
}
