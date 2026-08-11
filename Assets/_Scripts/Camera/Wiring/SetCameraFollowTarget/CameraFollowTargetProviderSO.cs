using UnityEngine;

using SPCamera.Contract;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 信箱 - 存储摄像机跟随目标设置器 Provider，供外部模块请求切换跟随目标
    /// </summary>
    [CreateAssetMenu(menuName = "SPCamera/Camera Follow Target Provider", fileName = "CameraFollowTargetProvider")]
    public sealed class CameraFollowTargetProviderSO : ScriptableObject
    {
        private ISetCameraFollowTarget _provider;

        /// <summary>当前注入的摄像机跟随目标设置器，未注入时为 null，供外部模块拿取</summary>
        public ISetCameraFollowTarget Provider => _provider;

        /// <summary>
        /// 接线胶水专用 - 注入摄像机跟随目标设置器
        /// 重复注入不同实例会告警，防止多实例串改
        /// </summary>
        /// <param name="provider">摄像机跟随目标设置器</param>
        internal void Bind(ISetCameraFollowTarget provider)
        {
            if (provider == null) return;
            if (_provider != null && !ReferenceEquals(_provider, provider))
                Debug.LogWarning(
                    $"CameraFollowTargetProviderSO: 已注入提供者 [{_provider}]，现又被覆盖为 [{provider}]。" +
                    "本槽位仅支持单跟随目标设置器注入，请避免多实例接线同一份 SO 资产。");

            _provider = provider;
        }

        /// <summary>
        /// 接线胶水专用 - 在跟随目标设置器销毁时清空槽位，避免悬空引用
        /// </summary>
        internal void Clear() => _provider = null;
    }
}

