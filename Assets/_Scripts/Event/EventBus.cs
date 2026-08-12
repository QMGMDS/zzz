using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPEvent
{
    /// <summary>
    /// 全局事件总线 - 按事件标识分发事实事件
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<object, List<SubscriptionEntry>> _entriesByKey = new();

        private EventBus() { }

        /// <summary>全局唯一事件总线实例</summary>
        public static EventBus Global { get; } = new EventBus();

        /// <inheritdoc />
        public IDisposable Subscribe<TPayload>(EventKey<TPayload> eventKey, Action<TPayload> handler)
        {
            if (eventKey == null)
                throw new ArgumentNullException(nameof(eventKey));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            List<SubscriptionEntry> entries = GetOrCreateEntries(eventKey);
            EnsureNotDuplicate(entries, handler, eventKey);

            SubscriptionEntry entry = new SubscriptionEntry(handler);
            entries.Add(entry);

            return new EventSubscription(() => Remove(eventKey, entry)); // 隐式转成 IDisposable
        }

        /// <inheritdoc />
        public void Publish<TPayload>(EventKey<TPayload> eventKey, TPayload payload)
        {
            if (eventKey == null)
                throw new ArgumentNullException(nameof(eventKey));

            if (!_entriesByKey.TryGetValue(eventKey, out List<SubscriptionEntry> entries))
                return;

            SubscriptionEntry[] snapshot = entries.ToArray(); // 拍下订阅列表快照
            /* 拍下快照的原因说明
                算作一个防御行为
                保证本次事件发布，其订阅者的执行不会在回调中有所改动
                若 A、B 订阅了同一个事件，在事件发布过程中，A 的回调中退订了自己或者 B，亦或者有新的订阅者加入进来了
                如果采用单纯的 for、foreach 遍历，就会导致遍历出问题。
                快照能保证这一轮分发的顺序和范围稳定
            */

            foreach (SubscriptionEntry entry in snapshot)
            {
                if (!entry.IsActive)
                    continue;

                InvokeHandler(eventKey, payload, entry);
            }
        }

        private List<SubscriptionEntry> GetOrCreateEntries<TPayload>(EventKey<TPayload> eventKey)
        {
            if (!_entriesByKey.TryGetValue(eventKey, out List<SubscriptionEntry> entries))
            {
                entries = new List<SubscriptionEntry>();
                _entriesByKey.Add(eventKey, entries);
            }

            return entries;
        }

        private static void EnsureNotDuplicate<TPayload>(
            IEnumerable<SubscriptionEntry> entries, // 只读检查，无任何修改操作
            Action<TPayload> handler,
            EventKey<TPayload> eventKey)
        {
            foreach (SubscriptionEntry entry in entries)
            {
                if (!entry.IsActive || !entry.Matches(handler))
                    continue;

                throw new InvalidOperationException($"重复订阅事件 [{eventKey.Name}]");
            }
        }

        private void Remove<TPayload>(EventKey<TPayload> eventKey, SubscriptionEntry entry)
        {
            entry.Deactivate();

            if (!_entriesByKey.TryGetValue(eventKey, out List<SubscriptionEntry> entries))
                return;

            entries.Remove(entry);

            if (entries.Count == 0)
                _entriesByKey.Remove(eventKey);
        }

        private static void InvokeHandler<TPayload>(
            EventKey<TPayload> eventKey,
            TPayload payload,
            SubscriptionEntry entry)
        {
            try
            {
                ((Action<TPayload>)entry.Handler).Invoke(payload);
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    new InvalidOperationException($"事件处理失败 [{eventKey.Name}]", exception));
            }
        }

        /// <summary>
        /// 订阅条目 - 保存处理函数与激活状态
        /// </summary>
        private sealed class SubscriptionEntry
        {
            public SubscriptionEntry(Delegate handler)
            {
                Handler = handler;
                IsActive = true;
            }

            /// <summary>事件处理函数</summary>
            public Delegate Handler { get; }

            /// <summary>订阅是否仍然有效</summary>
            public bool IsActive { get; private set; }

            /// <summary>判断处理函数是否一致</summary>
            public bool Matches(Delegate handler) => Equals(Handler, handler);

            /// <summary>标记订阅失效</summary>
            public void Deactivate() => IsActive = false;
        }
    }
}


