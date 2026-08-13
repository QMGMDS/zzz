using System;

using Animancer;

namespace SPCharacter.Core
{
    /// <summary>
    /// 角色动画播放指令源
    /// </summary>
    internal sealed class AnimationSource
    {
        private readonly AnimancerComponent _animancer;
        private AnimancerState _currentState;

        /// <summary>
        /// 创建 Animancer 动画指令源
        /// </summary>
        /// <param name="animancer">角色使用的 Animancer 组件</param>
        public AnimationSource(AnimancerComponent animancer)
        {
            if (animancer == null) throw new ArgumentNullException(nameof(animancer));
            if (animancer.Animator == null) throw new ArgumentException("AnimancerComponent 未设置 Animator。", nameof(animancer));
            if (animancer.Transitions == null) throw new ArgumentException("AnimancerComponent 未设置 Transition Library。", nameof(animancer));

            _animancer = animancer;
        }

        /// <summary>获取当前动画播放的归一化时刻</summary>
        public float CurrentNormalizedTime => _currentState != null ? _currentState.NormalizedTime : 0f;

        /// <summary>获取当前动画播放的时刻（秒）</summary>
        public float CurrentTime => _currentState != null ? _currentState.Time : 0f;

        /// <summary>
        /// 使用 Transition Library 的过渡规则播放动画
        /// </summary>
        /// <param name="transition">要播放的 Transition 资源</param>
        public void Play(TransitionAssetBase transition)
        {
            if (transition == null || !transition.IsValid)
                throw new ArgumentException("动画片段未设置有效的 Transition Asset", nameof(transition));

            _currentState = _animancer.Play(transition);
        }
    }
}
