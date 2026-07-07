using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 原始输入数据——纯硬件事实汇报，不包含任何手感处理。
    /// </summary>
    public struct RawInputData
    {
        /// <summary>WASD 横向输入轴</summary>
        public Vector2 MoveAxis;

        /// <summary>攻击按键按下边沿触发</summary>
        public bool AttackJustPressed;

        /// <summary>闪避/冲刺按键按下边沿触发</summary>
        public bool EvadeJustPressed;

        /// <summary>攻击按键本帧是否持续按住（非边沿信号）</summary>
        public bool AttackHeld;
    }

    /// <summary>
    /// 后处理数据——游戏逻辑使用的本帧意愿快照。
    /// </summary>
    public struct ProcessedInputData
    {
        /// <summary>防抖处理后的移动方向</summary>
        public Vector2 Move;

        /// <summary>攻击是否处于缓存窗口内</summary>
        public bool AttackPressed;

        /// <summary>闪避是否处于缓存窗口内</summary>
        public bool EvadePressed;

        /// <summary>攻击按键本帧是否持续按住（不经 buffer 处理）</summary>
        public bool AttackHeld;
    }

    /// <summary>
    /// 单帧输入快照——持有原始数据与处理后数据的完整副本。
    /// </summary>
    public struct FrameInputData
    {
        /// <summary>物理帧计数器</summary>
        public ulong FrameIndex;

        /// <summary>本帧原始硬件数据</summary>
        public RawInputData Raw;

        /// <summary>本帧处理后数据</summary>
        public ProcessedInputData Processed;
    }

    /// <summary>
    /// 堆内存输入数据容器——由 InputCollector 写入，外部系统通过只读引用读取。
    /// 持有当前帧与上一帧的快照，支持帧级差分分析。
    /// </summary>
    public class InputData
    {
        /// <summary>当前帧输入快照</summary>
        public FrameInputData CurrentFrameData;

        /// <summary>上一帧输入快照</summary>
        public FrameInputData LastFrameData;
    }
}
