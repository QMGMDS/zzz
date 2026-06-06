namespace Core.Input.Processors
{
    /// <summary>
    /// 移动意图处理器——将防抖后的输入方向直接透传至黑板。
    /// 不涉及 Consume 操作，Move 为连续值每帧自然覆盖。
    /// </summary>
    public class MoveIntentProcessor : IIntentProcessor
    {
        /// <inheritdoc/>
        public void Update(in ProcessedInputData input, IntentionBlackboard blackboard)
        {
            blackboard.MoveDirection = input.Move;
        }
    }
}
