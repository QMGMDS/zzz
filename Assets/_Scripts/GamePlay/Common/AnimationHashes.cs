using UnityEngine;

namespace GamePlay.Common
{
    /// <summary>
    /// 动画参数哈希值集中管理，避免多处重复定义
    /// </summary>
    public static class AnimationHashes
    {
        #region Movement Anim

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

        #endregion

        #region Attack Anim

        /// <summary>NormalAttack_1 动画状态名</summary>
        public static readonly int NormalAttack1 = Animator.StringToHash("NormalAttack_1");

        /// <summary>NormalAttack_2 动画状态名</summary>
        public static readonly int NormalAttack2 = Animator.StringToHash("NormalAttack_2");

        /// <summary>NormalAttack_3 动画状态名</summary>
        public static readonly int NormalAttack3 = Animator.StringToHash("NormalAttack_3");

        /// <summary>NormalAttack_4 动画状态名</summary>
        public static readonly int NormalAttack4 = Animator.StringToHash("NormalAttack_4");

        /// <summary>NormalAttack_1_End 动画状态名</summary>
        public static readonly int NormalAttack1End = Animator.StringToHash("NormalAttack_1_End");

        /// <summary>NormalAttack_2_End 动画状态名</summary>
        public static readonly int NormalAttack2End = Animator.StringToHash("NormalAttack_2_End");

        /// <summary>NormalAttack_3_End 动画状态名</summary>
        public static readonly int NormalAttack3End = Animator.StringToHash("NormalAttack_3_End");

        /// <summary>NormalAttack_4_End 动画状态名</summary>
        public static readonly int NormalAttack4End = Animator.StringToHash("NormalAttack_4_End");

        #endregion
    }
}
