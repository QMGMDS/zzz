using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace GamePlay.Enemy.BT
{
    /// <summary>
    /// 播放 Hit_Front 受击动画并等待播放完毕。期间若再次受击，由 Animator AnyState 自动重播，
    /// 本 Action 持续等待直至 normalizedTime 达到阈值。
    /// </summary>
    [TaskDescription("播放 Hit_Front 受击动画并等待播放完毕，播完后清除受击标记")]
    public class PlayHitAnimation : Action
    {
        private Animator _animator;
        private EnemyController _controller;

        public override void OnAwake()
        {
            _animator = GetComponent<Animator>();
            _controller = GetComponent<EnemyController>();
        }

        public override TaskStatus OnUpdate()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName("Hit_Front") && stateInfo.normalizedTime < 0.9f)
                return TaskStatus.Running;

            _controller.isHitRequested = false;
            return TaskStatus.Success;
        }
    }
}
