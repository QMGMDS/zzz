using UnityEngine;
using UnityEngine.Events;

namespace Core.Event
{
    /// <summary>
    /// Inspector 可配置的事件监听器，订阅 VoidEventChannelSO，触发时调用配置的 UnityEvent 回调链
    /// </summary>
    public class MonoEventListener : MonoBehaviour
    {
        [Tooltip("要监听的 VoidEventChannelSO 资产")]
        [SerializeField] private VoidEventChannelSO _channel;

        [Tooltip("事件触发时调用的 UnityEvent 回调")]
        [SerializeField] private UnityEvent _onEventRaised;

        private void OnEnable()
        {
            if (_channel != null)
            {
                _channel.Subscribe(HandleEvent);
            }
        }

        private void OnDisable()
        {
            if (_channel != null)
            {
                _channel.Unsubscribe(HandleEvent);
            }
        }

        private void HandleEvent()
        {
            _onEventRaised?.Invoke();
        }
    }
}
