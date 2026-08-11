using System;
using System.Collections.Generic;

namespace SPCharacter.Core
{
    /// <summary>
    /// 状态机 - 管理状态转移，每帧检测规则并执行切换
    /// </summary>
    internal sealed class CCStateMachine
    {
        private readonly CCStateGraph _graph;
        private readonly CCRunTimeBlackboard _blackboard;
        private string _currentStateId;

        /// <summary>
        /// 创建状态机并发布入口状态
        /// </summary>
        /// <param name="graph">运行期状态图</param>
        /// <param name="blackboard">角色运行时黑板</param>
        public CCStateMachine(CCStateGraph graph, CCRunTimeBlackboard blackboard)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (blackboard == null) throw new ArgumentNullException(nameof(blackboard));

            _graph = graph;
            _blackboard = blackboard;
            _currentStateId = graph.EntryId;
            PublishCurrentState();
        }

        /// <summary>
        /// 执行状态机逻辑更新
        /// </summary>
        public void LogicUpdate()
        {
            TryTransitionRule();
        }

        private void TryTransitionRule()
        {
            if (!_graph.RulesByFromId.TryGetValue(_currentStateId, out IReadOnlyList<StateTransitionRule> edges))
                return;

            for (int i = 0; i < edges.Count; i++)
            {
                StateTransitionRule rule = edges[i];
                if (!_blackboard.EvaluateCondition(rule.Condition))
                    continue;
                if (_blackboard.AnimationNormalizedTime < rule.InterruptPoint)
                    continue;

                TransitionToNode(rule.ToId);
                return;
            }
        }

        private void TransitionToNode(string id)
        {
            if (id == _currentStateId) // 同状态不转移
                return;

            StateNodeSO previousNode = _graph.NodesById[_currentStateId];
            _blackboard.PublishCompletionRotation(previousNode.CompletionRotationDegrees);

            _currentStateId = id;
            PublishCurrentState();
        }

        private void PublishCurrentState()
        {
            _blackboard.PublishState(_currentStateId);
        }
    }
}
