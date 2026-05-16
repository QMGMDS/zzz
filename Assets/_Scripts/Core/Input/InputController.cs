using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    /// <summary>
    /// 输入控制器，封装 Unity Input System，通过 IInputable 接口暴露输入事件供外部脚本订阅
    /// </summary>
    public class InputController : MonoBehaviour, IInputable, @InputSystem.ICombatMapActions
    {
        private @InputSystem _inputActions;
        private Vector2 _moveDirection;

        /// <inheritdoc cref="IInputable.MoveDirectionChanged"/>
        public event Action<Vector2> MoveDirectionChanged;

        /// <inheritdoc cref="IInputable.MoveDirection"/>
        public Vector2 MoveDirection => _moveDirection;

        private void Awake()
        {
            _inputActions = new @InputSystem();
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
    }
}

