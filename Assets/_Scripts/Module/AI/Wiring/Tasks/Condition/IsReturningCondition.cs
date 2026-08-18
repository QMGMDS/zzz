using BehaviorDesigner.Runtime.Tasks;

using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 脱战回归条件 - 已脱战且尚未回归巡逻范围时通过 用于驱动脱战回归分支
    /// </summary>
    internal sealed class IsReturningCondition : Conditional
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
        {
            if (_blackboard == null)
                return TaskStatus.Failure;

            return _blackboard.IsReturning ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
