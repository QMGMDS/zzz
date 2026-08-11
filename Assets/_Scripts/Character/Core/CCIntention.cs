using System;

namespace SPCharacter.Core
{
    /// <summary>
    /// 控制器原子意图定义，供状态转移规则组合判断
    /// </summary>
    [Flags]
    internal enum CCIntention : uint
    {
        None = 0,

        AnimationCompleted = 1 << 0,
        WantToMove = 1 << 1,
        WantToAttack = 1 << 2,
        WantToHoldAttack = 1 << 3,
        WantToEvade = 1 << 4,
        WantToTurn = 1 << 5,
        WantToSwitchIn = 1 << 6,
        WantToSwitchOut = 1 << 7,
    }
}
