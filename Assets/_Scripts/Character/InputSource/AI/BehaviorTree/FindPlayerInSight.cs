using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace SPCharacterController
{
    /// <summary>
    /// 视野索敌 - 玩家处于视野范围内时写入目标点，刚丢失视野时推迟下一次巡逻。
    /// </summary>
    [TaskCategory("SPCharacter")]
    [TaskDescription("玩家在视野范围内时写入其世界坐标；刚丢失视野时启动巡逻等待。")]
    public class FindPlayerInSight : SPAIConditional
    {
        [Tooltip("视野范围（绑定共享变量 SightRange）")]
        public SharedFloat SightRange;

        [Tooltip("丢失视野后等待巡逻的秒数（绑定共享变量 LostSightDelay）")]
        public SharedFloat LostSightDelay;

        [Tooltip("输出：下次允许巡逻的时刻（绑定共享变量 NextPatrolTime）")]
        public SharedFloat NextPatrolTime;

        [Tooltip("输出：目标点（绑定共享变量 TargetPosition）")]
        public SharedVector3 TargetPosition;

        private bool _wasPlayerVisible;

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            Vector3 playerPosition = PlayerTransform.position;
            Vector3 offset = playerPosition - transform.position;
            offset.y = 0f;

            bool isPlayerVisible = offset.sqrMagnitude <= SightRange.Value * SightRange.Value;
            if (!isPlayerVisible)
            {
                if (_wasPlayerVisible)
                    NextPatrolTime.Value = Time.time + LostSightDelay.Value;
                _wasPlayerVisible = false;
                return TaskStatus.Failure;
            }

            _wasPlayerVisible = true;
            TargetPosition.Value = playerPosition;
            return TaskStatus.Success;
        }
    }
}
