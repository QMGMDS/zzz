using GamePlay.State;
using UnityEngine;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 移动状态机，管理 Idle、Walk、EvadeFront、EvadeBack、Run 状态之间的切换。
    /// EvadeFront 与 EvadeBack 各自独立 CD，同类型 CD 到期后走 ReenterState 重播动画，
    /// 跨类型需来源 CD 与目标 CD 均到期才放行；动画由各 State 的 Enter 通过 CrossFadeInFixedTime 驱动。
    /// </summary>
    public class MovementStateMachine : StateMachine
    {
        private readonly float _evadeFrontCooldown;
        private readonly float _evadeBackCooldown;
        private float _lastEvadeFrontTime = float.MinValue;
        private float _lastEvadeBackTime = float.MinValue;

        public MovementStateMachine(float evadeFrontCooldown = 0.7f, float evadeBackCooldown = 0.7f)
        {
            _evadeFrontCooldown = evadeFrontCooldown;
            _evadeBackCooldown = evadeBackCooldown;

            RegisterState<IdleState>(new IdleState());
            RegisterState<WalkState>(new WalkState());
            RegisterState<EvadeFrontState>(new EvadeFrontState());
            RegisterState<EvadeBackState>(new EvadeBackState());
            RegisterState<RunState>(new RunState());
            RegisterState<NormalAttackState>(new NormalAttackState());
        }

        public override void Update()
        {
            if (_context.IsEvadeTriggered)
            {
                _context.ConsumeEvade();
                bool hasDirection = _context.MoveDirection.sqrMagnitude > 0.0001f;

                if (hasDirection)
                {
                    // 跨类型：需等来源 EvadeBack 的 CD 到期
                    if (CurrentStateType == typeof(EvadeBackState)
                        && Time.time - _lastEvadeBackTime < _evadeBackCooldown)
                        goto HandleAttack;

                    if (Time.time - _lastEvadeFrontTime >= _evadeFrontCooldown)
                    {
                        _lastEvadeFrontTime = Time.time;

                        if (CurrentStateType == typeof(EvadeFrontState))
                            ReenterState<EvadeFrontState>();
                        else
                            ChangeState<EvadeFrontState>();
                    }
                }
                else
                {
                    // 跨类型：需等来源 EvadeFront 的 CD 到期
                    if (CurrentStateType == typeof(EvadeFrontState)
                        && Time.time - _lastEvadeFrontTime < _evadeFrontCooldown)
                        goto HandleAttack;

                    if (Time.time - _lastEvadeBackTime >= _evadeBackCooldown)
                    {
                        _lastEvadeBackTime = Time.time;

                        if (CurrentStateType == typeof(EvadeBackState))
                            ReenterState<EvadeBackState>();
                        else
                            ChangeState<EvadeBackState>();
                    }
                }
            }

            HandleAttack:
            if (_context.IsAttackTriggered && CurrentStateType != typeof(NormalAttackState))
            {
                _context.ConsumeAttack();
                ChangeState<NormalAttackState>();
            }

            base.Update();
        }
    }
}
