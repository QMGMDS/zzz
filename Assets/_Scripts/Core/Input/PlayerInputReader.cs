using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Input
{
    /// <summary>
    /// 玩家输入阅读器——通过 InputActionReference 从 New Input System 采样原始输入数据。
    /// 继承 MonoBehaviour 以便在 Inspector 中拖拽配置 InputAction 引用与计时参数。
    /// </summary>
    public class PlayerInputReader : MonoBehaviour, IInputSource
    {
        [Header("Input Timing Settings")]
        [Tooltip("移动轴防抖缓存时间（秒），松开按键后在此窗口内保持最后一次有效值")]
        [SerializeField] private float _inputFlickerBuffer = 0.05f;

        [Tooltip("攻击按键缓存时间（秒），按下后在此时间窗口内视为攻击意图有效")]
        [SerializeField] private float _attackBufferTime = 0.2f;

        [Tooltip("闪避按键缓存时间（秒），按下后在此时间窗口内视为闪避意图有效")]
        [SerializeField] private float _evadeBufferTime = 0.2f;

        [Header("Input Action References")]
        [Tooltip("移动输入动作（WASD / 左摇杆）")]
        [SerializeField] private InputActionReference _moveAction;

        [Tooltip("攻击输入动作（鼠标左键）")]
        [SerializeField] private InputActionReference _attackAction;

        [Tooltip("闪避输入动作（鼠标右键 / Shift）")]
        [SerializeField] private InputActionReference _evadeAction;

        public float InputFlickerBuffer => _inputFlickerBuffer;
        public float AttackBufferTime => _attackBufferTime;
        public float EvadeBufferTime => _evadeBufferTime;

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
