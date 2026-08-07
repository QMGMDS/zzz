using System;
using SPInput.Contract;
using SPInput.Wiring;
using UnityEngine;
using SPCharacter.Contract;
using SPCamera.Contract;
using SPCamera.Wiring;

namespace SPCharacter.Wiring
{
    /// <summary>
    /// 玩家意图翻译机 - 从输入模块信箱 pull 后处理数据，翻译为角色意图快照。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacter/Input Translator", fileName = "PlayerInputTranslator")]
    public class InputTranslator : CharacterIntentionProviderAsset
    {
        private const float DirectionEpsilon = 1e-6f;

        [Header("接线")]
        [Tooltip("输入模块帧输入槽位 SO - 运行时信箱，据此 pull 当前后处理输入。必填，未配置抛异常拒绝运行。")]
        [SerializeField] private FrameInputProviderSO _frameInput;

        [Tooltip("相机模块坐标转换器槽位 SO - 配置后把输入平面方向转换为世界 XZ 目标方向；未配置时直通输入方向。")]
        [SerializeField] private CoordinateConverterProviderSO _coordinateConverter;

        [Header("转向意图")]
        [Tooltip("相邻有效玩家输入方向的最小转向夹角（度），严格大于此角度时产生 WantToTurn。")]
        [SerializeField, Range(0f, 180f)] private float _turnAngleThreshold = 135f;

        private Vector2 _previousInputDirection;
        private ulong _lastFrameIndex;
        private bool _hasPreviousFrame;
        private CharacterIntentionFrame _cachedFrame;

        /// <inheritdoc />
        public override CharacterIntentionFrame CurrentFrame
        {
            get
            {
                if (_frameInput == null)
                    throw new InvalidOperationException($"{nameof(InputTranslator)}: 未配置帧输入槽位 {_frameInput}。");

                IFrameInputProvider provider = _frameInput.Provider;
                if (provider == null)
                {
                    ResetHistory();
                    return default;
                }

                ProcessedFrameInput input = provider.CurrentProcessed;
                if (_hasPreviousFrame && input.FrameIndex == _lastFrameIndex)
                    return _cachedFrame;

                bool shouldTurn = ShouldTurn(input.MoveDirection);
                _previousInputDirection = input.MoveDirection;
                _hasPreviousFrame = true;
                _lastFrameIndex = input.FrameIndex;

                ICoordinateConverter coordinateConverter = _coordinateConverter == null ? null : _coordinateConverter.Provider;
                _cachedFrame = Translate(in input, coordinateConverter, shouldTurn);
                return _cachedFrame;
            }
        }

        private void OnEnable()
        {
            ResetHistory();
        }

        /// <summary>
        /// 翻译后处理输入为角色意图快照。
        /// </summary>
        /// <param name="input">输入模块的后处理帧数据</param>
        /// <param name="coordinateConverter">相机模块坐标转换器；为空时直通输入方向</param>
        /// <returns>角色单帧意图快照</returns>
        private static CharacterIntentionFrame Translate(
            in ProcessedFrameInput input,
            ICoordinateConverter coordinateConverter,
            bool shouldTurn)
        {
            CharacterIntention flags = CharacterIntention.None;

            if (input.Attack.IsPressed) flags |= CharacterIntention.WantToAttack;
            if (input.Attack.IsHeld) flags |= CharacterIntention.WantToHoldAttack;
            if (input.Evade.IsPressed) flags |= CharacterIntention.WantToEvade;
            if (input.HasMoveInput) flags |= CharacterIntention.WantToMove;
            if (shouldTurn) flags |= CharacterIntention.WantToTurn;

            Vector2 worldMoveDirection = coordinateConverter == null
                ? input.MoveDirection
                : coordinateConverter.ConvertToWorldMoveDirection(input.MoveDirection);

            return new CharacterIntentionFrame
            {
                MoveAxis = worldMoveDirection, // 这里写入的是世界 XZ 目标方向
                Flags = flags,
            };
        }

        private bool ShouldTurn(Vector2 currentInputDirection)
        {
            if (!_hasPreviousFrame ||
                currentInputDirection.sqrMagnitude <= DirectionEpsilon ||
                _previousInputDirection.sqrMagnitude <= DirectionEpsilon)
                return false;

            float clampedThreshold = Mathf.Clamp(_turnAngleThreshold, 0f, 180f);
            float turnAngleCosine = Mathf.Cos(clampedThreshold * Mathf.Deg2Rad);
            Vector2 currentDirection = currentInputDirection.normalized;
            Vector2 previousDirection = _previousInputDirection.normalized;
            return Vector2.Dot(currentDirection, previousDirection) < turnAngleCosine;
        }

        private void ResetHistory()
        {
            _previousInputDirection = Vector2.zero;
            _lastFrameIndex = 0ul;
            _hasPreviousFrame = false;
            _cachedFrame = default;
        }
    }
}
