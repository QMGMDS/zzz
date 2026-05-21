using System;
using UnityEngine;

namespace Core.Input
{
    /// <summary>输入抽象接口，支持事件订阅和轮询两种消费模式</summary>
    public interface IInputable
    {
        /// <summary>移动方向变化时触发</summary>
        event Action<Vector2> MoveDirectionChanged;

        /// <summary>当前移动方向（归一化 Vector2）</summary>
        Vector2 MoveDirection { get; }

        /// <summary>闪避输入触发时调用</summary>
        event Action EvadeTriggered;
    }
}