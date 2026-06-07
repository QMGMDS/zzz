using System.Collections.Generic;
using Core.Input.Processors;

namespace Core.Input
{
    /// <summary>
    /// 主处理器管线
    /// 读取 InputCollector 处理后的输入数据，驱动各 IntentProcessor
    /// 将输入翻译为意图写入 IntentionBlackboard。
    /// 下游系统仅需读取黑板即可获取角色意图，无需接触原始数据。
    /// </summary>
    public class MainProcessorPipeline
    {
        private readonly InputCollector _collector;
        private readonly IntentionBlackboard _blackboard;
        private readonly List<IIntentProcessor> _processors;

        /// <summary>
        /// 创建主处理器管线
        /// </summary>
        /// <param name="collector">输入采集员引用</param>
        /// <param name="blackboard">意图黑板（由外部传入，归属 PlayerController）</param>
        public MainProcessorPipeline(InputCollector collector, IntentionBlackboard blackboard)
        {
            _collector = collector;
            _blackboard = blackboard;
            _processors = new List<IIntentProcessor>
            {
                new MoveIntentProcessor(),
                new AttackIntentProcessor(collector),
                new EvadeIntentProcessor(collector)
            };
        }

        /// <summary>
        /// 遍历所有意图处理器，将处理后输入翻译为离散意图写入黑板。
        /// 每帧由外部驱动（通常在 InputCollector.Update 之后）。
        /// </summary>
        public void UpdateIntentProcessors()
        {
            ref readonly var input = ref _collector.Current.CurrentFrameData.Processed;

            foreach (var processor in _processors)
            {
                processor.Update(in input, _blackboard);
            }
        }
    }
}
