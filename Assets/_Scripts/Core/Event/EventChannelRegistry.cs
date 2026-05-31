using System.Collections.Generic;

namespace Core.Event
{
    /// <summary>事件通道静态注册表，收集所有活跃通道实例用于调试工具遍历</summary>
    public static class EventChannelRegistry
    {
        private static readonly HashSet<EventChannelSO> Channels = new HashSet<EventChannelSO>();

        /// <summary>注册通道</summary>
        /// <param name="channel">要注册的通道实例</param>
        public static void Register(EventChannelSO channel)
        {
            if (channel != null)
            {
                Channels.Add(channel);
            }
        }

        /// <summary>注销通道</summary>
        /// <param name="channel">要注销的通道实例</param>
        public static void Unregister(EventChannelSO channel)
        {
            if (channel != null)
            {
                Channels.Remove(channel);
            }
        }

        /// <summary>获取当前所有活跃通道的只读枚举</summary>
        /// <returns>活跃通道的可枚举集合</returns>
        public static IEnumerable<EventChannelSO> GetAll()
        {
            return Channels;
        }
    }
}
