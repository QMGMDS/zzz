using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// 意图黑板——MainProcessorPipeline 将处理后输入翻译为离散角色意图后写入此处。
    /// 后续状态机/拦截器等系统从此读取意图，无需直接接触原始输入数据。
    /// 每帧由 Pipeline 重新覆盖写入，未被消费的意图不会跨帧残留。
    /// </summary>
    public class IntentionBlackboard
    {
        /// <summary>移动意图方向（摄像机空间归一化 Vector2）</summary>
        public Vector2 MoveDirection { get; set; }

        /// <summary>攻击意图标记</summary>
        public bool WantToAttack { get; set; }

        /// <summary>闪避意图标记</summary>
        public bool WantToEvade { get; set; }
    }
}
