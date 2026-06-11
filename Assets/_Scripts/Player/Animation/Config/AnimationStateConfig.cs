using System;
using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 单个动画状态的播放配置——定义动画片段、淡入时间、播放速度等。
    /// </summary>
    [Serializable]
    public struct AnimationStateConfig
    {
        [Tooltip("待播放的 AnimationClip")]
        public AnimationClip Clip;

        [Tooltip("淡入过渡时长（秒），0 为瞬间切换")]
        [Min(0f)]
        public float FadeDuration;

        [Tooltip("动画播放速度倍率，1 为正常速度")]
        [Min(0f)]
        public float Speed;

        [Tooltip("是否为循环动画（Idle/MoveLoop 等）")]
        public bool IsLooping;
    }

    /// <summary>
    /// 状态→动画绑定条目——将逻辑状态枚举与动画配置配对，供 ScriptableObject 序列化。
    /// </summary>
    [Serializable]
    public struct AnimationStateBinding
    {
        [Tooltip("逻辑状态类型")]
        public PlayerStateType StateType;

        [Tooltip("该状态对应的动画播放配置")]
        public AnimationStateConfig Config;
    }
}
