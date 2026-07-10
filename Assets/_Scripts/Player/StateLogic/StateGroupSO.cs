using System;
using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 状态族 ScriptableObject——包含一组状态节点和族内转移规则。
    /// 专门负责族内状态的切换，节点本身不包含任何转移逻辑。
    /// </summary>
    [CreateAssetMenu(fileName = "StateGroup", menuName = "Player/StateGroup")]
    public class StateGroupSO : ScriptableObject
    {
        [Tooltip("本族包含的所有状态节点")]
        public StateNodeSO[] Nodes;

        [Tooltip("族内转移规则列表")]
        public InternalTransitionRule[] Rules;

        [Tooltip("外部通过拦截器进入本族时的默认入口节点索引")]
        public int DefaultEntryIndex;
    }

    /// <summary>
    /// 族内转移规则——定义从某个节点到另一个节点的切换条件和目标。
    /// </summary>
    [Serializable]
    public struct InternalTransitionRule
    {
        [Tooltip("来源节点在本族 Nodes 数组中的索引")]
        public int FromIndex;

        [Tooltip("目标节点在本族 Nodes 数组中的索引")]
        public int ToIndex;

        [Tooltip("触发条件")]
        public TransitionCondition Condition;
    }
}
