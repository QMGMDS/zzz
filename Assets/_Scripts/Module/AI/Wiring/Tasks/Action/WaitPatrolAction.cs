using UnityEngine;

using BehaviorDesigner.Runtime.Tasks;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 巡逻停留动作 - 到达巡逻点后原地停留配置秒数 计时满后结束交还树评估
    /// </summary>
    internal sealed class WaitPatrolAction : Action
    {
        private AIBrain _brain;
        private float _waitTimer;

        /// <inheritdoc />
        public override void OnAwake()
        {
            _brain = GetComponent<AIBrain>();
        }

        /// <inheritdoc />
        public override void OnStart()
        {
            _waitTimer = 0f;
        }

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            if (_brain == null)
                return TaskStatus.Failure;

            // 不写入移动轴即停走 计时满后结束本次停留
            _waitTimer += Time.deltaTime;
            return _waitTimer >= _brain.Config.PatrolWaitSeconds ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}
