using UnityEngine;

using SPInput.Contract;

namespace SPInput.Wiring
{
    /// <summary>
    /// 信箱 - 存储输入模块向外提供玩家帧输入数据
    /// </summary>
    [CreateAssetMenu(menuName = "SPInput/FrameInput Provider", fileName = "FrameInputProvider")]
    public sealed class FrameInputProviderSO : ScriptableObject
    {
        private IProvideFrameInput _provider;

        /// <summary>当前注入的帧输入采集器，未注入时为 null，供外部模块拿取</summary>
        public IProvideFrameInput Provider => _provider;

        /// <summary>
        /// 接线胶水专用 - 注入帧输入采集器
        /// 重复注入不同实例会告警，防止多实例串改
        /// </summary>
        /// <param name="provider">帧输入采集器</param>
        internal void Bind(IProvideFrameInput provider)
        {
            if (provider == null) return;
            if (_provider != null && !ReferenceEquals(_provider, provider))
                UnityEngine.Debug.LogWarning(
                    $"FrameInputProviderSO: 已注入提供者 [{_provider}]，现又被覆盖为 [{provider}]。" +
                    "本槽位仅支持单采集器注入，请避免多实例接线同一份 SO 资产。");

            _provider = provider;
        }

        /// <summary>
        /// 接线胶水专用 - 在采集器销毁时清空槽位，避免悬空引用
        /// </summary>
        internal void Clear() => _provider = null;
    }
}