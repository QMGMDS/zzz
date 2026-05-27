using Core.Input;
using CustomCameras;
using GamePlay.State;
using GamePlay.StateMachine;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 玩家控制器，实现 IStateContext 为状态提供依赖，委托状态机驱动角色行为
    /// </summary>
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

        private PlayerStateMachine _playerStateMachine;
        private Camera _mainCamera;
        private Vector2 _moveDirection;
        private bool _evadeTriggered;
        private bool _attackTriggered;

        #region IStateContext

        public Animator Animator => _animator;
        public CharacterController CharacterController => _characterController;
        public Transform Transform => transform;
        public Vector2 MoveDirection => _moveDirection;
        public StateMachineBase StateMachine => _playerStateMachine;
        public Camera MainCamera => _mainCamera;
        public float InputBufferTime => _inputBufferTime;
        public float EvadeFrontCommitDuration => _evadeFrontCooldown;
        public float EvadeBackCommitDuration => _evadeBackCooldown;
        public bool IsEvadeTriggered => _evadeTriggered;

        public void ConsumeEvade()
        {
            _evadeTriggered = false;
        }

        public bool IsAttackTriggered => _attackTriggered;

        public void ConsumeAttack()
        {
            _attackTriggered = false;
        }

        public float ComboWindowDuration => _comboWindowDuration;

        public Transform LockTarget => _cameraLockEnemy != null ? _cameraLockEnemy.CurrentTarget : null;

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
            if (_cameraLockEnemy != null)
            {
                _cameraLockEnemy.ToggleLock();
            }
        }
    }
}
