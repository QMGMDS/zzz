using UnityEngine;

namespace Core.Event
{
    /// <summary>
    /// Inspector 可配置的事件触发器，持有 VoidEventChannelSO 引用，提供 Raise 方法供 UnityEvent 或代码调用
    /// </summary>
    public class MonoEventTrigger : MonoBehaviour
    {
        [Tooltip("要触发的 VoidEventChannelSO 资产")]
        [SerializeField] private VoidEventChannelSO _channel;

        /// <summary>触发关联的事件通道</summary>
        public void Raise()
        {
            _channel?.Raise();
        }
    }
}
