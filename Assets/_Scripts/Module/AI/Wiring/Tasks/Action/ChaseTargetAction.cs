using BehaviorDesigner.Runtime.Tasks;
using SPCharacter.Contract;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 追逐目标动作 - 持续朝黑板中的当前目标位置移动
    /// </summary>
    internal sealed class ChaseTargetAction : Action
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
            if (_brain == null || _blackboard == null || !_blackboard.HasTarget || !_blackboard.IsTargetVisible)
            {
                return TaskStatus.Failure;
            }

            if (!_brain.TryGetAgentSession(out ICharacterAgentSession session))
            {
                return TaskStatus.Failure;
            }

            session.SetMoveAxis(PerceptionUtility.DirectionXZ(transform.position, _blackboard.TargetPosition));
            return TaskStatus.Running;
        }
    }
}
