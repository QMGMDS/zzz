using UnityEngine;
using UnityEngine.InputSystem;

namespace SPPlayerInput
{
    /// <summary>
    /// 玩家输入中心 - 每帧从 PlayerInputBinding 读取玩家输入，
    /// 将原始数据封装为 FrameRawInputData 供下游系统消费。
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public class SPPlayerInputCenter : MonoBehaviour
    {
        [Header("输入按键绑定配置")]
        [Tooltip("通过 ScriptableObject 配置的 InputActionReference 集合。")]
        [SerializeField] private PlayerInputBindingSO _binding;

        /// <summary>
        /// 当前帧的原始输入数据。
        /// </summary>
        public FrameRawInputData CurrentFrameInput { get; private set; }

        /// <summary>
        /// 上一帧的原始输入数据。
        /// </summary>
        public FrameRawInputData PreviousFrameInput { get; private set; }

        private ulong _frameIndex;

        #region 单例

        public static SPPlayerInputCenter Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError(
                    $"SPPlayerInputCenter: 场景中已存在另一个实例 '{Instance.name}'，" +
                    $"当前 '{name}' 将被禁用。请确保场景中只有一个 SPPlayerInputCenter");
                enabled = false;
                return;
            }
            Instance = this;

            // 配置检查
            if (_binding == null) Debug.LogError($"SPPlayerInputCenter ({name}): PlayerInputBindingSO 未设置。");
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #endregion

        #region New Input System 的激活失活

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
            if (_binding == null) return;

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
            if (_binding == null) return;

            _frameIndex++;

            PreviousFrameInput = CurrentFrameInput;

            CurrentFrameInput = new FrameRawInputData
            {
                FrameIndex = _frameIndex,
                MoveAxisValue = ReadValueSafe(_binding.MoveAction),
                AttackPressed = WasPressedThisFrameSafe(_binding.AttackAction),
                EvadePressed = WasPressedThisFrameSafe(_binding.EvadeAction),
                SkillPressed = WasPressedThisFrameSafe(_binding.SkillAction),
                SwitchCharacterPressed = WasPressedThisFrameSafe(_binding.SwitchCharacterAction),
                UltimatePressed = WasPressedThisFrameSafe(_binding.UltimateAction),
            };
        }

        private static Vector2 ReadValueSafe(InputActionReference actionRef)
        {
            return actionRef != null ? actionRef.action.ReadValue<Vector2>() : Vector2.zero;
        }

        private static bool WasPressedThisFrameSafe(InputActionReference actionRef)
        {
            return actionRef != null && actionRef.action.WasPressedThisFrame();
        }

        #endregion
    }
}
