using SPFramework.Event;

namespace SPTeam.Contract
{
    /// <summary>
    /// 队伍事件 - 由队伍模块发布的事实广播
    /// </summary>
    public static class TeamEvents
    {
        /// <summary>当前上场角色变化事件</summary>
        public static readonly EventKey<TeamActiveCharacterChangedEvent> ActiveCharacterChanged =
            new EventKey<TeamActiveCharacterChangedEvent>("Team.ActiveCharacterChanged");

        /// <summary>队伍切换锁状态变化事件</summary>
        public static readonly EventKey<TeamSwitchLockChangedEvent> SwitchLockChanged =
            new EventKey<TeamSwitchLockChangedEvent>("Team.SwitchLockChanged");
    }

    /// <summary>
    /// 当前上场角色变化事件负载
    /// </summary>
    public readonly struct TeamActiveCharacterChangedEvent
    {
        /// <summary>
        /// 创建当前上场角色变化事件
        /// </summary>
        /// <param name="previousCharacterId">切换前的角色 Id</param>
        /// <param name="currentCharacterId">切换后的角色 Id</param>
        public TeamActiveCharacterChangedEvent(string previousCharacterId, string currentCharacterId)
        {
            PreviousCharacterId = previousCharacterId;
            CurrentCharacterId = currentCharacterId;
        }

        /// <summary>切换前的角色 Id</summary>
        public string PreviousCharacterId { get; }

        /// <summary>切换后的角色 Id</summary>
        public string CurrentCharacterId { get; }
    }

    /// <summary>
    /// 队伍切换锁状态变化事件负载
    /// </summary>
    public readonly struct TeamSwitchLockChangedEvent
    {
        /// <summary>
        /// 创建切换锁状态变化事件
        /// </summary>
        /// <param name="isOperationLocked">是否锁定玩家操作</param>
        /// <param name="isSwitchLocked">是否处于切换中</param>
        public TeamSwitchLockChangedEvent(bool isOperationLocked, bool isSwitchLocked)
        {
            IsOperationLocked = isOperationLocked;
            IsSwitchLocked = isSwitchLocked;
        }

        /// <summary>是否锁定玩家操作</summary>
        public bool IsOperationLocked { get; }

        /// <summary>是否处于切换中</summary>
        public bool IsSwitchLocked { get; }
    }
}