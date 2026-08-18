using BehaviorDesigner.Runtime.Tasks;

using SPCharacter.Contract;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 回归巡逻范围动作 - 每帧朝巡逻锚点移动 进入巡逻半径内停走并结束 由脱战回归条件与清除动作包围
    /// </summary>
    internal sealed class ReturnToAnchorAction : Action
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
            if (_brain == null || _blackboard == null)
                return TaskStatus.Failure;

            float distance = PerceptionUtility.DistanceXZ(transform.position, _blackboard.AnchorPosition);
            if (distance <= _brain.Config.PatrolRadius)
                return TaskStatus.Success;

            if (!_brain.TryGetAgentSession(out ICharacterAgentSession session))
                return TaskStatus.Failure;

            session.SetMoveAxis(PerceptionUtility.DirectionXZ(transform.position, _blackboard.AnchorPosition));
            return TaskStatus.Running;
        }
    }
}
