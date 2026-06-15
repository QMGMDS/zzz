namespace SPPlayer
{
    /// <summary>
    /// 输入意图翻译处理器接口——读取本帧与上一帧的后处理数据，将其翻译为玩家意图写入 PlayerBrain 黑板
    /// 每个处理器只负责一个维度的意图翻译，保持单一职责。
    /// </summary>
    public interface IInputProcessor
    {
        /// <summary>
        /// 执行玩家输入意图翻译
        /// </summary>
        /// <param name="current">本帧纯化后输入数据（只读引用）</param>
        /// <param name="last">上一帧纯化后输入数据（只读引用）</param>
        /// <param name="blackboard">角色大脑黑板，写入翻译结果</param>
        void UpdateIntentionTranslation(in ProcessedInputData current, in ProcessedInputData last, PlayerBrain blackboard);
    }
}
