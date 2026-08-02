using SPInput_Contract;
using UnityEngine;

namespace SPInput_Wiring
{
    /// <summary>
    /// 帧输入提供者槽位 SO - 运行时信箱。
    /// 下游在 Inspector 引用同一份 SO 资产，运行时从 <see cref="Provider"/> 取用。
    /// 删除接线胶水后 Provider 为 null，下游空转不报错。
    /// 约束：仅支持单采集器注入；重复 Bind 会在控制台告警。
    /// </summary>
    [CreateAssetMenu(menuName = "SPInput/Frame Input Provider", fileName = "FrameInputProvider")]
    public class FrameInputProviderSO : ScriptableObject
    {
        // 运行时注入，不序列化。下游只读。
        private IFrameInputProvider _provider;

        /// <summary>当前注入的帧输入提供者；未注入时为 null。</summary>
        public IFrameInputProvider Provider => _provider;

        /// <summary>
        /// 接线胶水专用 - 注入提供者。
        /// 重复注入不同实例会告警，防止多实例串改。
        /// </summary>
        /// <param name="provider">帧输入提供者，通常为 FrameInputCollector</param>
        internal void Bind(IFrameInputProvider provider)
        {
            if (provider == null) return;
            if (_provider != null && !ReferenceEquals(_provider, provider))
                Debug.LogWarning(
                    $"FrameInputProviderSO: 已注入提供者 [{_provider}]，现又被覆盖为 [{provider}]。" +
                    "本槽位仅支持单采集器注入，请避免多实例接线同一份 SO 资产。");

            _provider = provider;
        }

        /// <summary>
        /// 接线胶水专用 - 在提供者销毁时清空槽位，避免悬空引用。
        /// </summary>
        internal void Clear() => _provider = null;
    }
}