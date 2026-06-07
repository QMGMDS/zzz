using Core.Input.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    /// <summary>
    /// 玩家输入阅读器——通过 InputActionReference 从 New Input System 采样原始输入数据。
    /// 不含后处理参数；防抖与按键缓存由 InputCollector 通过 InputPostProcessConfig 处理。
    /// </summary>
    public class PlayerInputReader : MonoBehaviour, IInputSource
    {
        [Header("Input Action References")]
        [Tooltip("移动输入动作（WASD / 左摇杆）")]
        [SerializeField] private InputActionReference _moveAction;

        [Tooltip("攻击输入动作（鼠标左键）")]
        [SerializeField] private InputActionReference _attackAction;

        [Tooltip("闪避输入动作（鼠标右键 / Shift）")]
        [SerializeField] private InputActionReference _evadeAction;

        #region IInputSource

        /// <inheritdoc/>
        public void FetchRawInput(ref RawInputData rawData)
        {
            rawData.MoveAxis = _moveAction != null ? _moveAction.action.ReadValue<Vector2>() : Vector2.zero;

            rawData.AttackJustPressed = _attackAction != null && _attackAction.action.WasPressedThisFrame();

            rawData.EvadeJustPressed = _evadeAction != null && _evadeAction.action.WasPressedThisFrame();
        }

        #endregion

        #region Life Cycle

        private void OnEnable()
        {
            ToggleActions(true);
        }

        private void OnDisable()
        {
            ToggleActions(false);
        }

        private void ToggleActions(bool enable)
        {
            InputActionReference[] all = { _moveAction, _attackAction, _evadeAction };
            foreach (var ar in all)
            {
                if (ar == null) continue;
                if (enable) ar.action.Enable();
                else ar.action.Disable();
            }
        }

        #endregion
    }
}
