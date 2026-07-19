using UnityEngine;

namespace SPPlayerInput
{
    /// <summary>
    /// 帧原始数据，纯硬件事实汇报，绝无任何手感处理
    /// </summary>
    public struct FrameRawInputData
    {
        /// <summary>记录本帧是哪一帧</summary>
        public ulong FrameIndex;

        /// <summary>本帧 WASD 移动轴数据</summary>
        public Vector2 MoveAxisValue;
        /// <summary>本帧攻击键是否被按下</summary>
        public bool AttackPressed;
        /// <summary>本帧闪避键是否被按下</summary>
        public bool EvadePressed;
        /// <summary>本帧技能键是否被按下</summary>
        public bool SkillPressed;
        /// <summary>本帧切换角色键是否被按下</summary>
        public bool SwitchCharacterPressed;
        /// <summary>本帧大招键是否被按下</summary>
        public bool UltimatePressed;
    }
}
