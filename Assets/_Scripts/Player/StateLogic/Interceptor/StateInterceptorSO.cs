using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 状态拦截器 ScriptableObject 基类——定义全局优先级打断规则。
    /// 拦截器之间通过 MainInterceptor 的遍历顺序决定优先级（排前面先抢）。
    /// 提供豁免清单机制，子类在 Inspector 中配置即可。
    /// </summary>
    public abstract class StateInterceptorSO : ScriptableObject
    {
        [Header("豁免清单")]
        [Tooltip("这些状态不会被本拦截器打断")]
        [SerializeField] protected PlayerStateType[] _exemptStates;

        /// <summary>
        /// 尝试拦截当前状态——检查触发条件，如果满足则返回目标状态。
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <param name="currentState">当前激活的状态</param>
        /// <param name="nextState">输出参数 : 拦截成功后要切换到的目标状态</param>
        /// <returns>true = 拦截成功，主拦截器负责逻辑状态切换</returns>
        public abstract bool TryIntercept(PlayerController player, BaseState currentState, out BaseState nextState);

        /// <summary>
        /// 判断指定状态是否在豁免清单中——豁免状态不会被本拦截器打断。
        /// </summary>
        /// <param name="stateType">待检查的状态类型</param>
        /// <returns>true = 豁免，不处理</returns>
        protected bool IsExempt(PlayerStateType stateType)
        {
            if (_exemptStates == null) return false;
            foreach (var state in _exemptStates) if (state == stateType) return true;
            return false;
        }
    }
}
