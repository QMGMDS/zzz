using SPFramework.Event;

namespace SPCharacter.Contract
{
    /// <summary>
    /// 角色事件 - 由角色模块发布的事实广播
    /// </summary>
    public static class CharacterEvents
    {
        /// <summary>角色上场位姿已应用事件</summary>
        public static readonly EventKey<CharacterSwitchInPoseAppliedEvent> SwitchInPoseApplied =
            new EventKey<CharacterSwitchInPoseAppliedEvent>("Character.SwitchInPoseApplied");

        /// <summary>角色上场动画完成事件</summary>
        public static readonly EventKey<CharacterSwitchInCompletedEvent> SwitchInCompleted =
            new EventKey<CharacterSwitchInCompletedEvent>("Character.SwitchInCompleted");

        /// <summary>角色退场动画完成事件</summary>
        public static readonly EventKey<CharacterSwitchOutCompletedEvent> SwitchOutCompleted =
            new EventKey<CharacterSwitchOutCompletedEvent>("Character.SwitchOutCompleted");
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
        public CharacterSwitchInPoseAppliedEvent(string characterId) => CharacterId = characterId;

        /// <summary>完成落位的角色 Id</summary>
        public string CharacterId { get; }
    }

    /// <summary>
    /// 角色上场动画完成事件负载
    /// </summary>
    public readonly struct CharacterSwitchInCompletedEvent
    {
        /// <summary>
        /// 创建角色上场动画完成事件
        /// </summary>
        /// <param name="characterId">完成上场的角色 Id</param>
        public CharacterSwitchInCompletedEvent(string characterId) => CharacterId = characterId;

        /// <summary>完成上场的角色 Id</summary>
        public string CharacterId { get; }
    }

    /// <summary>
    /// 角色退场动画完成事件负载
    /// </summary>
    public readonly struct CharacterSwitchOutCompletedEvent
    {
        /// <summary>
        /// 创建角色退场动画完成事件
        /// </summary>
        /// <param name="characterId">完成退场的角色 Id</param>
        public CharacterSwitchOutCompletedEvent(string characterId) => CharacterId = characterId;

        /// <summary>完成退场的角色 Id</summary>
        public string CharacterId { get; }
    }
}
