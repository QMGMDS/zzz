namespace GamePlay.State
{
    /// <summary>
    /// 状态抽象基类，提供生命周期默认实现与通用工具方法，
    /// 具体状态继承此类并按需重写 Enter/Exit/Update/LateUpdate/PhysicsUpdate
    /// </summary>
    public abstract class StateBase
    {
        protected IStateContext Context;

        public abstract void Enter(IStateContext context);
        public abstract void Exit();

        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void PhysicsUpdate() { }

        protected bool IsInAnimatorState(int stateHash)
        {
            return Context.Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash;
        }
    }
}
