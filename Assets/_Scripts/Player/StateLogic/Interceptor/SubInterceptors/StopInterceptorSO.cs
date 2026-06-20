using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 停止拦截器
    /// </summary>
    [CreateAssetMenu(fileName = "StopInterceptor", menuName = "Player/Interceptors/StopInterceptor")]
    public class StopInterceptorSO : StateInterceptorSO
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
            if (blackboard.WantToMove) return false;

            switch (stateType)
            {
                case PlayerStateType.WalkStart:
                case PlayerStateType.WalkLoop:
                case PlayerStateType.RunStart:
                case PlayerStateType.RunLoop:
                    nextState = player.StateMachine.GetState(PlayerStateType.Stop);
                    break;
                case PlayerStateType.RunTurn:
                    if (blackboard.AnimationCompleted)
                        nextState = player.StateMachine.GetState(PlayerStateType.Stop);
                    break;
                case PlayerStateType.EvadeFrontEnd:
                case PlayerStateType.EvadeBackEnd:
                    if (blackboard.AnimationCompleted)
                        nextState = player.StateMachine.GetState(PlayerStateType.Idle);
                    break;
                default:
                    break;
            }
            return nextState != null;
        }
    }
}
