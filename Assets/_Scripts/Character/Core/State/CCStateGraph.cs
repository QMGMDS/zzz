using System.Collections.Generic;

namespace SPCharacter.Core
{
    /// <summary>
    /// 运行期状态图 - 保存已校验的节点与转移规则查询表
    /// </summary>
    internal sealed class CCStateGraph
    {
        /// <summary>
        /// 创建运行期状态图
        /// </summary>
        /// <param name="entryId">入口状态 Id</param>
        /// <param name="nodesById">状态节点查询表</param>
        /// <param name="rulesByFromId">状态转移规则查询表</param>
        public CCStateGraph(
            string entryId,
            IReadOnlyDictionary<string, StateNodeSO> nodesById,
            IReadOnlyDictionary<string, IReadOnlyList<StateTransitionRule>> rulesByFromId)
        {
            EntryId = entryId;
            NodesById = nodesById;
            RulesByFromId = rulesByFromId;
        }

        /// <summary>入口状态 Id</summary>
        public string EntryId { get; }

        /// <summary>状态节点查询表</summary>
        public IReadOnlyDictionary<string, StateNodeSO> NodesById { get; }

        /// <summary>状态转移规则查询表</summary>
        public IReadOnlyDictionary<string, IReadOnlyList<StateTransitionRule>> RulesByFromId { get; }
    }
}
