using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 闪避拦截器
    /// </summary>
    [CreateAssetMenu(fileName = "EvadeInterceptor", menuName = "Player/Interceptors/EvadeInterceptor")]
    public class EvadeInterceptorSO : StateInterceptorSO
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
            if (!blackboard.WantToEvade) return false;

            switch (stateType)
            {
                case PlayerStateType.Idle:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeBack);
                    break;
                case PlayerStateType.Stop:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeBack);
                    break;
                case PlayerStateType.WalkStart:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeFront);
                    break;
                case PlayerStateType.WalkLoop:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeFront);
                    break;
                case PlayerStateType.RunStart:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeFront);
                    break;
                case PlayerStateType.RunLoop:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeFront);
                    break;
                case PlayerStateType.EvadeFrontEnd:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeFront);
                    break;
                case PlayerStateType.EvadeBackEnd:
                    nextState = player.StateMachine.GetState(PlayerStateType.EvadeBack);
                    break;
                default:
                    break;
            }
            return nextState != null;
        }
    }
}
