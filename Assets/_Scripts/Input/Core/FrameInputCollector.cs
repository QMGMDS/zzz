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

            ValidateBinding(_binding.MoveAction,        nameof(_binding.MoveAction));
            ValidateBinding(_binding.AttackAction,      nameof(_binding.AttackAction));
            ValidateBinding(_binding.EvadeAction,       nameof(_binding.EvadeAction));
            ValidateBinding(_binding.SkillAction,       nameof(_binding.SkillAction));
            ValidateBinding(_binding.SwitchCharacterAction, nameof(_binding.SwitchCharacterAction));
            ValidateBinding(_binding.UltimateAction,    nameof(_binding.UltimateAction));
        }

        /// <summary>
        /// 校验单个 InputActionReference 是否已配置。
        /// </summary>
        /// <param name="actionRef">待校验的输入引用</param>
        /// <param name="fieldName">SO 中对应字段名，用于错误定位</param>
        private static void ValidateBinding(InputActionReference actionRef, string fieldName)
        {
            if (actionRef == null)
                throw new InvalidOperationException(
                    $"InputBindingSO: [{fieldName}] 未配置 InputActionReference，请检查 SO 资产 Inspector。");
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
                IsAttackPressed = WasPressedThisFrame(_binding.AttackAction),
                IsEvadePressed = WasPressedThisFrame(_binding.EvadeAction),
                IsSkillPressed = WasPressedThisFrame(_binding.SkillAction),
                IsSwitchCharacterPressed = WasPressedThisFrame(_binding.SwitchCharacterAction),
                IsUltimatePressed = WasPressedThisFrame(_binding.UltimateAction),
            };
        }

        /// <summary>
        /// 读取 Vector2 输入值；引用未配置时返回零向量。
        /// </summary>
        /// <param name="actionRef">输入引用，可能为 null（防御漏配）</param>
        private static Vector2 ReadValue(InputActionReference actionRef)
            => actionRef == null ? Vector2.zero : actionRef.action.ReadValue<Vector2>();

        /// <summary>
        /// 查询本帧是否按下；引用未配置时返回 false。
        /// </summary>
        /// <param name="actionRef">输入引用，可能为 null（防御漏配）</param>
        private static bool WasPressedThisFrame(InputActionReference actionRef)
            => actionRef != null && actionRef.action.WasPressedThisFrame();

        #endregion
    }
}