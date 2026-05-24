using GamePlay.Common;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 四段普攻连击状态，内部通过三阶段（Playing / ComboWindow / Ending）管理全部连击逻辑。
    /// 动画结束后开启连击窗口：窗口内收到攻击输入则进入下一段，超时则播放对应 _End 动画。
    /// 四段连击后输入则回到第一段继续。
    /// </summary>
    public class NormalAttackState : IState
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

        private IStateContext _context;
        private Phase _phase;
        private int _comboIndex;
        private float _windowTimer;
        private int _playingEndHash;

        public void Enter(IStateContext context)
        {
            _context = context;
            _phase = Phase.Playing;
            _comboIndex = 0;
            _context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
        }

        public void Exit()
        {
        }

        public void Update()
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
            AnimatorStateInfo stateInfo = _context.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != targetHash) return;

            if (stateInfo.normalizedTime >= 0.9f)
            {
                _playingEndHash = EndHashes[_comboIndex];
                _context.Animator.CrossFadeInFixedTime(_playingEndHash, CrossFadeDuration);
                _phase = Phase.ComboWindow;
                _windowTimer = 0f;
            }
        }

        private void UpdateComboWindow()
        {
            _windowTimer += Time.deltaTime;

            if (_context.IsAttackTriggered)
            {
                _context.ConsumeAttack();
                _comboIndex++;
                if (_comboIndex >= AttackHashes.Length) _comboIndex = 0;
                _context.Animator.CrossFadeInFixedTime(AttackHashes[_comboIndex], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            if (_windowTimer >= _context.ComboWindowDuration)
            {
                _comboIndex = 0;
                _phase = Phase.Ending;
            }
        }

        private void UpdateEnding()
        {
            if (_context.IsAttackTriggered)
            {
                _context.ConsumeAttack();
                _comboIndex = 0;
                _context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            AnimatorStateInfo stateInfo = _context.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != _playingEndHash) return;
            if (stateInfo.normalizedTime >= 1f)
                _context.StateMachine.ChangeState<IdleState>();
        }

        public void LateUpdate()
        {
        }

        public void PhysicsUpdate()
        {
        }
    }
}
