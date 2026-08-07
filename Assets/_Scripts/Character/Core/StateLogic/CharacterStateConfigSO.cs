using System;
using UnityEngine;
using SPCharacter.Contract;

namespace SPCharacter.Core
{
    /// <summary>
    /// 角色状态转移规则配置 ScriptableObject - 包含该角色所有状态节点和状态间转移规则
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacter/StateLogic/CharacterStateConfig", fileName = "CharacterStateConfig")]
    public class CharacterStateConfigSO : ScriptableObject
    {
        [SerializeField, Tooltip("状态机入口节点 Id（对应 StateNodeSO.Id）")]
        private string _entryId;

        [SerializeField, Tooltip("所有状态节点")]
        private StateNodeSO[] _nodes;

        [SerializeField, Tooltip("状态间转移规则列表（由 TransitionTableImporter 从 Excel 生成）")]
        private StateTransitionRule[] _rules;

        /// <summary>状态机入口节点 Id（对应 StateNodeSO.Id）。</summary>
        public string EntryId
        {
            get => _entryId;
            internal set => _entryId = value;
        }

        /// <summary>所有状态节点。</summary>
        public StateNodeSO[] Nodes
        {
            get => _nodes;
            internal set => _nodes = value;
        }

        /// <summary>状态间转移规则列表（由 TransitionTableImporter 从 Excel 生成）。</summary>
        public StateTransitionRule[] Rules
        {
            get => _rules;
            internal set => _rules = value;
        }
    }

    /// <summary>
    /// 状态转移规则——定义从某个节点到另一个节点的切换条件和目标
    /// </summary>
    [Serializable]
    public struct StateTransitionRule
    {
        [SerializeField, Tooltip("来源节点 Id（对应 StateNodeSO.Id）")]
        private string _fromId;

        [SerializeField, Tooltip("目标节点 Id（对应 StateNodeSO.Id）")]
        private string _toId;

        [SerializeField, Tooltip("触发条件，由必须为 1 的意图位与必须为 0 的意图位组成")]
        private StateTransitionCondition _condition;

        [SerializeField, Range(0f, 1f), Tooltip("来源状态动画归一化进度达到该值后，才允许执行此转移；0 表示立即允许")]
        private float _interruptPoint;

        /// <summary>来源节点 Id。</summary>
        public string FromId => _fromId;

        /// <summary>目标节点 Id。</summary>
        public string ToId => _toId;

        /// <summary>状态转移触发条件。</summary>
        public StateTransitionCondition Condition => _condition;

        /// <summary>该转移允许发生的最小来源动画归一化进度。</summary>
        public float InterruptPoint => _interruptPoint;

        /// <summary>
        /// 创建状态转移规则。
        /// </summary>
        /// <param name="fromId">来源节点 Id</param>
        /// <param name="toId">目标节点 Id</param>
        /// <param name="condition">触发条件</param>
        /// <param name="interruptPoint">最小来源动画归一化进度，范围为 0 到 1</param>
        public StateTransitionRule(
            string fromId,
            string toId,
            StateTransitionCondition condition,
            float interruptPoint)
        {
            if (float.IsNaN(interruptPoint) || interruptPoint < 0f || interruptPoint > 1f)
                throw new ArgumentOutOfRangeException(nameof(interruptPoint), interruptPoint, "打断点必须位于 0 到 1 之间。");

            _fromId = fromId;
            _toId = toId;
            _condition = condition;
            _interruptPoint = interruptPoint;
        }
    }

    /// <summary>
    /// 状态转移条件 - 用两组位掩码表达“指定位必须为 1 / 指定位必须为 0”。
    /// 未出现在任一组中的意图位视为“自由”，不影响判定。
    /// </summary>
    [Serializable]
    public struct StateTransitionCondition
    {
        [SerializeField, Tooltip("必须全部为 1 的意图位（位掩码组合，None 表示不要求）")]
        private CharacterIntention _required;

        [SerializeField, Tooltip("必须全部为 0 的意图位（位掩码组合，None 表示不禁止）")]
        private CharacterIntention _forbidden;
        /* 为什么要多出一个反表格？
            例子：
            转换条件是：00000 要求第二位为 0
            当前条件是：00110
            00000 & 00110 -> 00000 == 00000 却能成功转换。

            由此可见，位运算只能判断相关性，该位是否与 1/0 有关。
            若该位与 1 有关，也不能说其与 0 无关。
            这是位运算的缺陷，尽管在数值表示上一位只有 1/0 两种表示。
        */

        /// <summary>必须全部为 1 的意图位（位掩码组合，None 表示不要求）。</summary>
        public CharacterIntention Required
        {
            get => _required;
            internal set => _required = value;
        }

        /// <summary>必须全部为 0 的意图位（位掩码组合，None 表示不禁止）。</summary>
        public CharacterIntention Forbidden
        {
            get => _forbidden;
            internal set => _forbidden = value;
        }
    }
}