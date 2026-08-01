using System;
namespace SPCharacterController
{
    /// <summary>
    /// 状态机 - 管理状态转移，每帧检测规则并执行切换。
    /// 条件评估委托给 CharacterRunTimeData 黑板，自身不感知具体角色类型。
    /// </summary>
    public class StateMachine
    {
        private CharacterStateConfigSO _config;
        private CharacterRunTimeData _blackboard;
        private int _currentNodeIndex = -1;

        /// <summary>
        /// 创建状态机并发布入口状态。
        /// </summary>
        /// <param name="config">角色状态配置</param>
        /// <param name="blackboard">角色运行时黑板</param>
        /// <param name="entryIndex">入口状态索引</param>
        public StateMachine(CharacterStateConfigSO config, CharacterRunTimeData blackboard, int entryIndex)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (blackboard == null) throw new ArgumentNullException(nameof(blackboard));
            if (config.Nodes == null || config.Nodes.Length == 0) throw new ArgumentException("角色状态配置没有状态节点。", nameof(config));
            for (int i = 0; i < config.Nodes.Length; i++)
                if (config.Nodes[i] == null) throw new ArgumentException($"角色状态配置的 Nodes[{i}] 为空。", nameof(config));
            if (config.Rules == null) throw new ArgumentException("角色状态配置没有转移规则。", nameof(config));
            if (entryIndex < 0 || entryIndex >= config.Nodes.Length) throw new ArgumentOutOfRangeException(nameof(entryIndex));

            _config = config;
            _blackboard = blackboard;
            _currentNodeIndex = entryIndex;
            PublishCurrentState();
        }

        public void LogicUpdate()
        {
            TryTransitionRule();
        }

        private void TryTransitionRule()
        {
            bool onlyCompletionTransition = !IsInsideInterruptWindow();

            foreach (var rule in _config.Rules)
            {
                if (rule.FromIndex != _currentNodeIndex)
                    continue;

                if (onlyCompletionTransition && (rule.Condition & CharacterIntention.AnimationCompleted) == 0)
                    continue;

                if (!_blackboard.EvaluateCondition(rule.Condition))
                    continue;

                TransitionToNode(rule.ToIndex);
                return;
            }
        }

        private bool IsInsideInterruptWindow()
        {
            StateNodeSO currentNode = _config.Nodes[_currentNodeIndex];
            if (!currentNode.UseInterruptWindow)
                return true;

            return currentNode.InterruptWindow.Contains(_blackboard.AnimationNormalizedTime);
        }

        private void TransitionToNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= _config.Nodes.Length)
                throw new InvalidOperationException($"状态转移目标索引超限：{nodeIndex}");
            if (nodeIndex == _currentNodeIndex)
                return;

            _currentNodeIndex = nodeIndex;
            PublishCurrentState();
        }

        private void PublishCurrentState()
        {
            _blackboard.PublishState(_config.Nodes[_currentNodeIndex]);
        }
    }
}
