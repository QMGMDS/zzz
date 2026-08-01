using SPInput_Contract;
using SPInput_Wiring;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 玩家输入源 - 从 FrameInputProviderSO 槽位 Pull 帧原始输入，
    /// 经死区判定后翻译为角色意图，写入黑板。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/InputSource/Player", fileName = "CCSource_PlayerSO")]
    public class CCSource_PlayerSO : CCSourceSO
    {
        private const float TurnAngleThresholdDegrees = 135f;

        [Header("输入来源")]
        [Tooltip("帧输入提供者槽位 SO，运行时通过其 Provider 拉取当前帧原始输入。")]
        [SerializeField] private FrameInputProviderSO _inputProviderSO;

        [Header("移动死区")]
        [Tooltip("移动输入长度低于此阈值时视为无移动意图")]
        [SerializeField] private float _moveDeadzone = 0.1f;

        [Tooltip("移动轴短暂归零时保留上一次有效移动输入的时间，单位为秒")]
        [Min(0f)]
        [SerializeField] private float _movementInputGraceSeconds = 0.05f;

        private Transform _movementReference;
        private float _movementInputGraceRemaining;

        /// <summary>
        /// 初始化 - 自行查找并绑定场景中的 Main Camera 作为移动方向参考。
        /// </summary>
        public void Initialize()
        {
            var cam = Camera.main;
            if (cam == null)
                throw new System.InvalidOperationException("玩家输入源初始化失败：场景中没有 Main Camera。");

            _movementReference = cam.transform;
        }

        /// <inheritdoc />
        public override void WriteIntentions(CharacterRunTimeData blackboard)
        {
            if (blackboard == null) throw new System.ArgumentNullException(nameof(blackboard));

            // 槽位未注入时静默跳过，保证删除接线后角色空转不报错。
            var provider = _inputProviderSO != null ? _inputProviderSO.Provider : null;
            if (provider == null) return;

            if (_movementReference == null)
            {
                var cam = Camera.main;
                if (cam != null)
                    _movementReference = cam.transform;
                else
                    throw new System.InvalidOperationException("未设置移动方向参考且场景中没有 Main Camera。");
            }

            FrameRawInput currentFrameInput = provider.CurrentFrame;

            ProcessMovement(currentFrameInput, blackboard);
            ProcessActions(currentFrameInput, blackboard);
        }

        #region 移动处理

        private void ProcessMovement(
            FrameRawInput currentFrameInput,
            CharacterRunTimeData blackboard)
        {
            Vector2 rawMoveInput = Vector2.ClampMagnitude(currentFrameInput.MoveAxisValue, 1f);
            bool hasCurrentMovementInput = HasMovementInput(rawMoveInput);

            if (hasCurrentMovementInput)
            {
                Vector2 cameraRelativeInput = ToCameraRelative(rawMoveInput);
                WriteTurnIntention(cameraRelativeInput, blackboard);
                WriteMovementInputAndIntentions(cameraRelativeInput, rawMoveInput.magnitude, blackboard);
                _movementInputGraceRemaining = _movementInputGraceSeconds;
                return;
            }

            WriteEmptyMovementInput(blackboard);
        }

        private Vector2 ToCameraRelative(Vector2 rawInput)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_movementReference.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 worldDir = forward * rawInput.y + right * rawInput.x;
            return new Vector2(worldDir.x, worldDir.z);
        }

        private void WriteMovementInputAndIntentions(
            Vector2 cameraRelativeInput,
            float rawMagnitude,
            CharacterRunTimeData blackboard)
        {
            float processedMagnitude = Mathf.InverseLerp(_moveDeadzone, 1f, rawMagnitude);
            Vector2 normalizedCameraRelative = cameraRelativeInput.sqrMagnitude > 0.0001f
                ? cameraRelativeInput.normalized * processedMagnitude
                : Vector2.zero;
            blackboard.WriteInput(normalizedCameraRelative);
            blackboard.SetInputIntention(CharacterIntention.WantToMove, true);
            blackboard.SetInputIntention(CharacterIntention.NotWantToMove, false);
        }

        private void WriteEmptyMovementInput(CharacterRunTimeData blackboard)
        {
            _movementInputGraceRemaining = Mathf.Max(0f, _movementInputGraceRemaining - Time.deltaTime);
            bool keepsPreviousMovement = blackboard.MoveInput != Vector2.zero &&
                                         _movementInputGraceRemaining > 0f;

            if (!keepsPreviousMovement)
                blackboard.WriteInput(Vector2.zero);

            blackboard.SetInputIntention(CharacterIntention.WantToMove, keepsPreviousMovement);
            blackboard.SetInputIntention(CharacterIntention.NotWantToMove, !keepsPreviousMovement);
            blackboard.SetInputIntention(CharacterIntention.WantToTurn, false);
        }

        private void WriteTurnIntention(
            Vector2 cameraRelativeInput,
            CharacterRunTimeData blackboard)
        {
            bool hasPreviousMovementInput = blackboard.MoveInput != Vector2.zero;
            bool wantsToTurn = hasPreviousMovementInput &&
                               Vector2.Angle(blackboard.MoveInput, cameraRelativeInput) > TurnAngleThresholdDegrees;
            blackboard.SetInputIntention(CharacterIntention.WantToTurn, wantsToTurn);
        }

        private bool HasMovementInput(Vector2 moveInput)
        {
            return moveInput.magnitude > _moveDeadzone;
        }

        #endregion

        #region 按键处理

        private void ProcessActions(FrameRawInput input, CharacterRunTimeData blackboard)
        {
            if (input.AttackPressed)
                blackboard.SetInputIntention(CharacterIntention.WantToAttack, true);

            if (input.EvadePressed)
                blackboard.SetInputIntention(CharacterIntention.WantToEvade, true);
        }

        #endregion
    }
}
