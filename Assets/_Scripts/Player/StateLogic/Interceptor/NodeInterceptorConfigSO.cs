using System;
using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 节点拦截器配置 SO——定义所有跨族精确节点转移规则。
    /// 每条规则指定来源节点(可多选)、触发条件和目标(族+节点索引)。
    /// 按 Priority 升序检测，最小优先，同帧至多触发一条。
    /// </summary>
    [CreateAssetMenu(fileName = "NodeInterceptorConfig", menuName = "Player/NodeInterceptorConfig")]
    public class NodeInterceptorConfigSO : ScriptableObject
    {
        [Tooltip("拦截规则列表（按 Priority 升序排序，越小越先检测）")]
        public NodeInterceptor[] Interceptors;
    }

    /// <summary>
    /// 单条跨族拦截规则——当当前节点匹配 FromNodes 且 Condition 满足时，跳转到目标族的指定节点。
    /// </summary>
    [Serializable]
    public struct NodeInterceptor
    {
        [Tooltip("触发本拦截的来源节点")]
        public StateNodeSO[] FromNodes;

        [Tooltip("触发条件")]
        public TransitionCondition Condition;

        [Tooltip("目标状态族")]
        public StateGroupSO TargetGroup;

        [Tooltip("目标节点在目标族 Nodes 数组中的索引")]
        public int TargetNodeIndex;

        [Tooltip("检测优先级，越小越先")]
        public int Priority;

        [Tooltip("调试用标识")]
        public string DisplayName;
    }
}
