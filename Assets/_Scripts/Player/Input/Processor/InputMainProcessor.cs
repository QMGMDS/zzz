using System.Collections.Generic;

namespace SPPlayer
{
    /// <summary>
    /// 输入意图翻译主处理器-子处理器的生成工厂
    /// 读取 InputCollector 处理后的输入数据，驱动各子处理器 InputProcessor
    /// 子处理器负责将输入翻译为意图写入 PlayerBrain 黑板
    /// 下游系统仅需读取黑板即可获取玩家输入意图，无需接触原始数据
    /// </summary>
    public class InputMainProcessor
    {
        private readonly InputCollector _collector;
        private readonly PlayerBrain _blackboard;
        private readonly List<IInputProcessor> _processors;

        /// <summary>
        /// 创建主处理器
        /// </summary>
        /// <param name="collector">输入采集员</param>
        /// <param name="blackboard">意图黑板</param>
        public InputMainProcessor(InputCollector collector, PlayerBrain blackboard)
        {
            _collector = collector;
            _blackboard = blackboard;
            _processors = new List<IInputProcessor>
            {
                new MoveInputProcessor(),
            };
        }

        /// <summary>
        /// 遍历所有意图处理器，将处理后输入翻译为输入意图写入黑板。
        /// 每帧由外部驱动。
        /// </summary>
        public void UpdateInputProcessors()
        {
            ref readonly var current = ref _collector.Current.CurrentFrameData.Processed;
            ref readonly var last = ref _collector.Current.LastFrameData.Processed;

            foreach (var processor in _processors)
            {
                processor.UpdateIntentionTranslation(in current, in last, _blackboard);
            }
        }
    }
}
