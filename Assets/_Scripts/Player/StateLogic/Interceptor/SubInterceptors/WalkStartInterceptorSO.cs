using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 行走启动拦截器——从静态族转移到行走族
    /// </summary>
    [CreateAssetMenu(fileName = "WalkStartInterceptor", menuName = "Player/Interceptors/WalkStartInterceptor")]
    public class WalkStartInterceptorSO : StateInterceptorSO
    {
        /// <summary>
        /// 尝试拦截——检测玩家是否有移动输入，若有则从静止状态族跳转到 WalkStart。
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

            if (blackboard.WantToMove)
            {
                nextState = player.StateMachine.GetState(PlayerStateType.WalkStart);
                return true;
            }

            return false;
        }
    }
}
