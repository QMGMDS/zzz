using System;
using UnityEngine;

namespace SPCharacter.Contract
{
    /// <summary>
    /// 控制器原子意图定义，供状态转移规则组合判断。
    /// </summary>
    [Flags]
    public enum CharacterIntention : uint
    {
        None = 0,

        AnimationCompleted = 1 << 0,
        WantToMove = 1 << 1,
        WantToAttack = 1 << 2,
        WantToHoldAttack = 1 << 3,
        WantToEvade = 1 << 4,
        WantToTurn = 1 << 5,
    }

    /// <summary>
    /// 单帧意图快照 - 外部意图源产出的纯数据契约。
    /// </summary>
    public struct CharacterIntentionFrame
    {
        /// <summary>本帧角色目标方向，XY 分量分别对应世界 XZ 轴。</summary>
        public Vector2 MoveAxis { get; init; }

        /// <summary>本帧原子意图位掩码（移动/攻击/闪避/转向等）</summary>
        public CharacterIntention Flags { get; init; }
    }
}
