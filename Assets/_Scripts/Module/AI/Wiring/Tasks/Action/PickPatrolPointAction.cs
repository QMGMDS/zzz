using UnityEngine;

using BehaviorDesigner.Runtime.Tasks;

using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 巡逻取点动作 - 在巡逻半径内随机取点 与上一巡逻点保持最小间距 单帧完成并写入代码黑板
    /// </summary>
    internal sealed class PickPatrolPointAction : Action
    {
        private const int MaxAttempts = 10;

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

            Vector3 anchor = _blackboard.AnchorPosition;
            Vector3 candidate = Vector3.zero;

            for (int i = 0; i < MaxAttempts; i++)
            {
                Vector2 offset = Random.insideUnitCircle * _brain.Config.PatrolRadius;
                candidate = new Vector3(anchor.x + offset.x, anchor.y, anchor.z + offset.y);

                // 无上一巡逻点 或与上一巡逻点满足最小间距时接受
                if (!_blackboard.HasLastPatrolPoint
                    || PerceptionUtility.DistanceXZ(candidate, _blackboard.LastPatrolPoint) >= _brain.Config.PatrolMinStepDistance)
                    break;
            }

            // 全部尝试均不满足最小间距时降级接受最后一次候选点 - 最小间距大于巡逻直径时约束无法满足 避免死循环
            _blackboard.SetPatrolPoint(candidate);
            return TaskStatus.Success;
        }
    }
}
