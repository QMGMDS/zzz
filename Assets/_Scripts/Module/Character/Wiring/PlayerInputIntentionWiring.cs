using UnityEngine;

using SPCamera.Contract;
using SPCharacter.Core;
using SPFramework.Service;
using SPInput.Contract;

namespace SPCharacter.Wiring
{
    /// <summary>
    /// 玩家输入意图接线胶水 - 将玩家后处理输入提交为角色意图
    /// </summary>
    internal sealed class PlayerInputIntentionWiring : MonoBehaviour, ICCWiringExtension, ICharacterInputGate
    {
        private bool _isOperationLocked;

        /// <inheritdoc />
        public void UpdateWiring(CCWiringContext context, IWriteIntention writer)
        {
            if (_isOperationLocked)
                return;

            // 输入服务未注册时本帧放弃提交意图
            if (!ModuleServiceHub.TryGet<IProvideFrameInput>(out IProvideFrameInput provider))
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

        /// <inheritdoc />
        public void SetOperationLocked(bool isLocked)
        {
            _isOperationLocked = isLocked;
        }

        private Vector2 ConvertMoveDirection(Vector2 inputDirection)
        {
            // 相机服务未注册时降级为原始输入方向
            return ModuleServiceHub.TryGet<IConvertCameraTransform>(out IConvertCameraTransform converter)
                ? converter.ConvertCameraTransform(inputDirection)
                : inputDirection;
        }

        private static void CommitIf(IWriteIntention writer, CCIntention intention, bool value)
        {
            if (value)
                writer.SetIntention(intention, true);
        }
    }
}
