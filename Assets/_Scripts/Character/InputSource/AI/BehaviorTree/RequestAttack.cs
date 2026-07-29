using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using TooltipAttribute = UnityEngine.TooltipAttribute;

namespace SPCharacterController
{
    /// <summary>
    /// 请求攻击 - 输出攻击意图，由状态机切换到攻击状态。
    /// </summary>
    [TaskCategory("SPCharacter")]
    [TaskDescription("输出攻击意图，由状态机切换到攻击状态。")]
    public class RequestAttack : SPAIAction
    {
        [Tooltip("输出：攻击意图（绑定共享变量 OutWantToAttack）")]
        public SharedBool OutWantToAttack;

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            OutWantToAttack.Value = true;
            return TaskStatus.Success;
        }
    }
}
