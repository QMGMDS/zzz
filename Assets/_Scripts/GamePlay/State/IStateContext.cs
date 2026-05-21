using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 状态上下文接口，提供状态执行所需的游戏世界依赖
    /// </summary>
    public interface IStateContext
    {
        /// <summary>角色 Animator 组件</summary>
        Animator Animator { get; }

        /// <summary>角色 CharacterController 组件</summary>
        CharacterController CharacterController { get; }

        /// <summary>角色 Transform</summary>
        Transform Transform { get; }

        /// <summary>当前帧输入方向（轮询模式）</summary>
        Vector2 MoveDirection { get; }

        /// <summary>主摄像机</summary>
        Camera MainCamera { get; }

        /// <summary>当前状态机引用，供状态内部触发切换</summary>
        IStateMachine StateMachine { get; }

        /// <summary>输入缓冲时间（秒），在该时间内持续无输入才判定为停止</summary>
        float InputBufferTime { get; }

        /// <summary>当前帧是否有闪避输入待消费</summary>
        bool IsEvadeTriggered { get; }

        /// <summary>消费闪避输入标记，防止重复触发</summary>
        void ConsumeEvade();
    }
}
