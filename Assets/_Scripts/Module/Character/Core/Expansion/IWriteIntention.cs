using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 写入意图行为 - 限定胶水扩展只能在统一意图窗口写入外部控制意图
    /// 将意图写入角色黑板中
    /// </summary>
    internal interface IWriteIntention
    {
        /// <summary>写入本帧移动方向</summary>
        void SetMoveAxis(Vector2 moveAxis);

        /// <summary>设置或清除本帧控制意图</summary>
        void SetIntention(CCIntention intention, bool value);
    }
}
