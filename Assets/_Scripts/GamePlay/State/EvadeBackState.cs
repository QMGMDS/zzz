namespace GamePlay.State
{
    /// <summary>
    /// 后撤步状态，继承 EvadeState 并指定方向为 Back。
    /// 仅作为类型标记存在，使状态机字典能以 Type 区分前闪避与后撤步。
    /// </summary>
    public class EvadeBackState : EvadeState
    {
        public EvadeBackState() : base(EvadeType.Back) { }
    }
}
