using UnityEngine;

namespace GamePlay.Common
{
    /// <summary>
    /// 动画参数哈希值集中管理，避免多处重复定义
    /// </summary>
    public static class AnimationHashes
    {
        /// <summary>Idle 动画状态名</summary>
        public static readonly int Idle = Animator.StringToHash("Idle");

        /// <summary>Walk 动画状态名</summary>
        public static readonly int Walk = Animator.StringToHash("Walk");

        /// <summary>WalkStart 动画状态名</summary>
        public static readonly int WalkStart = Animator.StringToHash("WalkStart");

        /// <summary>Run 动画状态名</summary>
        public static readonly int Run = Animator.StringToHash("Run");

        /// <summary>RunStart 动画状态名</summary>
        public static readonly int RunStart = Animator.StringToHash("RunStart");

        /// <summary>EvadeFront 动画状态名</summary>
        public static readonly int EvadeFront = Animator.StringToHash("EvadeFront");

        /// <summary>EvadeBack 动画状态名</summary>
        public static readonly int EvadeBack = Animator.StringToHash("EvadeBack");

        /// <summary>RunEnd 动画状态名</summary>
        public static readonly int RunEnd = Animator.StringToHash("RunEnd");
    }
}
