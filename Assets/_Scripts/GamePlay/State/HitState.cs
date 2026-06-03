using GamePlay.Combat;
using GamePlay.Common;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 受击状态：播放 Hit_Front 动画并执行击退。
    /// 在 InterruptNormalizedTime 之前不可被打断，之后由 PlayerStateMachine 正常路由输入。
    /// 击退速度逐帧衰减至零。
    /// </summary>
    public class HitState : StateBase
    {
        private const float CrossFadeDuration = 0.1f;
        private const float InterruptNormalizedTime = 0.5f;
        private const float KnockbackDecay = 5f;
        private const float DefaultKnockbackForce = 5f;

        private Vector3 _knockbackVelocity;
        private bool _isInterruptible;

        public override bool IsInterruptible => _isInterruptible;

        public override void Enter(IStateContext context)
        {
            Context = context;
            Context.Animator.CrossFadeInFixedTime(AnimationHashes.Hit_Front, CrossFadeDuration);
            _isInterruptible = false;

            if (context is Player.PlayerController player)
            {
                DamageInfo info = player.GetPendingDamageInfo();
                float force = info.KnockbackForce > 0f ? info.KnockbackForce : DefaultKnockbackForce;
                _knockbackVelocity = info.KnockbackDirection * force;
            }
        }

        public override void Exit()
        {
            _knockbackVelocity = Vector3.zero;
        }

        public override void Update()
        {
            _knockbackVelocity *= (1f - KnockbackDecay * Time.deltaTime);
            if (_knockbackVelocity.sqrMagnitude > 0.0001f)
            {
                Context.CharacterController.Move(_knockbackVelocity * Time.deltaTime);
            }

            AnimatorStateInfo stateInfo = Context.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != AnimationHashes.Hit_Front) return;

            float normalizedTime = stateInfo.normalizedTime;

            if (!_isInterruptible && normalizedTime >= InterruptNormalizedTime)
            {
                _isInterruptible = true;
            }

            if (normalizedTime >= 0.9f)
            {
                Context.StateMachine.ChangeState<IdleState>();
            }
        }
    }
}
