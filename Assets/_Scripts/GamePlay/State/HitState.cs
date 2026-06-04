using GamePlay.Common;

namespace GamePlay.State
{
    /// <summary>
    /// 受击状态：播放 Hit_Front 动画，击退靠动画 Root Motion 驱动。
    /// InterruptNormalizedTime 之前不可被打断，之后由 PlayerStateMachine 正常路由输入。
    /// </summary>
    public class HitState : StateBase
    {
        private const float CrossFadeDuration = 0.1f;
        private const float InterruptNormalizedTime = 0.1f;

        private bool _isInterruptible;

        public override bool IsInterruptible => _isInterruptible;

        public override void Enter(IStateContext context)
        {
            Context = context;
            Context.Animator.CrossFadeInFixedTime(AnimationHashes.Hit_Front, CrossFadeDuration);
            _isInterruptible = false;
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
            float normalizedTime = Context.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            if (!_isInterruptible && normalizedTime >= InterruptNormalizedTime)
                _isInterruptible = true;

            if (normalizedTime >= 0.9f)
                Context.StateMachine.ChangeState<IdleState>();
        }
    }
}
