using GamePlay.Common;
using GamePlay.State;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 移动状态机，管理 Idle、Walk、EvadeFront、Run 状态之间的切换。
    /// 闪避由状态机层统一拦截，任意状态均可触发（EvadeFront 自身除外）。
    /// </summary>
    public class MovementStateMachine : StateMachine
    {
        public MovementStateMachine()
        {
            RegisterState<IdleState>(new IdleState());
            RegisterState<WalkState>(new WalkState());
            RegisterState<EvadeFrontState>(new EvadeFrontState());
            RegisterState<RunState>(new RunState());
        }

        public override void Update()
        {
            if (_context.IsEvadeTriggered)
            {
                _context.ConsumeEvade();

                if (CurrentStateType != typeof(EvadeFrontState))
                {
                    _context.Animator.SetTrigger(AnimationHashes.Evade);
                    ChangeState<EvadeFrontState>();
                }
            }

            base.Update();
        }
    }
}
