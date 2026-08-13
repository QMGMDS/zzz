using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 角色状态转移规则配置 - 包含该角色所有状态节点和状态间转移规则
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacter/State/CCStateConfig", fileName = "CCStateConfig")]
    internal sealed class CCStateConfigSO : ScriptableObject
    {
        [SerializeField, Tooltip("状态机入口节点 Id")]
        private string _entryId;

        [SerializeField, Tooltip("所有状态节点")]
        private StateNodeSO[] _nodes;

        [SerializeField, Tooltip("状态间转移规则列表")]
        private StateTransitionRule[] _rules;

        /// <summary>状态机入口节点 Id</summary>
        public string EntryId => _entryId;

        /// <summary>所有状态节点</summary>
        public StateNodeSO[] Nodes => _nodes;

        /// <summary>状态间转移规则列表</summary>
        public StateTransitionRule[] Rules => _rules;

        /// <summary>
        /// 构建运行期状态图并校验配置完整性
        /// </summary>
        public CCStateGraph BuildRuntimeGraph()
        {
            ValidateRootFields();

            Dictionary<string, StateNodeSO> nodesById = BuildNodesById();
            Dictionary<string, List<StateTransitionRule>> mutableRulesByFromId = BuildRulesByFromId(nodesById);
            ValidateEntryId(nodesById);

            return new CCStateGraph(
                _entryId,
                new ReadOnlyDictionary<string, StateNodeSO>(nodesById),
                new ReadOnlyDictionary<string, IReadOnlyList<StateTransitionRule>>(FreezeRules(mutableRulesByFromId)));
        }

        private void ValidateRootFields()
        {
            if (_nodes == null || _nodes.Length == 0)
                throw new ArgumentException("角色状态配置没有状态节点。", nameof(_nodes));
            if (_rules == null)
                throw new ArgumentException("角色状态配置没有转移规则。", nameof(_rules));
            if (string.IsNullOrEmpty(_entryId))
                throw new ArgumentException("角色状态配置没有设置入口节点 Id。", nameof(_entryId));
        }

        private Dictionary<string, StateNodeSO> BuildNodesById()
        {
            Dictionary<string, StateNodeSO> nodesById = new Dictionary<string, StateNodeSO>(StringComparer.Ordinal);
            for (int i = 0; i < _nodes.Length; i++)
            {
                StateNodeSO node = _nodes[i];
                if (node == null)
                    throw new ArgumentException($"角色状态配置的 Nodes[{i}] 为空。", nameof(_nodes));
                if (string.IsNullOrEmpty(node.Id))
                    throw new ArgumentException($"角色状态配置的 Nodes[{i}].Id 为空。", nameof(_nodes));
                if (nodesById.ContainsKey(node.Id))
                    throw new ArgumentException($"角色状态配置存在重复的状态 Id：{node.Id}。", nameof(_nodes));

                nodesById.Add(node.Id, node);
            }

            return nodesById;
        }

        private Dictionary<string, List<StateTransitionRule>> BuildRulesByFromId(
            IReadOnlyDictionary<string, StateNodeSO> nodesById)
        {
            Dictionary<string, List<StateTransitionRule>> rulesByFromId =
                new Dictionary<string, List<StateTransitionRule>>(StringComparer.Ordinal);

            for (int i = 0; i < _rules.Length; i++)
            {
                StateTransitionRule rule = _rules[i];
                ValidateRule(rule, i, nodesById);

                if (!rulesByFromId.TryGetValue(rule.FromId, out List<StateTransitionRule> edges))
                {
                    edges = new List<StateTransitionRule>();
                    rulesByFromId.Add(rule.FromId, edges);
                }

                AddRuleByPriority(edges, rule);
            }

            return rulesByFromId;
        }

        private static void AddRuleByPriority(List<StateTransitionRule> edges, StateTransitionRule rule)
        {
            int insertIndex = edges.Count;
            for (int i = 0; i < edges.Count; i++)
            {
                if (rule.Priority <= edges[i].Priority)
                    continue;

                insertIndex = i;
                break;
            }

            edges.Insert(insertIndex, rule);
        }

        private void ValidateRule(
            StateTransitionRule rule,
            int index,
            IReadOnlyDictionary<string, StateNodeSO> nodesById)
        {
            if (string.IsNullOrEmpty(rule.FromId))
                throw new ArgumentException($"角色状态配置的 Rules[{index}].FromId 为空。", nameof(_rules));
            if (string.IsNullOrEmpty(rule.ToId))
                throw new ArgumentException($"角色状态配置的 Rules[{index}].ToId 为空。", nameof(_rules));
            if (!nodesById.ContainsKey(rule.FromId))
                throw new ArgumentException($"角色状态配置的 Rules[{index}].FromId 指向不存在的状态：{rule.FromId}。", nameof(_rules));
            if (!nodesById.ContainsKey(rule.ToId))
                throw new ArgumentException($"角色状态配置的 Rules[{index}].ToId 指向不存在的状态：{rule.ToId}。", nameof(_rules));
        }

        private void ValidateEntryId(IReadOnlyDictionary<string, StateNodeSO> nodesById)
        {
            if (!nodesById.ContainsKey(_entryId))
                throw new ArgumentException($"入口节点 Id 不存在：{_entryId}。", nameof(_entryId));
        }

        private static Dictionary<string, IReadOnlyList<StateTransitionRule>> FreezeRules(
            IReadOnlyDictionary<string, List<StateTransitionRule>> mutableRulesByFromId)
        {
            Dictionary<string, IReadOnlyList<StateTransitionRule>> rulesByFromId =
                new Dictionary<string, IReadOnlyList<StateTransitionRule>>(StringComparer.Ordinal);

            foreach (KeyValuePair<string, List<StateTransitionRule>> pair in mutableRulesByFromId)
            {
                rulesByFromId.Add(pair.Key, pair.Value.AsReadOnly());
            }

            return rulesByFromId;
        }
    }

    /// <summary>
    /// 状态转移规则
    /// </summary>
    [Serializable]
    internal struct StateTransitionRule
    {
        [SerializeField, Tooltip("来源节点 Id")]
        private string _fromId;

        [SerializeField, Tooltip("目标节点 Id")]
        private string _toId;

        [SerializeField, Tooltip("触发条件")]
        private StateTransitionCondition _condition;

        [SerializeField, Tooltip("转移优先级，数值越大越优先；同优先级按配置顺序判断")]
        private int _priority;

        [SerializeField, Range(0f, 1f), Tooltip("来源状态动画归一化进度达到该值后，才允许执行此转移；0 表示立即允许")]
        private float _interruptPoint;

        /// <summary>来源节点 Id</summary>
        public string FromId => _fromId;

        /// <summary>目标节点 Id</summary>
        public string ToId => _toId;

        /// <summary>状态转移触发条件</summary>
        public StateTransitionCondition Condition => _condition;

        /// <summary>转移优先级，数值越大越优先</summary>
        public int Priority => _priority;

        /// <summary>该转移所需的最小归一化进度（来源动画）</summary>
        public float InterruptPoint => _interruptPoint;

        /// <summary>
        /// 创建状态转移规则
        /// </summary>
        /// <param name="fromId">来源节点 Id</param>
        /// <param name="toId">目标节点 Id</param>
        /// <param name="condition">触发条件</param>
        /// <param name="interruptPoint">最小来源动画归一化进度，范围为 0 到 1</param>
        /// <param name="priority">转移优先级，数值越大越优先</param>
        public StateTransitionRule(
            string fromId,
            string toId,
            StateTransitionCondition condition,
            float interruptPoint,
            int priority = 0)
        {
            if (float.IsNaN(interruptPoint) || interruptPoint < 0f || interruptPoint > 1f)
                throw new ArgumentOutOfRangeException(nameof(interruptPoint), interruptPoint, "打断点必须位于 0 到 1 之间");

            _fromId = fromId;
            _toId = toId;
            _condition = condition;
            _priority = priority;
            _interruptPoint = interruptPoint;
        }
    }

    /// <summary>
    /// 状态转移条件 - 用两组位掩码表达 "指定位必须为 1 / 指定位必须为 0"
    /// 未出现在任一组中的意图位视为 "自由"，不影响判定
    /// </summary>
    [Serializable]
    internal struct StateTransitionCondition
    {
        [SerializeField, Tooltip("必须全部为 1 的意图位（位掩码组合，None 表示不要求）")]
        private CCIntention _required;

        [SerializeField, Tooltip("必须全部为 0 的意图位（位掩码组合，None 表示不禁止）")]
        private CCIntention _forbidden;
        /* 为什么要多出一个反表格？
            例子：
            转换条件是：00000 要求第二位为 0
            当前条件是：00110
            00000 & 00110 -> 00000 == 00000 却能成功转换。

            由此可见，位运算只能判断相关性，该位是否与 1/0 有关。
            若该位与 1 有关，也不能说其与 0 无关。
            这是位运算的缺陷，尽管在数值表示上一位只有 1/0 两种表示。
        */

        /// <summary>
        /// 创建状态转移条件
        /// </summary>
        /// <param name="required">必须全部为 1 的意图位</param>
        /// <param name="forbidden">必须全部为 0 的意图位</param>
        public StateTransitionCondition(CCIntention required, CCIntention forbidden)
        {
            if ((required & forbidden) != CCIntention.None)
                throw new ArgumentException("同一个意图不能同时作为必需和禁止条件。", nameof(forbidden));

            _required = required;
            _forbidden = forbidden;
        }

        /// <summary>必须全部为 1 的意图位</summary>
        public CCIntention Required => _required;

        /// <summary>必须全部为 0 的意图位</summary>
        public CCIntention Forbidden => _forbidden;
    }
}
