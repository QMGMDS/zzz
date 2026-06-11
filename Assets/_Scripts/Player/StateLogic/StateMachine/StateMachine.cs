namespace SPPlayer
{
    /// <summary>
    /// 状态机——持有当前状态引用，提供状态切换与初始化。
    /// 不包含任何游戏逻辑，只负责状态的 Enter/Exit 生命周期调用。
    /// </summary>
    public class StateMachine
    {
        /// <summary>当前激活状态</summary>
        public BaseState CurrentState { get; private set; }

        /// <summary>
        /// 初始化状态机——进入起始状态。
        /// </summary>
        /// <param name="startingState">起始状态实例</param>
        public void Initialize(BaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        /// <summary>
        /// 切换状态——先 Exit 当前状态，再 Enter 新状态。
        /// </summary>
        /// <param name="newState">目标状态实例</param>
        public void ChangeState(BaseState newState)
        {
            if (CurrentState != null)
                CurrentState.Exit();

            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}
