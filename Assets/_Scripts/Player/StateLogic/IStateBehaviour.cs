namespace SPPlayer
{
    /// <summary>
    /// 状态行为插件运行时接口——每个节点激活时由 GroupStateMachine 创建实例，退出时销毁。
    /// 用于处理少数需要复杂逻辑的状态（如蓄力分支、旋转快照补偿）。
    /// </summary>
    public interface IStateBehaviour
    {
        /// <summary>
        /// 进入节点时调用
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        void OnEnter(PlayerController player);

        /// <summary>
        /// 每帧 Update 调用，在族间拦截器检查之后、族内规则检测之前执行。
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <returns>true = 本行为已触发转移，GroupStateMachine 跳过本帧的规则检测</returns>
        bool OnUpdate(PlayerController player);

        /// <summary>
        /// 退出节点时调用
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        void OnExit(PlayerController player);
    }
}
