using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// 处理后的输入数据——游戏逻辑真正使用的意愿快照。
    /// BufferTimer 采用充能+衰减机制：按下边沿时充至独立配置值，
    /// 随后逐帧衰减直至归零或被 Consume 显式归零。
    /// </summary>
    public struct ProcessedInputData
    {
        /// <summary>防抖处理后的移动方向</summary>
        public Vector2 Move;

        /// <summary>闪避缓存计时器（秒），大于 0 表示闪避意图有效</summary>
        public float EvadeBufferTimer;

        /// <summary>攻击缓存计时器（秒），大于 0 表示攻击意图有效</summary>
        public float AttackBufferTimer;

        /// <summary>闪避是否处于缓存窗口内</summary>
        public bool EvadePressed => EvadeBufferTimer > 0f;

        /// <summary>攻击是否处于缓存窗口内</summary>
        public bool AttackPressed => AttackBufferTimer > 0f;
    }
}
