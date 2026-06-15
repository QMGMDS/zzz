namespace SPPlayer
{
    /// <summary>
    /// 玩家状态类型枚举——定义了所有可用的逻辑状态。
    /// 新增状态时在此枚举添加值。
    /// </summary>
    public enum PlayerStateType
    {
        Idle,
        IdleAFK,
        MoveStart,
        MoveLoop,
        Stop,
        Evade,
        Attack,
    }
}
