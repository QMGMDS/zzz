using BehaviorDesigner.Runtime.Tasks;

using SPCharacter.Contract;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 攻击动作 - 目标处于视野内且在攻击范围内时停走 面向目标并每帧请求一次攻击 目标出视野 出范围或无目标时结束
    /// </summary>
    internal sealed class AttackAction : Action
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
                return TaskStatus.Success;

            float distance = PerceptionUtility.DistanceXZ(transform.position, _blackboard.TargetPosition);
            if (distance > _brain.Config.AttackRange)
                return TaskStatus.Success;

            if (!_brain.TryGetAgentSession(out ICharacterAgentSession session))
                return TaskStatus.Failure;

            // 不写入移动轴即停走 朝向轴驱动面向目标 连击由角色状态机消化
            session.SetFacingDirection(PerceptionUtility.DirectionXZ(transform.position, _blackboard.TargetPosition));
            session.RequestAttack();
            return TaskStatus.Running;
        }
    }
}
