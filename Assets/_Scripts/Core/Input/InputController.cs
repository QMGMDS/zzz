using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    /// <summary>输入控制器，封装 Unity Input System 并暴露输入事件</summary>
    public class InputController : MonoBehaviour, IInputSource, @InputSystem.ICombatMapActions
    {
        private @InputSystem _inputActions;
        private Vector2 _moveDirection;

        /// <inheritdoc cref="IInputSource.MoveDirectionChanged"/>
        public event Action<Vector2> MoveDirectionChanged;

        /// <inheritdoc cref="IInputSource.EvadeTriggered"/>
        public event Action EvadeTriggered;

        /// <inheritdoc cref="IInputSource.AttackTriggered"/>
        public event Action AttackTriggered;

        /// <inheritdoc cref="IInputSource.LockEnemyTriggered"/>
        public event Action LockEnemyTriggered;

        /// <inheritdoc cref="IInputSource.MoveDirection"/>
        public Vector2 MoveDirection => _moveDirection;

        #region Life Cycle

        private void Awake()
        {
            _inputActions = new @InputSystem();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnEnable()
        {
            _inputActions.CombatMap.AddCallbacks(this);
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.CombatMap.RemoveCallbacks(this);
            _inputActions.Disable();
            SetDirection(Vector2.zero);
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        #endregion

        #region ICombatMapActions

        void @InputSystem.ICombatMapActions.OnMove(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>();
            if (direction != _moveDirection)
            {
                SetDirection(direction);
            }
        }

        private void SetDirection(Vector2 direction)
        {
            _moveDirection = direction;
            MoveDirectionChanged?.Invoke(_moveDirection);
        }

        /// <summary>摄像机视角输入处理（由 Cinemachine POV 接管，无需额外逻辑）</summary>
        /// <param name="context">输入回调上下文</param>
        public void OnCameraLook(InputAction.CallbackContext context)
        {
        }

        /// <summary>闪避输入处理，performed 时触发闪避事件</summary>
        /// <param name="context">输入回调上下文</param>
        public void OnEvade(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                EvadeTriggered?.Invoke();
            }
        }

        /// <summary>攻击输入处理，performed 时触发攻击事件</summary>
        /// <param name="context">输入回调上下文</param>
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                AttackTriggered?.Invoke();
            }
        }

        /// <summary>锁敌输入处理，performed 时触发锁敌事件</summary>
        /// <param name="context">输入回调上下文</param>
        public void OnLockEnemy(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                LockEnemyTriggered?.Invoke();
            }
        }

        #endregion
    }
}

