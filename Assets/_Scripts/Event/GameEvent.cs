using System;

namespace SPEvent
{
    /// <summary>
    /// 轻量全局事件中心 - 跨模块解耦的消息通道，各模块只依赖此类，互不知晓对方存在。
    /// </summary>
    public static class GameEvent
    {
        /// <summary>
        /// 角色切换事件 - 参数为新激活角色的索引。
        /// </summary>
        public static event Action<int> CharacterSwitched;

        /// <summary>
        /// 触发角色切换事件。
        /// </summary>
        /// <param name="newIndex">新激活角色的索引</param>
        public static void OnCharacterSwitched(int newIndex) => CharacterSwitched?.Invoke(newIndex);
    }
}
