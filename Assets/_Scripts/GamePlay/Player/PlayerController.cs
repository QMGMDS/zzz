using Core.Input;
using GamePlay.Common;
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

        private MovementStateMachine _movementStateMachine;
        private Camera _mainCamera;
        private Vector2 _moveDirection;

        #region IStateContext

        public Animator Animator => _animator;
        public CharacterController CharacterController => _characterController;
        public Transform Transform => transform;
        public Vector2 MoveDirection => _moveDirection;
        public IStateMachine StateMachine => _movementStateMachine;
        public Camera MainCamera => _mainCamera;

        #endregion

        #region Life Cycle

        private void Awake()
        {
            _mainCamera = Camera.main;
            _movementStateMachine = new MovementStateMachine();
            _movementStateMachine.Initialize<IdleState>(this);
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
            }
        }

        private void OnDisable()
        {
            if (_inputController != null)
            {
                _inputController.MoveDirectionChanged -= HandleMove;
            }

            if (_animator != null)
            {
                _animator.SetBool(AnimationHashes.HasInput, false);
                _animator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            _movementStateMachine.Update();
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
    }
}
