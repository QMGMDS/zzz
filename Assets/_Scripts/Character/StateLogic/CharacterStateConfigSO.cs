using System;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色状态转移规则配置 ScriptableObject - 包含该角色所有状态节点和状态间转移规则
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/StateLogic/CharacterStateConfig", fileName = "CharacterStateConfig")]
    public class CharacterStateConfigSO : ScriptableObject
    {
        [Tooltip("所有状态节点")]
        public StateNodeSO[] Nodes;

        [Tooltip("状态间转移规则列表（由 TransitionTableImporter 从 Excel 生成）")]
        public StateTransitionRule[] Rules;
    }

    /// <summary>
    /// 状态转移规则——定义从某个节点到另一个节点的切换条件和目标
    /// </summary>
    [Serializable]
    public struct StateTransitionRule
    {
        [Tooltip("来源节点在 CharacterStateConfigSO.Nodes 中的索引")]
        public int FromIndex;

        [Tooltip("目标节点在 CharacterStateConfigSO.Nodes 中的索引")]
        public int ToIndex;

        [Tooltip("触发条件（CharacterIntention 位掩码组合，None 表示不可达）")]
        public CharacterIntention Condition;
    }
}
