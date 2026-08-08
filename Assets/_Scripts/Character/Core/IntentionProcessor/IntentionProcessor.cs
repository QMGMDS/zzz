using System;
using SPCharacter.Contract;

namespace SPCharacter.Core
{
    /// <summary>
    /// 意图处理器 - 对帧输入意图进行后处理并写入黑板。
    /// </summary>
    public class IntentionProcessor
    {
        private readonly CharacterRunTimeData _blackboard;

        public IntentionProcessor(CharacterRunTimeData blackboard)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
        }

        /// <summary>
        /// 对帧意图快照进行后处理并写入黑板。
        /// </summary>
        /// <param name="frame">本帧意图快照</param>
        public void Process(in CharacterIntentionFrame frame)
        {
            _blackboard.WriteInput(frame.MoveAxis);
            WriteFlag(frame.Flags, CharacterIntention.WantToMove);
            WriteFlag(frame.Flags, CharacterIntention.WantToAttack);
            WriteFlag(frame.Flags, CharacterIntention.WantToHoldAttack);
            WriteFlag(frame.Flags, CharacterIntention.WantToEvade);
            WriteFlag(frame.Flags, CharacterIntention.WantToTurn);
        }

        private void WriteFlag(CharacterIntention flags, CharacterIntention intention)
        {
            _blackboard.SetControlIntention(intention, (flags & intention) != 0);
        }
    }
}