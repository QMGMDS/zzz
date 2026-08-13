using System;

namespace SPFramework.Event
{
    /// <summary>
    /// 类型安全事件标识
    /// </summary>
    /// <typeparam name="TPayload">事件负载类型（消息体）</typeparam>
    public sealed class EventKey<TPayload>
    {
        /// <summary>
        /// 创建事件标识
        /// </summary>
        /// <param name="name">事件调试名</param>
        public EventKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("事件名不能为空", nameof(name));

            Name = name;
        }

        /// <summary>事件调试名</summary>
        public string Name { get; }

        /// <summary>事件负载类型</summary>
        public Type PayloadType => typeof(TPayload);

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}
