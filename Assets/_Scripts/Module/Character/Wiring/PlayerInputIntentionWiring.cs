using UnityEngine;

using SPCamera.Contract;
using SPCharacter.Core;
using SPFramework.Service;
using SPInput.Contract;

namespace SPCharacter.Wiring
{
    /// <summary>
    /// 玩家输入意图胶水 - 将玩家后处理输入提交为角色意图
    /// </summary>
    internal sealed class PlayerInputIntentionWiring : MonoBehaviour, ICCWiringExtension
    {
        /// <inheritdoc />
        public void UpdateWiring(CCWiringContext context, IWriteIntention writer)
        {
            IProvideFrameInput provider = ModuleServiceHub.Get<IProvideFrameInput>();

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
            IConvertCameraTransform converter = ModuleServiceHub.Get<IConvertCameraTransform>();

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
