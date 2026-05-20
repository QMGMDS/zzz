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
    }
}
