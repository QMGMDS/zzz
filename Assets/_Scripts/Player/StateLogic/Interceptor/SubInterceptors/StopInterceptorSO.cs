using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 停止拦截器——从行走族转移到静止族
    /// </summary>
    [CreateAssetMenu(fileName = "StopInterceptor", menuName = "Player/Interceptors/StopInterceptor")]
    public class StopInterceptorSO : StateInterceptorSO
    {
        /// <summary>
        /// 尝试拦截——检测玩家是否松开了移动输入，若是则从行走状态族跳转到 Stop。
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <param name="currentState">当前激活的状态</param>
        /// <param name="nextState">输出参数 : 拦截成功后要切换到的目标状态</param>
        /// <returns>true = 拦截成功</returns>
        public override bool TryIntercept(PlayerController player, BaseState currentState, out BaseState nextState)
        {
            nextState = null;

            var blackboard = player.PlayerBrainBlackboard;
            if (blackboard == null) return false;

            var stateType = blackboard.CurrentPlayerState;

            // 豁免检查
            if (IsExempt(stateType)) return false;

            if (!blackboard.WantToMove)
            {
                nextState = player.StateMachine.GetState(PlayerStateType.Stop);
                return true;
            }

            return false;
        }
    }
}
