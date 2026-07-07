using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 攻击拦截器
    /// 攻击族状态由各自的 End 状态自治推进攻击链，本拦截器不再介入。
    /// </summary>
    [CreateAssetMenu(fileName = "AttackInterceptor", menuName = "Player/Interceptors/AttackInterceptor")]
    public class AttackInterceptorSO : StateInterceptorSO
    {
        /// <inheritdoc />
        public override bool TryIntercept(PlayerController player, BaseState currentState, out BaseState nextState)
        {
            nextState = null;

            var blackboard = player.PlayerBrainBlackboard;
            if (blackboard == null) return false;

            if (!blackboard.WantToAttack) return false;

            switch (blackboard.CurrentPlayerState)
            {
                case PlayerStateType.Idle:
                case PlayerStateType.Stop:
                case PlayerStateType.WalkStart:
                case PlayerStateType.WalkLoop:
                case PlayerStateType.RunStart:
                case PlayerStateType.RunLoop:
                case PlayerStateType.EvadeFrontEnd:
                case PlayerStateType.EvadeBackEnd:
                    nextState = player.StateMachine.GetState(PlayerStateType.Attack_1);
                    break;

                default:
                    break;
            }

            return nextState != null;
        }
    }
}
