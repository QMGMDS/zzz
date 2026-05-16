using Core.Input;
using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 玩家控制器，通过 IInputable 订阅输入事件驱动角色移动
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("输入控制器引用，需挂载 InputController 组件")]
        [SerializeField] private InputController _inputController;

        private IInputable Input => _inputController;

        private void OnEnable()
        {
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
        }

        private void HandleMove(Vector2 direction)
        {
            Debug.Log($"移动方向: {direction}");
        }
    }
}