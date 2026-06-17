namespace SPPlayer
{
    /// <summary>
    /// 玩家状态类型枚举——定义了所有可用的逻辑状态。
    /// 新增状态时在此枚举添加值。
    /// </summary>
    public enum PlayerStateType
    {
        // 静止族状态
        Idle,
        IdleAFK,
        Stop,

        // 行走族状态
        WalkStart,
        WalkLoop,

        // 奔跑族状态
        RunStart,
        RunLoop,
        RunTurn,

        // 闪避族状态
        Evade,

        // 攻击族状态
        Attack,
    }
}
