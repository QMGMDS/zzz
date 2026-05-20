using UnityEngine;

namespace GamePlay.Common
{
    /// <summary>
    /// 动画参数哈希值集中管理，避免多处重复定义
    /// </summary>
    public static class AnimationHashes
    {
        /// <summary>是否有输入</summary>
        public static readonly int HasInput = Animator.StringToHash("HasInput");
    }
}
