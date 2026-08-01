using SPInput_Contract;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SPInput_Core
{
    /// <summary>
    /// 帧输入采集器 - 每帧从 InputBindingSO 读取玩家输入，
    /// 封装为 FrameRawInput 供下游通过 IFrameInputProvider 消费。
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public class FrameInputCollector : MonoBehaviour, IFrameInputProvider
    {
        [Header("输入按键绑定配置")]
        [Tooltip("通过 ScriptableObject 配置的 InputActionReference 集合。")]
        [SerializeField] private InputBindingSO _binding;

        /// <summary>
        /// 当前帧的原始输入数据。
        /// </summary>
        public FrameRawInput CurrentFrame { get; private set; }

        /// <summary>
        /// 采集器内部使用的帧索引。
        /// </summary>
        private ulong _frameIndex;

        private void Awake()
        {
            if (_binding == null)
                throw new InvalidOperationException(
                    $"FrameInputCollector ({name}): InputBindingSO 未设置，请检查 Inspector 配置。");
        }

        #region 新输入系统的激活/失活

        private void OnEnable()
        {
            ToggleAllActions(true);
        }

        private void OnDisable()
        {
            ToggleAllActions(false);
        }

        private void ToggleAllActions(bool enable)
        {
            InputActionReference[] all = {
                _binding.MoveAction,
                _binding.AttackAction,
                _binding.EvadeAction,
                _binding.SkillAction,
                _binding.SwitchCharacterAction,
                _binding.UltimateAction,
            };

            foreach (var ar in all) ToggleAction(ar, enable);
        }

        private static void ToggleAction(InputActionReference actionRef, bool enable)
        {
            if (actionRef == null) return;
            if (enable) actionRef.action.Enable();
            else actionRef.action.Disable();
        }

        #endregion

        #region 采集数据

        private void Update()
        {
            _frameIndex++;

            CurrentFrame = new FrameRawInput
            {
                FrameIndex = _frameIndex,
                MoveAxisValue = ReadValue(_binding.MoveAction),
                AttackPressed = WasPressedThisFrame(_binding.AttackAction),
                EvadePressed = WasPressedThisFrame(_binding.EvadeAction),
                SkillPressed = WasPressedThisFrame(_binding.SkillAction),
                SwitchCharacterPressed = WasPressedThisFrame(_binding.SwitchCharacterAction),
                UltimatePressed = WasPressedThisFrame(_binding.UltimateAction),
            };
        }

        private static Vector2 ReadValue(InputActionReference actionRef)
        {
            return actionRef.action.ReadValue<Vector2>();
        }

        private static bool WasPressedThisFrame(InputActionReference actionRef)
        {
            return actionRef.action.WasPressedThisFrame();
        }

        #endregion
    }
}
