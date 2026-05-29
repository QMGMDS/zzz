using System;
using UnityEngine;

namespace Core.Event
{
    /// <summary>
    /// 无参事件通道，最常用的通道类型，通过 [CreateAssetMenu] 在 Project 窗口右键创建
    /// </summary>
    [CreateAssetMenu(menuName = "Event/Channels/Void Channel", fileName = "New Void Event Channel")]
    public class VoidEventChannelSO : EventChannelSO
    {
        private event Action OnRaised;

        /// <summary>触发事件，通知所有订阅者</summary>
        public void Raise()
        {
            RaiseCount++;
            OnRaised?.Invoke();
        }

        /// <summary>订阅事件</summary>
        /// <param name="callback">事件回调</param>
        public void Subscribe(Action callback)
        {
            OnRaised += callback;
            SubscriberCount++;
        }

        /// <summary>取消订阅</summary>
        /// <param name="callback">之前订阅的回调</param>
        public void Unsubscribe(Action callback)
        {
            OnRaised -= callback;
            SubscriberCount--;
        }
    }
}
