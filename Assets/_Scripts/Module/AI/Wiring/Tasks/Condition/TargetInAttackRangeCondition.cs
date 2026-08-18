using BehaviorDesigner.Runtime.Tasks;

using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 攻击范围内条件 - 目标处于视野内且与目标距离不大于攻击范围时通过
    /// </summary>
    internal sealed class TargetInAttackRangeCondition : Conditional
    {
        private AIBrain _brain;
        private AIRuntimeBlackboard _blackboard;

        /// <inheritdoc />
        public override void OnAwake()
        {
            _brain = GetComponent<AIBrain>();
            if (_brain != null)
                _blackboard = _brain.Blackboard;
        }

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            if (_brain == null || _blackboard == null || !_blackboard.HasTarget || !_blackboard.IsTargetVisible)
                return TaskStatus.Failure;

            float distance = PerceptionUtility.DistanceXZ(transform.position, _blackboard.TargetPosition);
            return distance <= _brain.Config.AttackRange ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
