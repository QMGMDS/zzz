using System;
using GamePlay.Player;
using GamePlay.State;
using UnityEngine;

namespace GamePlay.StateMachine.Interceptors
{
    /// <summary>
    /// 闪避输入拦截器：检测闪避缓冲标记并根据方向分别执行前闪避或后撤步。
    /// 正向与后向闪避各自独立 CD，跨类型需双方 CD 均到期才放行。
    /// 若因 CD 阻拦则消费输入后放行，链路自然传递至后续拦截器。
    /// </summary>
    public class EvadeInterceptor : StateInterceptorBase
    {
        private readonly float _frontCooldown;
        private readonly float _backCooldown;

        private float _lastFrontTime = float.MinValue;
        private float _lastBackTime = float.MinValue;

        /// <summary>
        /// 创建闪避拦截器
        /// </summary>
        /// <param name="frontCooldown">前闪避冷却时间（秒）</param>
        /// <param name="backCooldown">后撤步冷却时间（秒）</param>
        public EvadeInterceptor(float frontCooldown, float backCooldown)
        {
            _frontCooldown = frontCooldown;
            _backCooldown = backCooldown;
        }

        /// <inheritdoc/>
        public override bool TryIntercept(PlayerBlackboard blackboard, Type currentStateType, StateMachineBase stateMachine)
        {
            if (!blackboard.IsEvadeBuffered) return false;

            blackboard.ConsumeEvade();

            bool hasDirection = blackboard.MoveDirection.sqrMagnitude > 0.0001f;

            if (hasDirection)
                return TryEvadeFront(currentStateType, stateMachine);
            else
                return TryEvadeBack(currentStateType, stateMachine);
        }

        private bool TryEvadeFront(Type currentStateType, StateMachineBase stateMachine)
        {
            if (currentStateType == typeof(EvadeBackState)
                && Time.time - _lastBackTime < _backCooldown)
                return false;

            if (Time.time - _lastFrontTime < _frontCooldown)
                return false;

            _lastFrontTime = Time.time;

            if (currentStateType == typeof(EvadeFrontState))
                stateMachine.ReenterState(typeof(EvadeFrontState));
            else
                stateMachine.ChangeState(typeof(EvadeFrontState));

            return true;
        }

        private bool TryEvadeBack(Type currentStateType, StateMachineBase stateMachine)
        {
            if (currentStateType == typeof(EvadeFrontState)
                && Time.time - _lastFrontTime < _frontCooldown)
                return false;

            if (Time.time - _lastBackTime < _backCooldown)
                return false;

            _lastBackTime = Time.time;

            if (currentStateType == typeof(EvadeBackState))
                stateMachine.ReenterState(typeof(EvadeBackState));
            else
                stateMachine.ChangeState(typeof(EvadeBackState));

            return true;
        }
    }
}
