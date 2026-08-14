using SPFramework.Event;

namespace SPCharacter.Contract
{
    /// <summary>
    /// 角色切换完成事件 - 由角色模块发布 供队伍模块订阅
    /// </summary>
    public static class CharacterSwitchEvents
    {
        /// <summary>上场动画完成事件</summary>
        public static readonly EventKey<CharacterSwitchInCompletedEvent> SwitchInCompleted =
            new EventKey<CharacterSwitchInCompletedEvent>("Character.Switch.SwitchInCompleted");

        /// <summary>退场动画完成事件</summary>
        public static readonly EventKey<CharacterSwitchOutCompletedEvent> SwitchOutCompleted =
            new EventKey<CharacterSwitchOutCompletedEvent>("Character.Switch.SwitchOutCompleted");

        /// <summary>角色上场位姿已应用事件</summary>
        public static readonly EventKey<CharacterSwitchInPoseAppliedEvent> SwitchInPoseApplied =
            new EventKey<CharacterSwitchInPoseAppliedEvent>("Character.Switch.SwitchInPoseApplied");
    }

    /// <summary>
    /// 角色上场完成事件负载
    /// </summary>
    public readonly struct CharacterSwitchInCompletedEvent
    {
        /// <summary>
        /// 创建上场完成事件
        /// </summary>
        /// <param name="characterId">完成上场的角色 Id</param>
        public CharacterSwitchInCompletedEvent(string characterId)
        {
            CharacterId = characterId;
        }

        /// <summary>完成上场的角色 Id</summary>
        public string CharacterId { get; }
    }

    /// <summary>
    /// 角色退场完成事件负载
    /// </summary>
    public readonly struct CharacterSwitchOutCompletedEvent
    {
        /// <summary>
        /// 创建退场完成事件
        /// </summary>
        /// <param name="characterId">完成退场的角色 Id</param>
        public CharacterSwitchOutCompletedEvent(string characterId)
        {
            CharacterId = characterId;
        }

        /// <summary>完成退场的角色 Id</summary>
        public string CharacterId { get; }
    }

    /// <summary>
    /// 角色上场位姿已应用事件负载
    /// </summary>
    public readonly struct CharacterSwitchInPoseAppliedEvent
    {
        /// <summary>
        /// 创建角色上场位姿已应用事件
        /// </summary>
        /// <param name="characterId">完成落位的角色 Id</param>
        public CharacterSwitchInPoseAppliedEvent(string characterId)
        {
            CharacterId = characterId;
        }

        /// <summary>完成落位的角色 Id</summary>
        public string CharacterId { get; }
    }
}
