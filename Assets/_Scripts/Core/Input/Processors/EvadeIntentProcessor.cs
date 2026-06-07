using Core.Input.Data;

namespace Core.Input.Processors
{
    /// <summary>
    /// 闪避意图处理器——检测闪避缓存计时器，将闪避意图写入黑板。
    /// 写入后立即调用 Consume 核销当前帧的 Timer，防止因帧率波动导致同一按键被多重消费。
    /// </summary>
    public class EvadeIntentProcessor : IIntentProcessor
    {
        private readonly InputCollector _collector;

        /// <summary>
        /// 创建闪避意图处理器
        /// </summary>
        /// <param name="collector">输入采集员引用，用于消费核销</param>
        public EvadeIntentProcessor(InputCollector collector)
        {
            _collector = collector;
        }

        /// <inheritdoc/>
        public void Update(in ProcessedInputData input, IntentionBlackboard blackboard)
        {
            blackboard.WantToEvade = input.EvadePressed;

            if (input.EvadePressed)
            {
                _collector.ConsumeEvadePressed();
            }
        }
    }
}
