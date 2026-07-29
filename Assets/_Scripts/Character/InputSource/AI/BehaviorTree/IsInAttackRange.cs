using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace SPCharacterController
{
    /// <summary>
    /// 攻击范围判断 - 玩家处于攻击范围内时返回成功。
    /// </summary>
    [TaskCategory("SPCharacter")]
    [TaskDescription("玩家在攻击范围内时返回成功，否则返回失败。")]
    public class IsInAttackRange : SPAIConditional
    {
        [Tooltip("攻击范围（绑定共享变量 AttackRange）")]
        public SharedFloat AttackRange;

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            Vector3 offset = PlayerTransform.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= AttackRange.Value * AttackRange.Value
                ? TaskStatus.Success
                : TaskStatus.Failure;
        }
    }
}
