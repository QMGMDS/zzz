using UnityEngine;

namespace SPInput.Contract
{
    /// <summary>
    /// 帧后处理输入 - 输入模块对原始硬件数据做手感后处理后的特供数据。
    /// 作为值类型数据契约，构造期定稿后只读，下游不可修改。
    /// </summary>
    public struct ProcessedFrameInput
    {
        /// <summary>记录本帧是哪一帧</summary>
        public ulong FrameIndex { get; init; }

        /// <summary>本帧攻击键后处理状态（按下/长按）</summary>
        public ButtonInputState Attack { get; init; }

        /// <summary>本帧闪避键后处理状态（按下/长按）</summary>
        public ButtonInputState Evade { get; init; }

        /// <summary>本帧技能键后处理状态（按下/长按）</summary>
        public ButtonInputState Skill { get; init; }

        /// <summary>本帧切换角色键后处理状态（按下/长按）</summary>
        public ButtonInputState SwitchCharacter { get; init; }

        /// <summary>本帧大招键后处理状态（按下/长按）</summary>
        public ButtonInputState Ultimate { get; init; }

        /// <summary>本帧移动轴延时缓冲后并归一化的单位方向向量；无输入时为零向量</summary>
        public Vector2 MoveDirection { get; init; }

        /// <summary>本帧是否存在有效移动输入（非零或处于归零缓冲期内）</summary>
        public bool HasMoveInput { get; init; }
    }

    /// <summary>
    /// 单个按键的后处理状态 - "被按下"为本帧按下边沿，"被长按"为持续按压时长超过阈值。
    /// </summary>
    public struct ButtonInputState
    {
        /// <summary>本帧被按下（按下边沿，与原始 FrameRawInput 同源）</summary>
        public bool IsPressed { get; init; }

        /// <summary>被长按 - 持续按压时长已超过长按判定阈值，松开即失效并复位</summary>
        public bool IsHeld { get; init; }
    }
}
