using System;
using System.Collections.Generic;
using GamePlay.State;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 状态机基类，提供状态注册、切换与生命周期驱动的通用实现
    /// </summary>
    public abstract class StateMachineBase
    {
        private readonly Dictionary<Type, StateBase> _states = new();
        private StateBase _currentState;

        /// <summary>状态上下文，子类可访问以在生命周期方法中读取依赖</summary>
        protected IStateContext _context;

        /// <summary>当前状态类型，用于外部查询</summary>
        public Type CurrentStateType => _currentState?.GetType();

        /// <summary>当前状态是否可被输入打断，状态不可打断时 PlayerStateMachine 跳过输入路由</summary>
        public bool IsCurrentStateInterruptible => _currentState?.IsInterruptible ?? true;

        /// <summary>注册一个状态实例，与泛型类型绑定</summary>
        /// <param name="state">状态实例</param>
        /// <typeparam name="T">状态类型，作为字典键</typeparam>
        protected void RegisterState<T>(StateBase state) where T : StateBase
        {
            _states[typeof(T)] = state;
        }

        /// <summary>初始化上下文并进入指定的默认状态</summary>
        /// <param name="context">状态上下文</param>
        /// <typeparam name="T">默认状态类型</typeparam>
        public void Initialize<T>(IStateContext context) where T : StateBase
        {
            _context = context;
            ChangeState<T>();
        }

        /// <summary>切换到指定类型的状态，同状态切换会被忽略</summary>
        /// <typeparam name="T">目标状态类型</typeparam>
        public void ChangeState<T>() where T : StateBase
        {
            Type targetType = typeof(T);
            if (!_states.TryGetValue(targetType, out StateBase newState)) return;
            if (newState == _currentState) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter(_context);
        }

        /// <summary>强制重入目标状态，即使已是该状态也会执行 Exit → Enter</summary>
        /// <typeparam name="T">目标状态类型</typeparam>
        public void ReenterState<T>() where T : StateBase
        {
            Type targetType = typeof(T);
            if (!_states.TryGetValue(targetType, out StateBase newState)) return;

            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter(_context);
        }

        /// <summary>每帧调用当前状态的 Update，子类可重写以插入状态机层逻辑</summary>
        public virtual void Update()
        {
            _currentState?.Update();
        }

        /// <summary>每帧在 Animator 更新后调用当前状态的 LateUpdate</summary>
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
