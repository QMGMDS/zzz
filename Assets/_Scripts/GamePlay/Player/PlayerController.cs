using GamePlay.Attribute;
using GamePlay.Combat;
using CombatConfig = GamePlay.Combat.AttackComboConfigSO;
using GamePlay.State;
using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>玩家控制器，为状态机提供依赖并驱动角色行为</summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IStateContext, IDamageable
    {
        [Tooltip("角色 Animator 组件，挂载 Anbi AnimatorController")]
        [SerializeField] private Animator _animator;

        [Tooltip("角色 CharacterController 组件，用于驱动移动与碰撞")]
        [SerializeField] private CharacterController _characterController;

        [Tooltip("Root Motion 位移缩放倍率，1 为原始动画速度")]
        [SerializeField] private float _rootMotionScale = 1f;

        [Tooltip("前闪避冷却时间（秒），CD 期间无法再次触发前闪避")]
        [SerializeField] private float _evadeFrontCooldown = 0.3f;

        [Tooltip("后撤步冷却时间（秒），CD 期间无法再次触发后撤步")]
        [SerializeField] private float _evadeBackCooldown = 0.7f;

        [Tooltip("连击窗口持续时间（秒），当前攻击动画结束后在该时间内再次按下攻击键可进入下一段连击")]
        [SerializeField] private float _comboWindowDuration = 0.6f;

        [Tooltip("攻击碰撞体组件引用，挂载在武器骨骼子节点上")]
        [SerializeField] private AttackHitbox _attackHitbox;

        [Tooltip("连击攻击配置 SO，定义每段伤害与判定窗口")]
        [SerializeField] private CombatConfig _attackConfig;

        [Tooltip("特效生成挂点 Transform，建议放在武器骨骼子节点，位置和方向决定挥砍特效的基准")]
        [SerializeField] private Transform _effectSpawnPoint;

        [Tooltip("震屏事件通道，NormalAttackState 发出抖动力度时 CameraShakeHandler 会自动响应")]
        [SerializeField] private FloatEventChannelSO _cameraShakeChannel;

        [Tooltip("角色初始属性配置 SO")]
        [SerializeField] private CharacterAttributeSO _attributeConfig;

        [Tooltip("受击无敌帧持续时间（秒），受击后该时间内免疫伤害")]
        [SerializeField] private float _invincibleDuration = 0.5f;

        private CharacterAttributes _attributes;
        private PlayerStateMachine _playerStateMachine;
        private MotionDriver _motionDriver;
        private Camera _mainCamera;

        private float _currentHealth;
        private float _invincibleTimer;

        #region IStateContext

        /// <inheritdoc cref="IStateContext.Animator"/>
        public Animator Animator => _animator;

        /// <inheritdoc cref="IStateContext.CharacterController"/>
        public CharacterController CharacterController => _characterController;

        /// <inheritdoc cref="IStateContext.Transform"/>
        public Transform Transform => transform;

        /// <inheritdoc cref="IStateContext.StateMachine"/>
        public StateMachineBase StateMachine => _playerStateMachine;

        /// <inheritdoc cref="IStateContext.MainCamera"/>
        public Camera MainCamera => _mainCamera;

        /// <inheritdoc cref="IStateContext.EvadeFrontCommitDuration"/>
        public float EvadeFrontCommitDuration => _evadeFrontCooldown;

        /// <inheritdoc cref="IStateContext.EvadeBackCommitDuration"/>
        public float EvadeBackCommitDuration => _evadeBackCooldown;

        /// <inheritdoc cref="IStateContext.AttackHitbox"/>
        public AttackHitbox AttackHitbox => _attackHitbox;

        /// <inheritdoc cref="IStateContext.AttackConfig"/>
        public CombatConfig AttackConfig => _attackConfig;

        /// <inheritdoc cref="IStateContext.EffectSpawnPoint"/>
        public Transform EffectSpawnPoint => _effectSpawnPoint;

        /// <inheritdoc cref="IStateContext.CameraShakeChannel"/>
        public FloatEventChannelSO CameraShakeChannel => _cameraShakeChannel;

        /// <summary>角色属性只读接口，供战斗系统、状态机等外部模块读取</summary>
        public IAttributeProvider Attributes => _attributes;

        /// <inheritdoc cref="IStateContext.MotionDriver"/>
        public MotionDriver MotionDriver => _motionDriver;

        #endregion

        #region IDamageable

        /// <inheritdoc cref="IDamageable.TakeDamage"/>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_invincibleTimer > 0f) return;

            _currentHealth -= damageInfo.Amount;
            _currentHealth = Mathf.Max(0f, _currentHealth);

            _invincibleTimer = _invincibleDuration;

            if (_playerStateMachine.CurrentStateType == typeof(HitState))
                _playerStateMachine.ReenterState<HitState>();
            else
                _playerStateMachine.ChangeState<HitState>();
        }

        #endregion

        #region Life Cycle

        private void Awake()
        {
            _mainCamera = Camera.main;
            _attributes = new CharacterAttributes(_attributeConfig);
            _currentHealth = _attributes.GetAttribute(AttributeType.MaxHealth);

            _motionDriver = new MotionDriver();
            _motionDriver.Initialize(transform, _mainCamera);

            _playerStateMachine = new PlayerStateMachine(_evadeFrontCooldown, _evadeBackCooldown);
            _playerStateMachine.Initialize<IdleState>(this);
        }

        private void OnEnable()
        {
            if (_animator != null)
            {
                _animator.applyRootMotion = true;
            }
        }

        private void OnDisable()
        {
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            if (_invincibleTimer > 0f)
                _invincibleTimer -= Time.deltaTime;

            _playerStateMachine.Update();
        }

        private void LateUpdate()
        {
            _playerStateMachine.LateUpdate();
        }

        private void OnAnimatorMove()
        {
            _motionDriver.ApplyRootMotion(_characterController, _animator, _rootMotionScale);
        }

        #endregion
    }
}
