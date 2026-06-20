using UnityEngine;
using Animancer;

namespace SPPlayer
{
    /// <summary>
    /// 动画源——通过 Animancer 插件实现动画的自由操纵
    /// </summary>
    public class AnimationSource
    {
        private readonly AnimancerComponent _animancer;

        /// <summary>
        /// 创建动画源
        /// </summary>
        /// <param name="animancer">Animancer 动画引擎组件</param>
        public AnimationSource(AnimancerComponent animancer)
        {
            _animancer = animancer;
        }

        /// <summary>获取当前动画播放的归一化时刻 (0~1)</summary>
        public float CurrentNormalizedTime
        {
            get
            {
                if (_animancer == null) return 0f;
                var state = _animancer.States.Current;
                return state?.NormalizedTime ?? 0f;
            }
        }

        /// <summary>获取当前动画播放的时刻（秒）</summary>
        public float CurrentTime
        {
            get
            {
                if (_animancer == null) return 0f;
                var state = _animancer.States.Current;
                return state?.Time ?? 0f;
            }
        }

        /// <summary>
        /// 播放动画过渡
        /// </summary>
        /// <param name="transition">Animancer 过渡数据</param>
        public void Play(ITransition transition)
        {
            if (_animancer == null || transition == null || !transition.IsValid()) return;

            _animancer.Play(transition);
        }
    }
}
