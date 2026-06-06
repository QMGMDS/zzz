namespace Core.Input.Processors
{
    /// <summary>
    /// 攻击意图处理器——检测攻击缓存计时器，将攻击意图写入黑板。
    /// 写入后立即调用 Consume 核销当前帧的 Timer，防止因帧率波动导致同一按键被多重消费。
    /// </summary>
    public class AttackIntentProcessor : IIntentProcessor
    {
        private readonly InputCollector _collector;

        /// <summary>
        /// 创建攻击意图处理器
        /// </summary>
        /// <param name="collector">输入采集员引用，用于消费核销</param>
        public AttackIntentProcessor(InputCollector collector)
        {
            _collector = collector;
        }

        /// <inheritdoc/>
        public void Update(in ProcessedInputData input, IntentionBlackboard blackboard)
        {
            blackboard.WantToAttack = input.AttackPressed;

            if (input.AttackPressed)
            {
                _collector.ConsumeAttackPressed();
            }
        }
    }
}
