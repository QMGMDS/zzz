using System;
using System.Collections.Generic;
using GamePlay.State;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 状态机基类，提供状态注册、切换与生命周期驱动的通用实现
    /// </summary>
    public abstract class StateMachine : IStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();
        private IState _currentState;

        /// <summary>状态上下文，子类可访问以在 Update 等生命周期方法中读取</summary>
        protected IStateContext _context;

        public Type CurrentStateType => _currentState?.GetType();

        /// <summary>注册一个状态实例，与泛型类型绑定</summary>
        protected void RegisterState<T>(IState state) where T : IState
        {
            _states[typeof(T)] = state;
        }

        /// <summary>初始化上下文并进入指定的默认状态</summary>
        public void Initialize<T>(IStateContext context) where T : IState
        {
            _context = context;
            ChangeState<T>();
        }

        /// <summary>切换到指定类型的状态，同状态切换会被忽略</summary>
        public void ChangeState<T>() where T : IState
        {
            Type targetType = typeof(T);
            if (!_states.TryGetValue(targetType, out IState newState)) return;
            if (newState == _currentState) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter(_context);
        }

        /// <summary>每帧调用当前状态的 Update，子类可重写以插入状态机层逻辑</summary>
        public virtual void Update()
        {
            _currentState?.Update();
        }

        /// <summary>每帧 LateUpdate，在 Animator 更新后调用</summary>
        public void LateUpdate()
        {
            _currentState?.LateUpdate();
        }

        /// <summary>每物理帧调用当前状态的 PhysicsUpdate</summary>
        public void PhysicsUpdate()
        {
            _currentState?.PhysicsUpdate();
        }
    }
}
