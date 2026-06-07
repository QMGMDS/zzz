using UnityEngine;

namespace Core.Input.Data
{
    /// <summary>
    /// 处理后的输入数据——游戏逻辑使用的本帧意愿快照，不含后处理内部状态。
    /// </summary>
    public struct ProcessedInputData
    {
        /// <summary>防抖处理后的移动方向</summary>
        public Vector2 Move;

        /// <summary>攻击是否处于缓存窗口内</summary>
        public bool AttackPressed;

        /// <summary>闪避是否处于缓存窗口内</summary>
        public bool EvadePressed;
    }
}
