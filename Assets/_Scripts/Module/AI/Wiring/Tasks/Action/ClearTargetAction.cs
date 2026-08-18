using BehaviorDesigner.Runtime.Tasks;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 清除目标动作 - 清除黑板中的目标并进入返回锚点流程
    /// </summary>
    internal sealed class ClearTargetAction : Action
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
            if (_blackboard == null || !_blackboard.HasTarget)
            {
                return TaskStatus.Failure;
            }

            _blackboard.ClearTargetAndBeginReturning();
            return TaskStatus.Success;
        }
    }
}
