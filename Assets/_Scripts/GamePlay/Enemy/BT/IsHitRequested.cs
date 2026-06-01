using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace GamePlay.Enemy.BT
{
    /// <summary>
    /// 检测敌人是否受击（isHitRequested == true），供行为树 Selector 条件分支使用
    /// </summary>
    [TaskDescription("检测敌人 isHitRequested 是否为 true")]
    public class IsHitRequested : Conditional
    {
        private EnemyController _controller;

        public override void OnAwake()
        {
            _controller = GetComponent<EnemyController>();
        }

        public override TaskStatus OnUpdate()
        {
            return _controller.isHitRequested ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
