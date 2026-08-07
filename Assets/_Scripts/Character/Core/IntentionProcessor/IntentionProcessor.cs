using System;
using SPCharacter.Contract;

namespace SPCharacter.Core
{
    /// <summary>
    /// 意图处理器 - 对帧输入意图进行后处理并写入黑板。
    /// 意图合成等后处理逻辑在此扩展。
    /// </summary>
    public class IntentionProcessor
    {
        private readonly CharacterRunTimeData _blackboard;

        public IntentionProcessor(CharacterRunTimeData blackboard)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
        }

        /// <summary>
        /// 对帧意图快照进行后处理，结果写入黑板。
        /// </summary>
        /// <param name="frame">本帧意图快照</param>
        public void Process(in CharacterIntentionFrame frame)
        {
            _blackboard.WriteInput(frame.MoveAxis);
            WriteFlags(frame.Flags);
        }

        private void WriteFlags(CharacterIntention flags)
        {
            if ((flags & CharacterIntention.WantToMove) != 0) _blackboard.SetControlIntention(CharacterIntention.WantToMove, true);
            else _blackboard.SetControlIntention(CharacterIntention.WantToMove, false);

            if ((flags & CharacterIntention.WantToAttack) != 0) _blackboard.SetControlIntention(CharacterIntention.WantToAttack, true);
            else _blackboard.SetControlIntention(CharacterIntention.WantToAttack, false);

            if ((flags & CharacterIntention.WantToHoldAttack) != 0) _blackboard.SetControlIntention(CharacterIntention.WantToHoldAttack, true);
            else _blackboard.SetControlIntention(CharacterIntention.WantToHoldAttack, false);

            if ((flags & CharacterIntention.WantToEvade) != 0) _blackboard.SetControlIntention(CharacterIntention.WantToEvade, true);
            else _blackboard.SetControlIntention(CharacterIntention.WantToEvade, false);

            if ((flags & CharacterIntention.WantToTurn) != 0) _blackboard.SetControlIntention(CharacterIntention.WantToTurn, true);
            else _blackboard.SetControlIntention(CharacterIntention.WantToTurn, false);
        }
    }
}