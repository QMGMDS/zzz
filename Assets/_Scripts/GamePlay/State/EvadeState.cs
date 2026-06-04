using System;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 闪避状态基类，通过 EvadeType 参数化区分前闪避与后撤步。
    /// 动画播完后 CD 时间（软直）内不可被移动打断，CD 结束后根据输入决定后续状态。
    /// </summary>
    public class EvadeState : StateBase
    {
        /// <summary>闪避方向类型，由子类在构造时指定</summary>
        protected enum EvadeType
        {
            Front,
            Back
        }

        private const float NaturalExitThreshold = 0.95f;

        private readonly EvadeType _type;
        private float _animEnterTime;
        private bool _hasEnteredAnimState;

        /// <summary>
        /// 创建指定方向的闪避状态
        /// </summary>
        /// <param name="type">闪避方向</param>
        protected EvadeState(EvadeType type)
        {
            _type = type;
        }

        /// <inheritdoc/>
        public override void Enter(IStateContext context)
        {
            Context = context;
            Context.Animator.CrossFadeInFixedTime(GetAnimHash(), GetCrossFadeDuration());
            _hasEnteredAnimState = false;
        }

        /// <inheritdoc/>
        public override void Exit()
        {
        }

        /// <inheritdoc/>
        public override void Update()
        {
            int targetHash = GetAnimHash();
            AnimatorStateInfo stateInfo = Context.Animator.GetCurrentAnimatorStateInfo(0);

            if (!_hasEnteredAnimState)
            {
                if (stateInfo.shortNameHash == targetHash)
                {
                    _hasEnteredAnimState = true;
                    _animEnterTime = Time.time;
                }

                return;
            }

            if (stateInfo.shortNameHash != targetHash) return;

            if (Time.time - _animEnterTime < GetCommitDuration()) return;

            if (Context.Blackboard.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState(GetNextStateType());
                return;
            }

            if (stateInfo.normalizedTime >= NaturalExitThreshold)
                Context.StateMachine.ChangeState<IdleState>();
        }

        private int GetAnimHash()
        {
            return _type == EvadeType.Front
                ? Common.AnimationHashes.EvadeFront
                : Common.AnimationHashes.EvadeBack;
        }

        private float GetCrossFadeDuration()
        {
            return _type == EvadeType.Front ? 0.1f : 0.05f;
        }

        private float GetCommitDuration()
        {
            return _type == EvadeType.Front
                ? Context.EvadeFrontCommitDuration
                : Context.EvadeBackCommitDuration;
        }

        private Type GetNextStateType()
        {
            return _type == EvadeType.Front ? typeof(RunState) : typeof(WalkState);
        }
    }
}
