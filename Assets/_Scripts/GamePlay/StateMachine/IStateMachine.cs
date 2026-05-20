using System;
using GamePlay.State;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 状态机接口，供具体状态在内部触发状态切换
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>切换到指定类型的状态</summary>
        void ChangeState<T>() where T : IState;

        /// <summary>当前状态的类型</summary>
        Type CurrentStateType { get; }
    }
}
