using SPInput_Contract;
using UnityEngine;

namespace SPInput_Wiring
{
    /// <summary>
    /// 帧输入提供者槽位 SO - 运行时信箱。
    /// 下游在 Inspector 引用同一份 SO 资产，运行时从 <see cref="Provider"/> 取用。
    /// 删除接线胶水后 Provider 为 null，下游空转不报错。
    /// </summary>
    [CreateAssetMenu(menuName = "SPInput/Frame Input Provider", fileName = "FrameInputProvider")]
    public class FrameInputProviderSO : ScriptableObject
    {
        // 运行时注入，不序列化。下游只读。
        private IFrameInputProvider _provider;

        /// <summary>当前注入的帧输入提供者；未注入时为 null。</summary>
        public IFrameInputProvider Provider => _provider;

        /// <summary>接线胶水专用：注入提供者。</summary>
        internal void Bind(IFrameInputProvider provider) => _provider = provider;

        /// <summary>接线胶水专用：在提供者销毁时清空槽位，避免悬空引用。</summary>
        internal void Clear() => _provider = null;
    }
}

