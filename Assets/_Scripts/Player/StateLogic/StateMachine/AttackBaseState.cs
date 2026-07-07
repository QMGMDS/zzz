namespace SPPlayer
{
    /// <summary>
    /// 攻击状态（非 End）的中间基类，承载攻击链提前取消体系。
    /// </summary>
    public abstract class AttackBaseState : BaseState
    {
        /// <summary>
        /// 创建攻击状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        protected AttackBaseState(PlayerController player) : base(player) { }

        /// <summary>
        /// 允许被下一段攻击提前打断的归一化时间阈值。
        /// 返回大于等于 1 的值表示不允许提前打断。
        /// </summary>
        protected virtual float AttackCancelThreshold => 1f;

        /// <summary>
        /// 提前取消后转移到的目标攻击状态类型。
        /// 返回 null 表示不启用提前取消。
        /// </summary>
        protected virtual PlayerStateType? CancelTargetStateType => null;

        /// <summary>
        /// 若满足提前取消条件（有攻击意图 + 动画进度超过阈值），则直接切换到下一攻击状态。
        /// </summary>
        /// <returns>是否触发了提前取消</returns>
        protected bool TryEarlyCancel()
        {
            if (CancelTargetStateType == null) return false;
            if (!PlayerBrainBlackboard.WantToAttack) return false;
            if (PlayerBrainBlackboard.CurrentNormalizedTime < AttackCancelThreshold) return false;

            _player.StateMachine.ChangeState(_player.StateMachine.GetState(CancelTargetStateType.Value));
            return true;
        }
    }
}
