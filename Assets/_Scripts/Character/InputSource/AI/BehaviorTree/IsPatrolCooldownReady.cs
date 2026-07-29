using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace SPCharacterController
{
    /// <summary>
    /// 巡逻冷却判断 - 当前时间到达下次允许巡逻的时刻时返回成功。
    /// </summary>
    [TaskCategory("SPCharacter")]
    [TaskDescription("巡逻冷却完毕时返回成功，否则返回失败。")]
    public class IsPatrolCooldownReady : SPAIConditional
    {
        [Tooltip("下次允许巡逻的时刻（绑定共享变量 NextPatrolTime）")]
        public SharedFloat NextPatrolTime;

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            return Time.time >= NextPatrolTime.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
