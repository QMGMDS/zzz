using System;
using UnityEngine;

namespace Core.Event
{
    /// <summary>
    /// 带 Transform 参数的事件通道
    /// </summary>
    [CreateAssetMenu(menuName = "Event/Channels/Transform Channel", fileName = "New Transform Event Channel")]
    public class TransformEventChannelSO : EventChannelSO
    {
        private event Action<Transform> OnRaised;

        /// <summary>触发事件，通知所有订阅者</summary>
        /// <param name="value">事件参数</param>
        public void Raise(Transform value)
        {
            RaiseCount++;
            OnRaised?.Invoke(value);
        }

        /// <summary>订阅事件</summary>
        /// <param name="callback">事件回调，参数为 Raise 时传入的 Transform</param>
        public void Subscribe(Action<Transform> callback)
        {
            OnRaised += callback;
            SubscriberCount++;
        }

        /// <summary>取消订阅</summary>
        /// <param name="callback">之前订阅的回调</param>
        public void Unsubscribe(Action<Transform> callback)
        {
            OnRaised -= callback;
            SubscriberCount--;
        }
    }
}
