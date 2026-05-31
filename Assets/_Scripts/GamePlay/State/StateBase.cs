namespace GamePlay.State
{
    /// <summary>状态抽象基类，提供生命周期默认实现与通用工具方法</summary>
    public abstract class StateBase
    {
        protected IStateContext Context;

        /// <summary>进入状态时调用，初始化状态数据</summary>
        /// <param name="context">状态上下文，提供角色依赖</param>
        public abstract void Enter(IStateContext context);

        /// <summary>退出状态时调用，清理状态数据</summary>
        public abstract void Exit();

        /// <summary>每帧调用，处理状态逻辑</summary>
        public virtual void Update() { }

        /// <summary>每帧在 Animator 更新后调用，处理旋转等覆盖逻辑</summary>
        public virtual void LateUpdate() { }

        /// <summary>每物理帧调用，处理物理相关逻辑</summary>
        public virtual void PhysicsUpdate() { }

        /// <summary>检查 Animator 当前是否在指定状态</summary>
        /// <param name="stateHash">目标状态的 shortNameHash</param>
        /// <returns>当前正在播放该动画则为 true</returns>
        protected bool IsInAnimatorState(int stateHash)
        {
            return Context.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
        }
    }
}
