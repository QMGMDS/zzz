namespace SPPlayer
{
    /// <summary>
    /// 闪避输入意图翻译器
    /// </summary>
    public class EvadeInputProcessor : IInputProcessor
    {
        /// <summary>
        /// 将处理后的输入翻译为闪避意图，写入黑板
        /// </summary>
        /// <param name="current">当前帧处理后的输入数据</param>
        /// <param name="last">上一帧处理后的输入数据</param>
        /// <param name="blackboard">玩家大脑黑板</param>
        public void UpdateIntentionTranslation(in ProcessedInputData current, in ProcessedInputData last, PlayerBrain blackboard)
        {
            if (current.EvadePressed)
                blackboard.WantToEvade = true;
        }
    }
}
