using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using SPAI.Core;

namespace SPAI.Wiring.Tasks
{
    /// <summary>
    /// 等待最后目击位置动作 - 抵达最后目击位置后等待配置时长
    /// </summary>
    internal sealed class WaitForLastSeenTargetAction : Action
    {
        private AIBrain _brain;
        private AIRuntimeBlackboard _blackboard;
        private float _waitTimer;

        /// <inheritdoc />
        public override void OnAwake()
        {
            _brain = GetComponent<AIBrain>();
            if (_brain != null)
            {
                _blackboard = _brain.Blackboard;
            }
        }

        /// <inheritdoc />
        public override void OnStart()
        {
            _waitTimer = 0f;
        }

        /// <inheritdoc />
        public override void OnReset()
        {
            _waitTimer = 0f;
        }

        /// <inheritdoc />
        public override TaskStatus OnUpdate()
        {
            if (_brain == null || _blackboard == null || !_blackboard.HasTarget || _blackboard.IsTargetVisible)
            {
                return TaskStatus.Failure;
            }

            _waitTimer += Time.deltaTime;
            return _waitTimer >= _brain.Config.LastSeenWaitSeconds
                ? TaskStatus.Success
                : TaskStatus.Running;
        }
    }
}
