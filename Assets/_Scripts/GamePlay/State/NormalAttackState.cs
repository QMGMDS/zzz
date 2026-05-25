using GamePlay.Common;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 四段普攻连击状态，内部通过三阶段（Playing / ComboWindow / Ending）管理全部连击逻辑。
    /// 动画结束后开启连击窗口：窗口内收到攻击输入则进入下一段，超时则播放对应 _End 动画。
    /// 四段连击后输入则回到第一段继续。
    /// </summary>
    public class NormalAttackState : StateBase
    {
        private enum Phase
        {
            Playing,
            ComboWindow,
            Ending
        }

        private const float CrossFadeDuration = 0.05f;

        private static readonly int[] AttackHashes =
        {
            AnimationHashes.NormalAttack1,
            AnimationHashes.NormalAttack2,
            AnimationHashes.NormalAttack3,
            AnimationHashes.NormalAttack4,
        };

        private static readonly int[] EndHashes =
        {
            AnimationHashes.NormalAttack1End,
            AnimationHashes.NormalAttack2End,
            AnimationHashes.NormalAttack3End,
            AnimationHashes.NormalAttack4End,
        };

        private Phase _phase;
        private int _comboIndex;
        private float _windowTimer;
        private int _playingEndHash;

        public override void Enter(IStateContext context)
        {
            Context = context;
            _phase = Phase.Playing;
            _comboIndex = 0;
            Context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
            switch (_phase)
            {
                case Phase.Playing:
                    UpdatePlaying();
                    break;
                case Phase.ComboWindow:
                    UpdateComboWindow();
                    break;
                case Phase.Ending:
                    UpdateEnding();
                    break;
            }
        }

        private void UpdatePlaying()
        {
            int targetHash = AttackHashes[_comboIndex];
            AnimatorStateInfo stateInfo = Context.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != targetHash) return;

            if (stateInfo.normalizedTime >= 0.9f)
            {
                _playingEndHash = EndHashes[_comboIndex];
                Context.Animator.CrossFadeInFixedTime(_playingEndHash, CrossFadeDuration);
                _phase = Phase.ComboWindow;
                _windowTimer = 0f;
            }
        }

        private void UpdateComboWindow()
        {
            _windowTimer += Time.deltaTime;

            if (Context.IsAttackTriggered)
            {
                Context.ConsumeAttack();
                _comboIndex++;
                if (_comboIndex >= AttackHashes.Length) _comboIndex = 0;
                Context.Animator.CrossFadeInFixedTime(AttackHashes[_comboIndex], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            if (_windowTimer >= Context.ComboWindowDuration)
            {
                _comboIndex = 0;
                _phase = Phase.Ending;
            }
        }

        private void UpdateEnding()
        {
            if (Context.IsAttackTriggered)
            {
                Context.ConsumeAttack();
                _comboIndex = 0;
                Context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            AnimatorStateInfo stateInfo = Context.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != _playingEndHash) return;
            if (stateInfo.normalizedTime >= 1f)
                Context.StateMachine.ChangeState<IdleState>();
        }
    }
}
