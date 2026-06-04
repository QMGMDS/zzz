namespace GamePlay.State
{
    /// <summary>
    /// 前闪避状态，继承 EvadeState 并指定方向为 Front。
    /// 仅作为类型标记存在，使状态机字典能以 Type 区分前闪避与后撤步。
    /// </summary>
    public class EvadeFrontState : EvadeState
    {
        public EvadeFrontState() : base(EvadeType.Front) { }
    }
}
