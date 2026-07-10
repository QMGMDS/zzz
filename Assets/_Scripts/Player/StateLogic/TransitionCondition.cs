namespace SPPlayer
{
    /// <summary>
    /// 状态转移条件枚举——族内规则和跨族拦截器共用。
    /// </summary>
    public enum TransitionCondition
    {
        #region 基础条件

        /// <summary>进入节点后立即跳转</summary>
        Immediate,

        /// <summary>由 StateBehaviourSO 接管判断</summary>
        Custom,

        /// <summary>动画完成</summary>
        AnimationCompleted,

        /// <summary>有攻击意图</summary>
        WantToAttack,

        /// <summary>有闪避意图</summary>
        WantToEvade,

        /// <summary>有移动意图</summary>
        WantToMove,

        /// <summary>无移动意图</summary>
        NotWantToMove,

        /// <summary>移动方向与角色朝向相反</summary>
        MoveDirectionFlipped,

        #endregion

        #region 组合节点

        /// <summary>动画完成 + 移动意图</summary>
        AnimationCompleted_And_WantToMove,

        /// <summary>动画完成 + 无移动意图</summary>
        AnimationCompleted_And_NotWantToMove,

        #endregion
    }
}
