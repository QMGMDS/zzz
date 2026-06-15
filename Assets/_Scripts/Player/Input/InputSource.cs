using UnityEngine;
using UnityEngine.InputSystem;

namespace SPPlayer
{
    /// <summary>
    /// 玩家输入源——通过 InputActionReference 从 New Input System 采样原始输入数据。
    /// 只采集最原始的输入数据 RawInputData，不包含任何后处理
    /// </summary>
    public class InputSource : MonoBehaviour
    {
        #region InputActionReference

        [Header("Input Action References")]
        [Tooltip("移动输入动作（WASD / 左摇杆）")]
        [SerializeField] private InputActionReference _moveAction;

        [Tooltip("攻击输入动作（鼠标左键）")]
        [SerializeField] private InputActionReference _attackAction;

        [Tooltip("闪避/冲刺输入动作（鼠标右键 / Shift）——按下边沿触发闪避，按住+移动触发冲刺")]
        [SerializeField] private InputActionReference _evadeAction;

        #endregion

        /// <summary>
        /// 采集原始输入数据，返还 rawData 给外部
        /// 由采集员调用
        /// </summary>
        /// <param name="rawData">采集员传入的原始输入引用</param>
        public void FetchRawInput(ref RawInputData rawData)
        {
            rawData.MoveAxis = _moveAction != null ? _moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            rawData.AttackJustPressed = _attackAction != null && _attackAction.action.WasPressedThisFrame();
            rawData.EvadeJustPressed = _evadeAction != null && _evadeAction.action.WasPressedThisFrame();
        }

        #region New Input System 的激活失活

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
