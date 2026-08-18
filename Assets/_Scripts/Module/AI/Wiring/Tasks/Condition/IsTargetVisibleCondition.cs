using BehaviorDesigner.Runtime.Tasks;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 目标可见条件 - 黑板中存在目标且目标当前处于可见状态
    /// </summary>
    internal sealed class IsTargetVisibleCondition : Conditional
    {
        private AIRuntimeBlackboard _blackboard;

        /// <inheritdoc />
        public override void OnAwake()
        {
            AIBrain brain = GetComponent<AIBrain>();
            if (brain != null)
            {
                _blackboard = brain.Blackboard;
            }
        }

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            return _blackboard != null && _blackboard.HasTarget && _blackboard.IsTargetVisible
                ? TaskStatus.Success
                : TaskStatus.Failure;
        }
    }
}
