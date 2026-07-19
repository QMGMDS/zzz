using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 玩家输入源 - 从 SPPlayerInputCenter 单例读取帧原始输入，
    /// 经死区判定后翻译为角色意图，写入黑板。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/InputSource/Player", fileName = "CCSource_PlayerInputSO")]
    public class CCSource_PlayerInputSO : CCSourceSO
    {
        private const float TurnAngleThresholdDegrees = 135f;

        [Header("移动死区")]
        [Tooltip("移动输入长度低于此阈值时视为无移动意图")]
        [SerializeField] private float _moveDeadzone = 0.1f;

        [Tooltip("移动轴短暂归零时保留上一次有效移动输入的时间，单位为秒")]
        [Min(0f)]
        [SerializeField] private float _movementInputGraceSeconds = 0.05f;

        private float _movementInputGraceRemaining;

        /// <inheritdoc />
        public override void WriteIntentions(CharacterRunTimeData blackboard)
        {
            if (blackboard == null) throw new System.ArgumentNullException(nameof(blackboard));
            if (SPPlayerInput.SPPlayerInputCenter.Instance == null)
                throw new System.InvalidOperationException("场景中没有可用的玩家输入中心。");

            SPPlayerInput.SPPlayerInputCenter inputCenter = SPPlayerInput.SPPlayerInputCenter.Instance;
            SPPlayerInput.FrameRawInputData currentFrameInput = inputCenter.CurrentFrameInput;

            ProcessMovement(currentFrameInput, blackboard);
            ProcessActions(currentFrameInput, blackboard);
        }

        #region 移动处理

        private void ProcessMovement(
            SPPlayerInput.FrameRawInputData currentFrameInput,
            CharacterRunTimeData blackboard)
        {
            Vector2 currentMoveInput = Vector2.ClampMagnitude(currentFrameInput.MoveAxisValue, 1f);
            bool hasCurrentMovementInput = HasMovementInput(currentMoveInput);

            if (hasCurrentMovementInput)
            {
                WriteTurnIntention(currentMoveInput, blackboard);
                WriteMovementInputAndIntentions(currentMoveInput, blackboard);
                _movementInputGraceRemaining = _movementInputGraceSeconds;
                return;
            }

            WriteEmptyMovementInput(blackboard);
        }

        private void WriteMovementInputAndIntentions(
            Vector2 moveInput,
            CharacterRunTimeData blackboard)
        {
            float processedMagnitude = Mathf.InverseLerp(_moveDeadzone, 1f, moveInput.magnitude);
            blackboard.WriteInput(moveInput.normalized * processedMagnitude, processedMagnitude);
            blackboard.SetInputIntention(CharacterIntention.WantToMove, true);
            blackboard.SetInputIntention(CharacterIntention.NotWantToMove, false);
        }

        private void WriteEmptyMovementInput(CharacterRunTimeData blackboard)
        {
            _movementInputGraceRemaining = Mathf.Max(0f, _movementInputGraceRemaining - Time.deltaTime);
            bool keepsPreviousMovement = blackboard.MoveInputMagnitude > 0f &&
                                         _movementInputGraceRemaining > 0f;

            if (!keepsPreviousMovement)
                blackboard.WriteInput(Vector2.zero, 0f);

            blackboard.SetInputIntention(CharacterIntention.WantToMove, keepsPreviousMovement);
            blackboard.SetInputIntention(CharacterIntention.NotWantToMove, !keepsPreviousMovement);
            blackboard.SetInputIntention(CharacterIntention.WantToTurn, false);
        }

        private void WriteTurnIntention(
            Vector2 currentMoveInput,
            CharacterRunTimeData blackboard)
        {
            bool hasPreviousMovementInput = blackboard.MoveInputMagnitude > 0f;
            bool wantsToTurn = hasPreviousMovementInput &&
                               Vector2.Angle(blackboard.MoveInput, currentMoveInput) > TurnAngleThresholdDegrees;
            blackboard.SetInputIntention(CharacterIntention.WantToTurn, wantsToTurn);
        }

        private bool HasMovementInput(Vector2 moveInput)
        {
            return moveInput.magnitude > _moveDeadzone;
        }

        #endregion

        #region 按键处理

        private void ProcessActions(SPPlayerInput.FrameRawInputData input, CharacterRunTimeData blackboard)
        {
            if (input.AttackPressed)
                blackboard.SetInputIntention(CharacterIntention.WantToAttack, true);

            if (input.EvadePressed)
                blackboard.SetInputIntention(CharacterIntention.WantToEvade, true);
        }

        #endregion
    }
}
