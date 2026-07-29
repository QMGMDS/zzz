using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace SPCharacterController
{
    /// <summary>
    /// 随机巡逻取点 - 以敌人当前位置为圆心、巡逻范围为半径随机取 XZ 平面点写入目标点。
    /// </summary>
    [TaskCategory("SPCharacter")]
    [TaskDescription("在巡逻范围内随机取点写入目标点。")]
    public class RandomPatrolPoint : SPAIAction
    {
        [Tooltip("巡逻范围（绑定共享变量 PatrolRange）")]
        public SharedFloat PatrolRange;

        [Tooltip("输出：目标点（绑定共享变量 TargetPosition）")]
        public SharedVector3 TargetPosition;

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            Vector2 circle = Random.insideUnitCircle * PatrolRange.Value;
            TargetPosition.Value = transform.position + new Vector3(circle.x, 0f, circle.y);
            return TaskStatus.Success;
        }
    }
}
