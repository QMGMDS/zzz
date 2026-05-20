namespace GamePlay.State
{
    /// <summary>
    /// 状态接口，定义状态生命周期的标准方法
    /// </summary>
    public interface IState
    {
        /// <summary>进入状态时调用，接收上下文引用</summary>
        void Enter(IStateContext context);

        /// <summary>退出状态时调用</summary>
        void Exit();

        /// <summary>每帧逻辑更新</summary>
        void Update();

        /// <summary>每物理帧更新</summary>
        void PhysicsUpdate();
    }
}
