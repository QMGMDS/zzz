using System.Collections.Generic;

namespace Core.Event
{
    /// <summary>
    /// 事件通道静态注册表，收集所有活跃的 EventChannelSO 实例，供调试面板遍历
    /// </summary>
    public static class EventChannelRegistry
    {
        private static readonly HashSet<EventChannelSO> Channels = new HashSet<EventChannelSO>();

        /// <summary>注册通道（由 EventChannelSO.OnEnable 自动调用）</summary>
        public static void Register(EventChannelSO channel)
        {
            if (channel != null)
            {
                Channels.Add(channel);
            }
        }

        /// <summary>注销通道（由 EventChannelSO.OnDisable 自动调用）</summary>
        public static void Unregister(EventChannelSO channel)
        {
            if (channel != null)
            {
                Channels.Remove(channel);
            }
        }

        /// <summary>获取当前所有活跃通道的只读枚举</summary>
        public static IEnumerable<EventChannelSO> GetAll()
        {
            return Channels;
        }
    }
}
