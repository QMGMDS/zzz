using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// WalkStart 状态
    /// </summary>
    public class WalkStartState : BaseState
    {
        /// <summary>
        /// 创建 WalkStart 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public WalkStartState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.WalkStart;

        /// <inheritdoc />
        protected override void OnEnter() { }

        private const float TransitionThreshold = 0.9f;

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.CurrentNormalizedTime >= TransitionThreshold
                && PlayerBrainBlackboard.WantToMove)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.WalkLoop));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
