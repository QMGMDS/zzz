using Core.Event;
using Core.Input;
using GamePlay.Combat;
using CombatConfig = GamePlay.Combat.AttackComboConfigSO;
using CustomCameras;
using GamePlay.State;
using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>玩家控制器，为状态机提供依赖并驱动角色行为</summary>
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour, IStateContext
    {
        [Tooltip("输入控制器引用，需挂载 InputController 组件")]
        [SerializeField] private InputController _inputController;

        [Tooltip("角色 Animator 组件，挂载 Anbi AnimatorController")]
        [SerializeField] private Animator _animator;

        [Tooltip("角色 CharacterController 组件，用于驱动移动与碰撞")]
        [SerializeField] private CharacterController _characterController;

        [Tooltip("Root Motion 位移缩放倍率，1 为原始动画速度")]
        [SerializeField] private float _rootMotionScale = 1f;

        [Tooltip("输入缓冲时间（秒），防止方向快速切换时误判为停止")]
        [SerializeField] private float _inputBufferTime = 0.05f;

        [Tooltip("前闪避冷却时间（秒），CD 期间无法再次触发前闪避")]
        [SerializeField] private float _evadeFrontCooldown = 0.3f;

        [Tooltip("后撤步冷却时间（秒），CD 期间无法再次触发后撤步")]
        [SerializeField] private float _evadeBackCooldown = 0.7f;

        [Tooltip("连击窗口持续时间（秒），当前攻击动画结束后在该时间内再次按下攻击键可进入下一段连击")]
        [SerializeField] private float _comboWindowDuration = 0.6f;

        [Tooltip("锁敌摄像机组件引用，挂载在 Cinemachine Virtual Camera 上")]
        [SerializeField] private CameraLockEnemy _cameraLockEnemy;

        [Tooltip("锁敌切换事件通道，触发时通知 CameraLockEnemy 执行 ToggleLock")]
        [SerializeField] private VoidEventChannelSO _lockEnemyToggleChannel;

        [Tooltip("攻击碰撞体组件引用，挂载在武器骨骼子节点上")]
        [SerializeField] private AttackHitbox _attackHitbox;

        [Tooltip("连击攻击配置 SO，定义每段伤害与判定窗口")]
        [SerializeField] private CombatConfig _attackConfig;

        private PlayerStateMachine _playerStateMachine;
        private Camera _mainCamera;
        private Vector2 _moveDirection;
        private bool _evadeTriggered;
        private bool _attackTriggered;

        #region IStateContext

        /// <inheritdoc cref="IStateContext.Animator"/>
        public Animator Animator => _animator;

        /// <inheritdoc cref="IStateContext.CharacterController"/>
        public CharacterController CharacterController => _characterController;

        /// <inheritdoc cref="IStateContext.Transform"/>
        public Transform Transform => transform;

        /// <inheritdoc cref="IStateContext.MoveDirection"/>
        public Vector2 MoveDirection => _moveDirection;

        /// <inheritdoc cref="IStateContext.StateMachine"/>
        public StateMachineBase StateMachine => _playerStateMachine;

        /// <inheritdoc cref="IStateContext.MainCamera"/>
        public Camera MainCamera => _mainCamera;

        /// <inheritdoc cref="IStateContext.InputBufferTime"/>
        public float InputBufferTime => _inputBufferTime;

        /// <inheritdoc cref="IStateContext.EvadeFrontCommitDuration"/>
        public float EvadeFrontCommitDuration => _evadeFrontCooldown;

        /// <inheritdoc cref="IStateContext.EvadeBackCommitDuration"/>
        public float EvadeBackCommitDuration => _evadeBackCooldown;

        /// <inheritdoc cref="IStateContext.IsEvadeTriggered"/>
        public bool IsEvadeTriggered => _evadeTriggered;

        /// <inheritdoc cref="IStateContext.ConsumeEvade"/>
        public void ConsumeEvade()
        {
            _evadeTriggered = false;
        }

        /// <inheritdoc cref="IStateContext.IsAttackTriggered"/>
        public bool IsAttackTriggered => _attackTriggered;

        /// <inheritdoc cref="IStateContext.ConsumeAttack"/>
        public void ConsumeAttack()
        {
            _attackTriggered = false;
        }

        /// <inheritdoc cref="IStateContext.ComboWindowDuration"/>
        public float ComboWindowDuration => _comboWindowDuration;

        /// <inheritdoc cref="IStateContext.LockTarget"/>
        public Transform LockTarget => _cameraLockEnemy != null ? _cameraLockEnemy.CurrentTarget : null;

        /// <inheritdoc cref="IStateContext.AttackHitbox"/>
        public AttackHitbox AttackHitbox => _attackHitbox;

        /// <inheritdoc cref="IStateContext.AttackConfig"/>
        public CombatConfig AttackConfig => _attackConfig;

        #endregion

        #region Life Cycle

        private void Awake()
        {
            _mainCamera = Camera.main;
            _playerStateMachine = new PlayerStateMachine(_evadeFrontCooldown, _evadeBackCooldown);
            _playerStateMachine.Initialize<IdleState>(this);
        }

        private void OnEnable()
        {
            if (_animator != null)
            {
                _animator.applyRootMotion = true;
            }

            if (_inputController != null)
            {
                _inputController.MoveDirectionChanged += HandleMove;
                _inputController.EvadeTriggered += HandleEvade;
                _inputController.AttackTriggered += HandleAttack;
                _inputController.LockEnemyTriggered += HandleLockEnemy;
            }
        }

        private void OnDisable()
        {
            if (_inputController != null)
            {
                _inputController.MoveDirectionChanged -= HandleMove;
                _inputController.EvadeTriggered -= HandleEvade;
                _inputController.AttackTriggered -= HandleAttack;
                _inputController.LockEnemyTriggered -= HandleLockEnemy;
            }

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            _playerStateMachine.Update();
        }

        private void LateUpdate()
        {
            _playerStateMachine.LateUpdate();
        }

        private void OnAnimatorMove()
        {
            if (_characterController == null || _animator == null) return;

            Vector3 delta = _animator.deltaPosition * _rootMotionScale;
            _characterController.Move(delta);
        }

        #endregion

        private void HandleMove(Vector2 direction)
        {
            _moveDirection = direction;
        }

        private void HandleEvade()
        {
            _evadeTriggered = true;
        }

        private void HandleAttack()
        {
            _attackTriggered = true;
        }

        private void HandleLockEnemy()
        {
            _lockEnemyToggleChannel?.Raise();
        }
    }
}
