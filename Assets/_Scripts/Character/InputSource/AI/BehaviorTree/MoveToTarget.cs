using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace SPCharacterController
{
    /// <summary>
    /// 移动至目标点 - 每帧输出指向目标点的移动方向，进入到达判定距离后完成；
    /// 绑定冷却变量时，到达即开始巡逻冷却计时。
    /// </summary>
    [TaskCategory("SPCharacter")]
    [TaskDescription("输出指向目标点的移动方向直到到达，可选在到达时写入巡逻冷却。")]
    public class MoveToTarget : SPAIAction
    {
        [Tooltip("目标点（绑定共享变量 TargetPosition）")]
        public SharedVector3 TargetPosition;

        [Tooltip("到达判定距离")]
        public SharedFloat ArriveDistance = 0.5f;

        [Tooltip("输出：移动方向（绑定共享变量 OutMoveDirection）")]
        public SharedVector3 OutMoveDirection;

        [Tooltip("巡逻冷却秒数（可选绑定共享变量 PatrolCooldown，与 NextPatrolTime 同时绑定时生效）")]
        public SharedFloat PatrolCooldown;

        [Tooltip("输出：下次允许巡逻的时刻（可选绑定共享变量 NextPatrolTime）")]
        public SharedFloat NextPatrolTime;

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            Vector3 offset = TargetPosition.Value - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= ArriveDistance.Value * ArriveDistance.Value)
            {
                OutMoveDirection.Value = Vector3.zero;
                if (!NextPatrolTime.IsNone && !PatrolCooldown.IsNone)
                    NextPatrolTime.Value = Time.time + PatrolCooldown.Value;
                return TaskStatus.Success;
            }

            OutMoveDirection.Value = offset.normalized;
            return TaskStatus.Running;
        }

        /// <inheritdoc />
        public override void OnEnd()
        {
            OutMoveDirection.Value = Vector3.zero;
        }
    }
}
