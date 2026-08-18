using BehaviorDesigner.Runtime.Tasks;

using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 目标存在条件 - 黑板持有有效目标时通过
    /// </summary>
    internal sealed class HasTargetCondition : Conditional
    {
        private AIRuntimeBlackboard _blackboard;

        /// <inheritdoc />
        public override void OnAwake()
        {
            AIBrain brain = GetComponent<AIBrain>();
            if (brain != null)
                _blackboard = brain.Blackboard;
        }

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
            => _blackboard != null && _blackboard.HasTarget ? TaskStatus.Success : TaskStatus.Failure;
    }
}
