using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using SPInput.Contract;

namespace SPInput.Core
{
    /// <summary>
    /// 帧输入采集器 - 每帧从 InputBindingConfigSO 读取玩家输入，产出帧级别输入数据
    /// </summary>
    [DefaultExecutionOrder(-400)]
    internal sealed class FrameInputCollector : MonoBehaviour, IProvideFrameInput
    {
        [Header("输入按键绑定配置")]
        [Tooltip("通过 ScriptableObject 配置的 InputActionReference 集合")]
        [SerializeField] private InputBindingConfigSO _binding;

        [Header("后处理参数配置")]
        [Tooltip("输入后处理参数 SO（长按阈值、归零缓冲等），必配，Awake 强校验")]
        [SerializeField] private ProcessedFrameConfigSO _processingConfig;

        /// <summary>当前帧的原始输入数据</summary>
        public RawFrameInput CurrentFrame { get; private set; }

        /// <summary>当前帧的后处理输入数据</summary>
        public ProcessedFrameInput CurrentProcessed { get; private set; }

        /// <summary>
        /// 采集器内部使用的帧索引
        /// </summary>
        private ulong _frameIndex;

        // 按键长按计时 - 每个 InputAction 一份持续按压累计时长，松开归零
        private readonly Dictionary<InputAction, float> _holdTimers = new();

        // 轴延时缓冲状态
        private Vector2 _lastNonZeroAxis;
        private float _releaseElapsed;

        private float HoldThreshold => _processingConfig.HoldThreshold;
        private float ReleaseBuffer => _processingConfig.ReleaseBuffer;

        #region 校验

        private void Awake()
        {
            if (_binding == null)
                throw new InvalidOperationException(
                    $"FrameInputCollector ({name}): InputBindingConfigSO 未设置，请检查 Inspector 配置");

            if (_processingConfig == null)
                throw new InvalidOperationException(
                    $"FrameInputCollector ({name}): ProcessedFrameConfigSO 未设置，请检查 Inspector 配置");

            ValidateBinding(_binding.MoveAction, nameof(_binding.MoveAction));
            ValidateBinding(_binding.AttackAction, nameof(_binding.AttackAction));
            ValidateBinding(_binding.EvadeAction, nameof(_binding.EvadeAction));
            ValidateBinding(_binding.SkillAction, nameof(_binding.SkillAction));
            ValidateBinding(_binding.SwitchCharacterAction, nameof(_binding.SwitchCharacterAction));
            ValidateBinding(_binding.UltimateAction, nameof(_binding.UltimateAction));
        }

        /// <summary>
        /// 校验单个 InputActionReference 是否已配置
        /// </summary>
        private static void ValidateBinding(InputActionReference actionRef, string fieldName)
        {
            if (actionRef == null)
                throw new InvalidOperationException(
                    $"InputBindingConfigSO: [{fieldName}] 未配置 InputActionReference，请检查 SO 资产 Inspector");
        }

        #endregion

        #region 新输入系统的激活/失活

        private void OnEnable()
        {
            ToggleAllActions(true);
        }

        private void OnDisable()
        {
            ToggleAllActions(false);
            _holdTimers.Clear();
            _lastNonZeroAxis = Vector2.zero;
            _releaseElapsed = 0f;
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

        #region 采集与后处理

        private void Update()
        {
            _frameIndex++;

            Vector2 moveAxis = ReadValue(_binding.MoveAction);

            CurrentFrame = new RawFrameInput
            {
                FrameIndex = _frameIndex,
                MoveAxisValue = moveAxis,
                IsAttackPressed = WasPressedThisFrame(_binding.AttackAction),
                IsEvadePressed = WasPressedThisFrame(_binding.EvadeAction),
                IsSkillPressed = WasPressedThisFrame(_binding.SkillAction),
                IsSwitchCharacterPressed = WasPressedThisFrame(_binding.SwitchCharacterAction),
                IsUltimatePressed = WasPressedThisFrame(_binding.UltimateAction),
            };

            CurrentProcessed = new ProcessedFrameInput
            {
                FrameIndex = _frameIndex,
                Attack = ProcessButton(_binding.AttackAction),
                Evade = ProcessButton(_binding.EvadeAction),
                Skill = ProcessButton(_binding.SkillAction),
                SwitchCharacter = ProcessButton(_binding.SwitchCharacterAction),
                Ultimate = ProcessButton(_binding.UltimateAction),
                MoveDirection = ProcessAxis(moveAxis, out bool hasMoveInput),
                HasMoveInput = hasMoveInput,
            };
        }

        /// <summary>
        /// 读取 Vector2 输入值
        /// </summary>
        private static Vector2 ReadValue(InputActionReference actionRef)
            => actionRef.action.ReadValue<Vector2>();

        /// <summary>
        /// 查询该按键本帧是否按下
        /// </summary>
        private static bool WasPressedThisFrame(InputActionReference actionRef)
            => actionRef.action.WasPressedThisFrame();

        /// <summary>
        /// 后处理 - 累计持续按压时长，超过阈值即长按，松开归零
        /// </summary>
        private ButtonInputState ProcessButton(InputActionReference actionRef)
        {
            InputAction action = actionRef.action;
            bool isPressed = action.WasPressedThisFrame();
            bool isHeld = false;

            if (action.IsPressed())
            {
                _holdTimers.TryGetValue(action, out float t);
                t += Time.deltaTime;
                _holdTimers[action] = t;
                isHeld = t > HoldThreshold;
            }
            else
            {
                _holdTimers.Remove(action);
            }

            return new ButtonInputState { IsPressed = isPressed, IsHeld = isHeld };
        }

        /// <summary>
        /// 后处理 - 延时缓冲非零方向
        /// </summary>
        private Vector2 ProcessAxis(Vector2 current, out bool hasInput)
        {
            // 轻量死区过滤 - 视近零为零，避免抖动尾刺
            const float DeadZone = 1e-4f;
            bool isZero = current.sqrMagnitude <= DeadZone * DeadZone;

            Vector2 result;
            if (!isZero)
            {
                _lastNonZeroAxis = current;
                _releaseElapsed = 0f;
                hasInput = true;
                result = current;
            }
            else if (_releaseElapsed < ReleaseBuffer && _lastNonZeroAxis.sqrMagnitude > 0f)
            {
                _releaseElapsed += Time.deltaTime;
                hasInput = true;
                result = _lastNonZeroAxis;
            }
            else
            {
                hasInput = false;
                result = Vector2.zero;
            }

            // 归一化 - 保证输出为单位方向向量；零向量不归一化以避免 NaN
            float mag = result.magnitude;
            return mag > 0f ? result / mag : result;
        }

        #endregion
    }
}
