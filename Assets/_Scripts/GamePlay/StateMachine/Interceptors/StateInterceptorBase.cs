using System;
using GamePlay.Player;

namespace GamePlay.StateMachine.Interceptors
{
    /// <summary>
    /// 状态拦截器基类，提供全局输入到状态切换的拦截判定。
    /// Interceptor 链表按优先级依次调用，首个返回 true 的拦截器生效，
    /// 后续拦截器不再执行，最后落入当前状态的内部 Update 逻辑。
    /// </summary>
    public abstract class StateInterceptorBase
    {
        /// <summary>
        /// 尝试拦截当前输入并执行状态切换
        /// </summary>
        /// <param name="blackboard">玩家意图黑板，提供本帧输入标记与配置参数</param>
        /// <param name="currentStateType">当前状态的 Type，供跨类型 CD 判定用</param>
        /// <param name="stateMachine">状态机引用，执行 ChangeState / ReenterState</param>
        /// <returns>true — 已拦截并完成状态切换；false — 放行到下一个拦截器</returns>
        public abstract bool TryIntercept(PlayerBlackboard blackboard, Type currentStateType, StateMachineBase stateMachine);
    }
}
