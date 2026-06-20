using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 移动启动拦截器
    /// </summary>
    [CreateAssetMenu(fileName = "WalkStartInterceptor", menuName = "Player/Interceptors/WalkStartInterceptor")]
    public class MoveStartInterceptorSO : StateInterceptorSO
    {
        /// <inheritdoc />
        public override bool TryIntercept(PlayerController player, BaseState currentState, out BaseState nextState)
        {
            nextState = null;

            var blackboard = player.PlayerBrainBlackboard;
            if (blackboard == null) return false;

            var stateType = blackboard.CurrentPlayerState;

            // 豁免检查
            if (IsExempt(stateType)) return false;

            // 拦截器检查
            if (!blackboard.WantToMove) return false;

            switch (stateType)
            {
                case PlayerStateType.Idle:
                case PlayerStateType.Stop:
                case PlayerStateType.EvadeFrontEnd:
                case PlayerStateType.EvadeBackEnd:
                    nextState = player.StateMachine.GetState(PlayerStateType.WalkStart);
                    break;
                case PlayerStateType.EvadeFront:
                    if (blackboard.AnimationCompleted)
                        nextState = player.StateMachine.GetState(PlayerStateType.RunStart);
                    break;
                default:
                    break;
            }
            return nextState != null;
        }
    }
}
