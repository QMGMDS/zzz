using System;
using System.Collections.Generic;

namespace SPCharacter.Core
{
    /// <summary>
    /// 状态机 - 管理状态转移，每帧检测规则并执行切换。
    /// 转移规则按来源 Id 分桶到哈希表，运行期只遍历当前状态的出边规则。
    /// </summary>
    public class StateMachine
    {
        private readonly Dictionary<string, StateNodeSO> _nodesById;
        private readonly Dictionary<string, List<StateTransitionRule>> _rulesByFromId;
        private readonly CharacterRunTimeData _blackboard;
        private string _currentStateId;

        /// <summary>
        /// 创建状态机并发布入口状态。
        /// </summary>
        /// <param name="config">角色状态配置</param>
        /// <param name="blackboard">角色运行时黑板</param>
        public StateMachine(CharacterStateConfigSO config, CharacterRunTimeData blackboard)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (blackboard == null) throw new ArgumentNullException(nameof(blackboard));
            if (config.Nodes == null || config.Nodes.Length == 0)
                throw new ArgumentException("角色状态配置没有状态节点。", nameof(config));
            if (config.Rules == null) throw new ArgumentException("角色状态配置没有转移规则。", nameof(config));
            if (string.IsNullOrEmpty(config.EntryId))
                throw new ArgumentException("角色状态配置没有设置入口节点 Id。", nameof(config));

            _nodesById = new Dictionary<string, StateNodeSO>(StringComparer.Ordinal);
            for (int i = 0; i < config.Nodes.Length; i++)
            {
                StateNodeSO node = config.Nodes[i];
                if (node == null) throw new ArgumentException($"角色状态配置的 Nodes[{i}] 为空。", nameof(config));
                if (string.IsNullOrEmpty(node.Id))
                    throw new ArgumentException($"角色状态配置的 Nodes[{i}].Id 为空。", nameof(config));
                if (_nodesById.ContainsKey(node.Id))
                    throw new ArgumentException($"角色状态配置存在重复的状态 Id：{node.Id}。", nameof(config));
                _nodesById.Add(node.Id, node);
            }

            _rulesByFromId = new Dictionary<string, List<StateTransitionRule>>(StringComparer.Ordinal);
            for (int i = 0; i < config.Rules.Length; i++)
            {
                StateTransitionRule rule = config.Rules[i];
                if (string.IsNullOrEmpty(rule.FromId))
                    throw new ArgumentException($"角色状态配置的 Rules[{i}].FromId 为空。", nameof(config));
                if (string.IsNullOrEmpty(rule.ToId))
                    throw new ArgumentException($"角色状态配置的 Rules[{i}].ToId 为空。", nameof(config));
                if (!_nodesById.ContainsKey(rule.FromId))
                    throw new ArgumentException($"角色状态配置的 Rules[{i}].FromId 指向不存在的状态：{rule.FromId}。", nameof(config));
                if (!_nodesById.ContainsKey(rule.ToId))
                    throw new ArgumentException($"角色状态配置的 Rules[{i}].ToId 指向不存在的状态：{rule.ToId}。", nameof(config));

                if (!_rulesByFromId.TryGetValue(rule.FromId, out List<StateTransitionRule> edges))
                {
                    edges = new List<StateTransitionRule>();
                    _rulesByFromId.Add(rule.FromId, edges);
                }
                edges.Add(rule);
            }

            if (!_nodesById.ContainsKey(config.EntryId))
                throw new ArgumentException($"入口节点 Id 不存在：{config.EntryId}。", nameof(config));

            _blackboard = blackboard;
            _currentStateId = config.EntryId;
            PublishCurrentState();
        }

        /// <summary>
        /// 只读状态节点解析入口 - 供动画驱动等内部子系统按 Id 反查节点数据。
        /// </summary>
        public IReadOnlyDictionary<string, StateNodeSO> NodesById => _nodesById;

        public void LogicUpdate()
        {
            TryTransitionRule();
        }

        private void TryTransitionRule()
        {
            if (!_rulesByFromId.TryGetValue(_currentStateId, out List<StateTransitionRule> edges))
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
            if (!_nodesById.TryGetValue(id, out _))
                throw new InvalidOperationException($"状态转移目标 Id 不存在：{id}");
            if (id == _currentStateId)
                return;

            StateNodeSO previousNode = _nodesById[_currentStateId];
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