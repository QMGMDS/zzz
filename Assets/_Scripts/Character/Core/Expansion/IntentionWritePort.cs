using System;

using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 意图写入口 - 将胶水提交的控制意图写入黑板
    /// </summary>
    internal sealed class IntentionWritePort : IWriteIntention
    {
        private const CCIntention ControlIntentionMask =
            CCIntention.WantToMove |
            CCIntention.WantToAttack |
            CCIntention.WantToHoldAttack |
            CCIntention.WantToEvade |
            CCIntention.WantToTurn |
            CCIntention.WantToSwitchIn |
            CCIntention.WantToSwitchOut;

        private readonly CCRunTimeBlackboard _blackboard;

        public IntentionWritePort(CCRunTimeBlackboard blackboard)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
        }

        /// <inheritdoc />
        public void SetMoveAxis(Vector2 moveAxis)
        {
            _blackboard.WriteInput(moveAxis);
        }

        /// <inheritdoc />
        public void SetIntention(CCIntention intention, bool value)
        {
            ValidateControlIntention(intention);
            _blackboard.SetControlIntention(intention, value);
        }

        private static void ValidateControlIntention(CCIntention intention)
        {
            if (intention == CCIntention.None || (intention & ~ControlIntentionMask) != CCIntention.None)
                throw new ArgumentOutOfRangeException(nameof(intention), intention, "只能提交控制意图");
        }
    }
}
