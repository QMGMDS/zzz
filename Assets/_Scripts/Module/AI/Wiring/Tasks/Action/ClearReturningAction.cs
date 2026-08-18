using BehaviorDesigner.Runtime.Tasks;

using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 清除脱战回归动作 - 回归巡逻范围完成后复位脱战状态 恢复常规巡逻
    /// </summary>
    internal sealed class ClearReturningAction : Action
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

            _blackboard.CompleteReturning();
            return TaskStatus.Success;
        }
    }
}
