using System;

namespace SPFramework.Event
{
    /// <summary>
    /// 事件订阅句柄 - 负责执行一次性退订逻辑
    /// </summary>
    internal sealed class EventSubscriptionHandle : IDisposable
    {
        private Action _disposeAction;
        private bool _isDisposed;

        /// <summary>
        /// 创建订阅句柄
        /// </summary>
        /// <param name="disposeAction">退订动作</param>
        public EventSubscriptionHandle(Action disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }

        /// <summary>
        /// 取消当前订阅
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _disposeAction.Invoke();
            _disposeAction = null;
        }
    }
}
