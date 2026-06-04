using System;
using GamePlay.Player;

namespace GamePlay.StateMachine.Interceptors
{
    /// <summary>
    /// 强制覆盖拦截器，处理外部驱动的高优先级状态切换（伤害 → HitState 等）。
    /// 当前为预留骨架，实际伤害逻辑暂由 PlayerController.TakeDamage 直接驱动，
    /// 待后续将进攻队列迁移至黑板后接入。
    /// </summary>
    public class OverrideInterceptor : StateInterceptorBase
    {
        /// <inheritdoc/>
        public override bool TryIntercept(PlayerBlackboard blackboard, Type currentStateType, StateMachineBase stateMachine)
        {
            return false;
        }
    }
}
