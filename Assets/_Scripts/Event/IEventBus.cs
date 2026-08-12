using System;

namespace SPEvent
{
    /// <summary>
    /// 事件总线接口 - 发布事实事件并管理订阅
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 订阅指定事件
        /// </summary>
        /// <typeparam name="TPayload">事件负载类型</typeparam>
        /// <param name="eventKey">事件标识</param>
        /// <param name="handler">事件处理函数</param>
        /// <returns>用于取消订阅的句柄</returns>
        IDisposable Subscribe<TPayload>(EventKey<TPayload> eventKey, Action<TPayload> handler);

        /// <summary>
        /// 发布指定事件
        /// </summary>
        /// <typeparam name="TPayload">事件负载类型</typeparam>
        /// <param name="eventKey">事件标识</param>
        /// <param name="payload">事件负载</param>
        void Publish<TPayload>(EventKey<TPayload> eventKey, TPayload payload);
    }
}
