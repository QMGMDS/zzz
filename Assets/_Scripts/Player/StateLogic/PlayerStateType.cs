namespace SPPlayer
{
    /// <summary>
    /// 玩家状态类型枚举，定义了所有可用的逻辑状态。
    /// 新增状态时在此枚举添加值。
    /// </summary>
    public enum PlayerStateType
    {
        // 静止族成员
        Idle,
        IdleAFK,
        Stop,

        // 行走族成员
        WalkStart,
        WalkLoop,

        // 奔跑族成员
        RunStart,
        RunLoop,
        RunTurn,

        // 闪避族成员
        EvadeFront,
        EvadeFrontEnd,
        EvadeBack,
        EvadeBackEnd,

        // 攻击族成员
        Attack_1,
        Attack_1_End,
        Attack_2,
        Attack_2_End,
        Attack_3,
        Attack_3_End,
        Attack_4_Normal,
        Attack_4_Normal_End,
    }
}
