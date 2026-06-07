using Core.Input.Data;

namespace Core.Input.Processors
{
    /// <summary>
    /// 意图处理器接口——读取处理后输入数据，将其翻译为角色意图写入 IntentionBlackboard。
    /// 每个处理器只负责一个维度的意图翻译，保持单一职责。
    /// </summary>
    public interface IIntentProcessor
    {
        /// <summary>
        /// 执行意图翻译
        /// </summary>
        /// <param name="input">本帧处理后输入数据</param>
        /// <param name="blackboard">意图黑板，写入翻译结果</param>
        void Update(in ProcessedInputData input, IntentionBlackboard blackboard);
    }
}
