namespace Core.Event
{
    /// <summary>事件通道公共接口，暴露调试统计信息供统一查询</summary>
    public interface IEventChannel
    {
        /// <summary>通道名称（ScriptableObject 资产名）</summary>
        string ChannelName { get; }

        /// <summary>当前订阅者数量</summary>
        int SubscriberCount { get; }

        /// <summary>累计触发次数</summary>
        int RaiseCount { get; }

        /// <summary>重置累计统计计数</summary>
        void ResetStats();
    }
}
