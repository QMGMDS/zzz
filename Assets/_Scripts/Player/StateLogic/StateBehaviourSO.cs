using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 状态行为插件 SO 基类——为需要复杂逻辑的少数状态提供代码注入。
    /// SO 是共享资产，通过 CreateRuntime() 为每个角色创建独立的运行时实例。
    /// </summary>
    public abstract class StateBehaviourSO : ScriptableObject
    {
        /// <summary>
        /// 创建本行为的运行时实例——每个角色激活该节点时调用一次。
        /// </summary>
        /// <returns>运行时行为实例</returns>
        public abstract IStateBehaviour CreateRuntime();
    }
}
