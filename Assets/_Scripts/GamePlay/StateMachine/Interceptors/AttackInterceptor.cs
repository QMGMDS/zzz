using System;
using GamePlay.Player;
using GamePlay.State;

namespace GamePlay.StateMachine.Interceptors
{
    /// <summary>
    /// 攻击输入拦截器：检测攻击缓冲标记，将当前可打断状态切换至 NormalAttackState。
    /// 若已在攻击状态中则放行，由 NormalAttackState 内部 ComboWindow 自行处理连击。
    /// </summary>
    public class AttackInterceptor : StateInterceptorBase
    {
        /// <inheritdoc/>
        public override bool TryIntercept(PlayerBlackboard blackboard, Type currentStateType, StateMachineBase stateMachine)
        {
            if (!blackboard.IsAttackBuffered) return false;
            if (currentStateType == typeof(NormalAttackState)) return false;

            blackboard.ConsumeAttack();
            stateMachine.ChangeState(typeof(NormalAttackState));
            return true;
        }
    }
}
