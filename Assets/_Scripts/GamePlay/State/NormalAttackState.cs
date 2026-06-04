using GamePlay.Combat;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 四段普攻连击状态，内部通过三阶段（Playing / ComboWindow / Ending）管理全部连击逻辑。
    /// 动画结束后开启连击窗口：窗口内收到攻击输入则进入下一段，超时则播放对应 _End 动画。
    /// 碰撞体启停与特效生成分别委托给 HitboxService 与 EffectService。
    /// 锁敌旋转委托给 MotionDriver。
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
            Common.AnimationHashes.NormalAttack1,
            Common.AnimationHashes.NormalAttack2,
            Common.AnimationHashes.NormalAttack3,
            Common.AnimationHashes.NormalAttack4,
        };

        private static readonly int[] EndHashes =
        {
            Common.AnimationHashes.NormalAttack1End,
            Common.AnimationHashes.NormalAttack2End,
            Common.AnimationHashes.NormalAttack3End,
            Common.AnimationHashes.NormalAttack4End,
        };

        private Phase _phase;
        private int _comboIndex;
        private float _windowTimer;
        private int _playingEndHash;
        private bool _hitboxEnabled;
        private int _hitWindowIndex;
        private int _effectSpawnIndex;

        /// <inheritdoc/>
        public override void Enter(IStateContext context)
        {
            Context = context;
            _phase = Phase.Playing;
            _comboIndex = 0;
            _hitboxEnabled = false;
            _hitWindowIndex = 0;
            _effectSpawnIndex = 0;
            Context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
        }

        /// <inheritdoc/>
        public override void Exit()
        {
            Context.AttackHitbox?.Disable();
            _hitboxEnabled = false;
        }

        /// <inheritdoc/>
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

            float normalizedTime = stateInfo.normalizedTime;
            HitboxService.Update(
                normalizedTime, _comboIndex, Context.AttackConfig, Context.AttackHitbox,
                Context.Attributes, Context.CameraShakeChannel,
                ref _hitWindowIndex, ref _hitboxEnabled);
            EffectService.Update(
                normalizedTime, _comboIndex, Context.AttackConfig, Context.EffectSpawnPoint,
                ref _effectSpawnIndex);

            if (normalizedTime >= 0.9f)
            {
                Context.AttackHitbox?.Disable();
                _hitboxEnabled = false;
                _playingEndHash = EndHashes[_comboIndex];
                Context.Animator.CrossFadeInFixedTime(_playingEndHash, CrossFadeDuration);
                _phase = Phase.ComboWindow;
                _windowTimer = 0f;
            }
        }

        private void UpdateComboWindow()
        {
            _windowTimer += Time.deltaTime;

            if (Context.Blackboard.IsAttackBuffered)
            {
                Context.Blackboard.ConsumeAttack();
                AdvanceToNextCombo();
                return;
            }

            if (Context.Blackboard.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState<WalkState>();
                return;
            }

            if (_windowTimer >= Context.Blackboard.ComboWindowDuration)
            {
                _comboIndex = 0;
                _phase = Phase.Ending;
            }
        }

        private void UpdateEnding()
        {
            if (Context.Blackboard.IsAttackBuffered)
            {
                Context.Blackboard.ConsumeAttack();
                _comboIndex = 0;
                _hitboxEnabled = false;
                _hitWindowIndex = 0;
                _effectSpawnIndex = 0;
                Context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            if (Context.Blackboard.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState<WalkState>();
                return;
            }

            AnimatorStateInfo stateInfo = Context.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash != _playingEndHash) return;
            if (stateInfo.normalizedTime >= 1f)
                Context.StateMachine.ChangeState<IdleState>();
        }

        /// <inheritdoc/>
        public override void LateUpdate()
        {
            Context.MotionDriver.UpdateLockEnemyRotation(Context.Blackboard.LockTarget);
        }

        private void AdvanceToNextCombo()
        {
            _comboIndex++;
            if (_comboIndex >= AttackHashes.Length) _comboIndex = 0;
            _hitboxEnabled = false;
            _hitWindowIndex = 0;
            _effectSpawnIndex = 0;
            Context.Animator.CrossFadeInFixedTime(AttackHashes[_comboIndex], CrossFadeDuration);
            _phase = Phase.Playing;
        }
    }
}
