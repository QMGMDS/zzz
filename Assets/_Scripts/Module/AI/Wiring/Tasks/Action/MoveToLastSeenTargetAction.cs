using BehaviorDesigner.Runtime.Tasks;
using SPCharacter.Contract;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 移动到最后目击位置动作 - 目标不可见时移动到黑板记录的最后目标位置
    /// </summary>
    internal sealed class MoveToLastSeenTargetAction : Action
    {
        private AIBrain _brain;
        private AIRuntimeBlackboard _blackboard;

        /// <inheritdoc />
        public override void OnAwake()
        {
            _brain = GetComponent<AIBrain>();
            if (_brain != null)
            {
                _blackboard = _brain.Blackboard;
            }
        }

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            if (_brain == null || _blackboard == null || !_blackboard.HasTarget || _blackboard.IsTargetVisible)
            {
                return TaskStatus.Failure;
            }

            if (!_brain.TryGetAgentSession(out ICharacterAgentSession session))
            {
                return TaskStatus.Failure;
            }

            float distance = PerceptionUtility.DistanceXZ(transform.position, _blackboard.TargetPosition);
            if (distance <= _brain.Config.PatrolArriveDistance)
            {
                return TaskStatus.Success;
            }

            session.SetMoveAxis(PerceptionUtility.DirectionXZ(transform.position, _blackboard.TargetPosition));
            return TaskStatus.Running;
        }
    }
}
