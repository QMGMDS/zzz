namespace SPPlayer
{
    /// <summary>
    /// 移动输入意图翻译器
    /// </summary>
    public class MoveInputProcessor : IInputProcessor
    {
        /// <summary>
        /// 将处理后的输入翻译为移动意图，写入黑板
        /// </summary>
        /// <param name="current">当前帧处理后的输入数据</param>
        /// <param name="last">上一帧处理后的输入数据</param>
        /// <param name="blackboard">玩家大脑黑板</param>
        public void UpdateIntentionTranslation(in ProcessedInputData current, in ProcessedInputData last, PlayerBrain blackboard)
        {
            blackboard.WantToMove = current.Move.sqrMagnitude > 0.01f;
        }
    }
}
