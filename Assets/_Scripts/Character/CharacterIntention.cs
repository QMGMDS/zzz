using System;

namespace SPCharacterController
{
    /// <summary>
    /// 统一意图定义 - 所有角色类型的原子条件集中管理。
    /// 各角色类型的转移规则只引用自己分区的值，但枚举本身是全局共享的。
    /// </summary>
    [Flags]
    public enum CharacterIntention : uint
    {
        None = 0,

        // ═══════════════════════════════════════
        //  公共意图 0 ~ 6
        // ═══════════════════════════════════════
        AnimationCompleted = 1 << 0,

        // ═══════════════════════════════════════
        //  玩家独有意图 7 ~ 
        // ═══════════════════════════════════════
        WantToMove = 1 << 7,
        NotWantToMove = 1 << 8,
        WantToEvade = 1 << 9,
        WantToAttack = 1 << 10,
        WantToTurn = 1 << 11,

        // ═══════════════════════════════════════
        //  怪物独有意图
        // ═══════════════════════════════════════
    }
}
