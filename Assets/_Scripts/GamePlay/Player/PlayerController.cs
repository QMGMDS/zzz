using Core.Input;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 玩家控制器，通过 IInputable 订阅输入事件驱动角色移动与动画状态机
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        private static readonly int HasInputId = Animator.StringToHash("HasInput");

        [Tooltip("输入控制器引用，需挂载 InputController 组件")]
        [SerializeField] private InputController _inputController;

        [Tooltip("角色 Animator 组件，挂载 Anbi AnimatorController")]
        [SerializeField] private Animator _animator;

        [Tooltip("角色 CharacterController 组件，用于驱动移动与碰撞")]
        [SerializeField] private CharacterController _characterController;

        [Tooltip("Root Motion 位移缩放倍率，1 为原始动画速度")]
        [SerializeField] private float _rootMotionScale = 1f;

        private IInputable Input => _inputController;

        #region Life Cycle

        private void OnEnable()
        {
            if (_animator != null)
            {
                _animator.applyRootMotion = true;
            }

            if (Input != null)
            {
                Input.MoveDirectionChanged += HandleMove;
            }
        }

        private void OnDisable()
        {
            if (Input != null)
            {
                Input.MoveDirectionChanged -= HandleMove;
            }

            if (_animator != null)
            {
                _animator.SetBool(HasInputId, false);
                _animator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            if (Input == null) return;

            Vector2 direction = Input.MoveDirection;
            float magnitude = direction.magnitude;

            if (magnitude > 0.01f)
            {
                Vector3 lookDirection = new Vector3(direction.x, 0f, direction.y);
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
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
            float magnitude = direction.magnitude;
            bool hasInput = magnitude > 0.01f;

            if (_animator != null)
            {
                _animator.SetBool(HasInputId, hasInput);
            }
        }
    }
}