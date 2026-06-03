using GamePlay.State;
using UnityEngine;

namespace GamePlay.StateMachine
{
    /// <summary>
    /// 玩家状态机，管理全部玩家状态间的输入路由与切换。
    /// 闪避各方向独立 CD，同类型 CD 到期后重播动画，跨类型需双方 CD 均到期才放行。
    /// </summary>
    public class PlayerStateMachine : StateMachineBase
    {
        private readonly float _evadeFrontCooldown;
        private readonly float _evadeBackCooldown;
        private float _lastEvadeFrontTime = float.MinValue;
        private float _lastEvadeBackTime = float.MinValue;

        /// <summary>初始化玩家状态机并注册全部状态</summary>
        /// <param name="evadeFrontCooldown">前闪避冷却时间</param>
        /// <param name="evadeBackCooldown">后撤步冷却时间</param>
        public PlayerStateMachine(float evadeFrontCooldown = 0.7f, float evadeBackCooldown = 0.7f)
        {
            _evadeFrontCooldown = evadeFrontCooldown;
            _evadeBackCooldown = evadeBackCooldown;

            RegisterState<IdleState>(new IdleState());
            RegisterState<WalkState>(new WalkState());
            RegisterState<EvadeFrontState>(new EvadeFrontState());
            RegisterState<EvadeBackState>(new EvadeBackState());
            RegisterState<RunState>(new RunState());
            RegisterState<NormalAttackState>(new NormalAttackState());
            RegisterState<HitState>(new HitState());
        }

        /// <summary>每帧检查输入并路由到对应状态，优先处理闪避再处理攻击</summary>
        public override void Update()
        {
            if (!IsCurrentStateInterruptible)
            {
                base.Update();
                return;
            }

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
