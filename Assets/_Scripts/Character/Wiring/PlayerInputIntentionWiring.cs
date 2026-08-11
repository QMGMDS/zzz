using UnityEngine;

using SPCamera.Contract;
using SPCamera.Wiring;
using SPCharacter.Core;
using SPInput.Contract;
using SPInput.Wiring;

namespace SPCharacter.Wiring
{
    /// <summary>
    /// 玩家输入意图胶水 - 将玩家后处理输入提交为角色意图
    /// </summary>
    internal sealed class PlayerInputIntentionWiring : MonoBehaviour, ICCWiringExtension
    {
        [Header("输入")]
        [SerializeField, Tooltip("帧输入提供者槽位 SO")]
        private FrameInputProviderSO _inputProviderSO;

        [Header("摄像机")]
        [SerializeField, Tooltip("摄像机坐标转换提供者槽位 SO")]
        private CameraTransformProviderSO _cameraTransformProviderSO;

        /// <inheritdoc />
        public void UpdateWiring(CCWiringContext context, IWriteIntention writer)
        {
            if (_inputProviderSO == null)
                return;

            IProvideFrameInput provider = _inputProviderSO.Provider;
            if (provider == null)
                return;

            ProcessedFrameInput input = provider.CurrentProcessed;
            Vector2 moveAxis = input.HasMoveInput
                ? ConvertMoveDirection(input.MoveDirection)
                : Vector2.zero;

            writer.SetMoveAxis(moveAxis);
            CommitIf(writer, CCIntention.WantToMove, input.HasMoveInput);
            CommitIf(writer, CCIntention.WantToAttack, input.Attack.IsPressed);
            CommitIf(writer, CCIntention.WantToHoldAttack, input.Attack.IsHeld);
            CommitIf(writer, CCIntention.WantToEvade, input.Evade.IsPressed);
        }

        private Vector2 ConvertMoveDirection(Vector2 inputDirection)
        {
            IConvertCameraTransform converter = _cameraTransformProviderSO == null
                ? null
                : _cameraTransformProviderSO.Provider;

            return converter == null
                ? inputDirection
                : converter.ConvertCameraTransform(inputDirection);
        }

        private static void CommitIf(IWriteIntention writer, CCIntention intention, bool value)
        {
            if (value)
                writer.SetIntention(intention, true);
        }
    }
}
