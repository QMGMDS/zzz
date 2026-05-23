using GamePlay.State;
using UnityEngine;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 移动状态机，管理 Idle、Walk、EvadeFront、EvadeBack、Run 状态之间的切换。
    /// 闪避由状态机层统一拦截，任意状态均可触发（闪避状态自身除外）。
    /// 动画切换由各 State 的 Enter 通过 CrossFadeInFixedTime 驱动，Animator Controller 负责混合。
    /// </summary>
    public class MovementStateMachine : StateMachine
    {
        private readonly float _evadeCooldown;
        private float _lastEvadeTime = float.MinValue;

        public MovementStateMachine(float evadeCooldown = 0.5f)
        {
            _evadeCooldown = evadeCooldown;

            RegisterState<IdleState>(new IdleState());
            RegisterState<WalkState>(new WalkState());
            RegisterState<EvadeFrontState>(new EvadeFrontState());
            RegisterState<EvadeBackState>(new EvadeBackState());
            RegisterState<RunState>(new RunState());
        }

        public override void Update()
        {
            if (_context.IsEvadeTriggered)
            {
                _context.ConsumeEvade();

                if (Time.time - _lastEvadeTime < _evadeCooldown)
                    return;

                bool inAnyEvade = CurrentStateType == typeof(EvadeFrontState)
                               || CurrentStateType == typeof(EvadeBackState);

                if (!inAnyEvade)
                {
                    _lastEvadeTime = Time.time;

                    if (_context.MoveDirection.sqrMagnitude > 0.0001f)
                    {
                        ChangeState<EvadeFrontState>();
                    }
                    else
                    {
                        ChangeState<EvadeBackState>();
                    }
                }
            }

            base.Update();
        }
    }
}
