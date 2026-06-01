using Core.Pool;
using GamePlay.Common;
using GamePlay.Combat;
using GamePlay.Effects;
using CombatConfig = GamePlay.Combat.AttackComboConfigSO;
using UnityEngine;

namespace GamePlay.State
{
    /// <summary>
    /// 四段普攻连击状态，内部通过三阶段（Playing / ComboWindow / Ending）管理全部连击逻辑。
    /// 动画结束后开启连击窗口：窗口内收到攻击输入则进入下一段，超时则播放对应 _End 动画。
    /// 四段连击后输入则回到第一段继续。
    /// Hitbox 启用时机由 SO 的 HitboxActiveStart/End 控制（归一化时间）。
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
        private const float RotationSmoothTime = 0.1f;

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
        private float _rotationVelocity;
        private bool _hitboxEnabled;
        private int _effectSpawnIndex;

        /// <inheritdoc/>
        public override void Enter(IStateContext context)
        {
            Context = context;
            _phase = Phase.Playing;
            _comboIndex = 0;
            _hitboxEnabled = false;
            _effectSpawnIndex = 0;
            ApplySegmentConfig(0);
            Context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
        }

        /// <inheritdoc/>
        public override void Exit()
        {
            DisableHitbox();
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
            UpdateHitboxForCurrentSegment(normalizedTime);
            UpdateEffectForCurrentSegment(normalizedTime);

            if (normalizedTime >= 0.9f)
            {
                DisableHitbox();
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
                _hitboxEnabled = false;
                _effectSpawnIndex = 0;
                ApplySegmentConfig(_comboIndex);
                Context.Animator.CrossFadeInFixedTime(AttackHashes[_comboIndex], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            if (Context.MoveDirection.sqrMagnitude > 0.0001f)
            {
                Context.StateMachine.ChangeState<WalkState>();
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
                _hitboxEnabled = false;
                _effectSpawnIndex = 0;
                ApplySegmentConfig(0);
                Context.Animator.CrossFadeInFixedTime(AttackHashes[0], CrossFadeDuration);
                _phase = Phase.Playing;
                return;
            }

            if (Context.MoveDirection.sqrMagnitude > 0.0001f)
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
            Transform lockTarget = Context.LockTarget;
            if (lockTarget == null) return;

            Vector3 dirToEnemy = lockTarget.position - Context.Transform.position;
            dirToEnemy.y = 0f;
            if (dirToEnemy.sqrMagnitude <= 0.0001f) return;

            float targetAngle = Mathf.Atan2(dirToEnemy.x, dirToEnemy.z) * Mathf.Rad2Deg;
            Transform t = Context.Transform;
            float smoothedAngle = Mathf.SmoothDampAngle(
                t.eulerAngles.y,
                targetAngle,
                ref _rotationVelocity,
                RotationSmoothTime
            );
            t.eulerAngles = new Vector3(0f, smoothedAngle, 0f);
        }

        private void ApplySegmentConfig(int index)
        {
            CombatConfig config = Context.AttackConfig;
            if (config == null || config.Segments == null || index >= config.Segments.Length) return;

            AttackSegmentConfig seg = config.Segments[index];
            Context.AttackHitbox?.SetDamage(seg.Damage, seg.KnockbackForce);
        }

        private void UpdateHitboxForCurrentSegment(float normalizedTime)
        {
            CombatConfig config = Context.AttackConfig;
            AttackHitbox hitbox = Context.AttackHitbox;
            if (config == null || config.Segments == null || hitbox == null) return;
            if (_comboIndex >= config.Segments.Length) return;

            AttackSegmentConfig seg = config.Segments[_comboIndex];
            bool shouldEnable = normalizedTime >= seg.HitboxActiveStart
                             && normalizedTime < seg.HitboxActiveEnd;

            if (shouldEnable && !_hitboxEnabled)
            {
                hitbox.Enable();
                _hitboxEnabled = true;
            }
            else if (!shouldEnable && _hitboxEnabled)
            {
                DisableHitbox();
            }
        }

        private void DisableHitbox()
        {
            Context.AttackHitbox?.Disable();
            _hitboxEnabled = false;
        }

        private void UpdateEffectForCurrentSegment(float normalizedTime)
        {
            CombatConfig config = Context.AttackConfig;
            if (config == null || config.Segments == null || _comboIndex >= config.Segments.Length) return;

            AttackSegmentConfig seg = config.Segments[_comboIndex];
            EffectSpawnInfo[] spawns = seg.EffectSpawns;
            if (spawns == null || spawns.Length == 0) return;

            Transform spawnPoint = Context.EffectSpawnPoint;
            if (spawnPoint == null) return;

            while (_effectSpawnIndex < spawns.Length && normalizedTime >= spawns[_effectSpawnIndex].NormalizedTime)
            {
                EffectSpawnInfo info = spawns[_effectSpawnIndex];
                SlashEffect effect = PoolManager.Instance.Get<SlashEffect>("FX_Slash");
                if (effect != null)
                {
                    effect.transform.SetPositionAndRotation(
                        spawnPoint.TransformPoint(info.LocalPosition),
                        spawnPoint.rotation * Quaternion.Euler(info.LocalRotation)
                    );
                }
                _effectSpawnIndex++;
            }
        }
    }
}
