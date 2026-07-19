using System;
using Animancer;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 唯一对接动画系统的指令源 - 确保能自由更换动画系统
    /// </summary>
    public class AnimationSource
    {
        private readonly AnimancerComponent _animancer;
        private AnimancerState _currentState;

        /// <summary>
        /// 创建 Animancer 动画指令源。
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
        /// 使用 Transition Library 的过渡规则播放动画。
        /// </summary>
        /// <param name="animation">要播放的动画片段</param>
        public void Play(SPAnimClip animation)
        {
            if (!animation.IsValid) throw new ArgumentException("动画片段未设置有效的 Transition Asset。", nameof(animation));

            _currentState = _animancer.Play(animation.Transition);
        }
    }

    /// <summary>
    /// 自定义类型的动画片段
    /// 隔离状态节点与具体动画系统使用的 Transition Asset 类型。
    /// </summary>
    [Serializable]
    public struct SPAnimClip
    {
        [Tooltip("Animancer Transition Asset")]
        public TransitionAssetBase Transition;

        /// <summary>动画引用是否有效</summary>
        public bool IsValid => Transition != null && Transition.IsValid;
    }
}
