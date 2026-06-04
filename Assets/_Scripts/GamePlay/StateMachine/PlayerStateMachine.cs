using System.Collections.Generic;
using GamePlay.State;
using GamePlay.StateMachine.Interceptors;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 玩家状态机，负责状态注册、Interceptor 链驱动与生命周期派遣。
    /// 闪避 CD 逻辑已迁移至 EvadeInterceptor，攻击路由已迁移至 AttackInterceptor。
    /// 所有全局输入判定均通过 Interceptor 链表完成，状态内部仅处理自身专属逻辑。
    /// </summary>
    public class PlayerStateMachine : StateMachineBase
    {
        private readonly List<StateInterceptorBase> _interceptors = new();

        /// <summary>
        /// 初始化玩家状态机，注册全部状态并构建 Interceptor 链表
        /// </summary>
        /// <param name="evadeFrontCooldown">前闪避冷却时间（秒），透传至 EvadeInterceptor</param>
        /// <param name="evadeBackCooldown">后撤步冷却时间（秒），透传至 EvadeInterceptor</param>
        public PlayerStateMachine(float evadeFrontCooldown = 0.7f, float evadeBackCooldown = 0.7f)
        {
            RegisterState<IdleState>(new IdleState());
            RegisterState<WalkState>(new WalkState());
            RegisterState<EvadeFrontState>(new EvadeFrontState());
            RegisterState<EvadeBackState>(new EvadeBackState());
            RegisterState<RunState>(new RunState());
            RegisterState<NormalAttackState>(new NormalAttackState());
            RegisterState<HitState>(new HitState());

            _interceptors.Add(new OverrideInterceptor());
            _interceptors.Add(new EvadeInterceptor(evadeFrontCooldown, evadeBackCooldown));
            _interceptors.Add(new AttackInterceptor());
        }

        /// <summary>
        /// 每帧按优先级遍历 Interceptor 链表，首个返回 true 的拦截器执行状态切换并中断链。
        /// 无论是否被拦截，均会驱动当前状态的 Update，保证状态切换首帧不丢失逻辑。
        /// </summary>
        public override void Update()
        {
            if (!IsCurrentStateInterruptible)
            {
                base.Update();
                return;
            }

            var blackboard = _context.Blackboard;
            foreach (var interceptor in _interceptors)
            {
                if (interceptor.TryIntercept(blackboard, CurrentStateType, this))
                    break;
            }

            base.Update();
        }
    }
}
