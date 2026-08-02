using UnityEngine;

namespace SPInput_Contract
{
    /// <summary>
    /// 帧原始输入 - 纯硬件事实汇报，绝无任何手感处理。
    /// 作为值类型数据契约，构造期定稿后只读，下游不可修改。
    /// </summary>
    public struct FrameRawInput
    {
        /// <summary>记录本帧是哪一帧</summary>
        public ulong FrameIndex { get; init; }

        /// <summary>本帧 WASD 移动轴数据</summary>
        public Vector2 MoveAxisValue { get; init; }

        /// <summary>本帧攻击键是否被按下</summary>
        public bool IsAttackPressed { get; init; }

        /// <summary>本帧闪避键是否被按下</summary>
        public bool IsEvadePressed { get; init; }

        /// <summary>本帧技能键是否被按下</summary>
        public bool IsSkillPressed { get; init; }

        /// <summary>本帧切换角色键是否被按下</summary>
        public bool IsSwitchCharacterPressed { get; init; }

        /// <summary>本帧大招键是否被按下</summary>
        public bool IsUltimatePressed { get; init; }
    }
}