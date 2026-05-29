using UnityEngine;

namespace Core.Event
{
    /// <summary>
    /// 事件通道抽象基类，自动向 EventChannelRegistry 注册/注销，维护触发统计
    /// </summary>
    public abstract class EventChannelSO : ScriptableObject, IEventChannel
    {
        /// <inheritdoc cref="IEventChannel.ChannelName"/>
        public string ChannelName => name;

        /// <inheritdoc cref="IEventChannel.SubscriberCount"/>
        public int SubscriberCount { get; protected set; }

        /// <inheritdoc cref="IEventChannel.RaiseCount"/>
        public int RaiseCount { get; protected set; }

        /// <inheritdoc cref="IEventChannel.ResetStats"/>
        public void ResetStats()
        {
            RaiseCount = 0;
        }

        #region Life Cycle

        private void OnEnable()
        {
            EventChannelRegistry.Register(this);
        }

        private void OnDisable()
        {
            EventChannelRegistry.Unregister(this);
        }

        #endregion
    }
}
