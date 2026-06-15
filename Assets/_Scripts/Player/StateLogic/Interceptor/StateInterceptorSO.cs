using System.Collections.Generic;
using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 状态拦截器 ScriptableObject 基类——定义全局优先级打断规则。
    /// 每个拦截器包含一个豁免清单：清单内的状态不会被本拦截器打断。
    /// 拦截器之间通过 GlobalInterruptProcessor 的遍历顺序决定优先级（排前面先抢）。
    /// </summary>
    public abstract class StateInterceptorSO : ScriptableObject
    {
        [Header("豁免清单")]
        [Tooltip("这些状态不会被本拦截器打断")]
        [SerializeField] private PlayerStateType[] _exemptStates;

        private HashSet<PlayerStateType> _exemptSet;
        private bool _exemptSetInitialized;

        /// <summary>豁免状态集合（懒初始化 + 缓存）</summary>
        protected HashSet<PlayerStateType> ExemptSet
        {
            get
            {
                if (!_exemptSetInitialized)
                {
                    _exemptSet = new HashSet<PlayerStateType>(_exemptStates ?? new PlayerStateType[0]);
                    _exemptSetInitialized = true;
                }
                return _exemptSet;
            }
        }

        /// <summary>
        /// 尝试拦截当前状态——检查触发条件，如果满足且当前状态不在豁免清单中，则返回目标状态。
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <param name="currentState">当前激活的状态</param>
        /// <param name="nextState">输出参数 : 拦截成功后要切换到的目标状态</param>
        /// <returns>true = 拦截成功，主拦截器负责逻辑状态切换</returns>
        public abstract bool TryIntercept(PlayerController player, BaseState currentState, out BaseState nextState);
    }
}
