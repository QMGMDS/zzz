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

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            // 族内过渡：WalkStart -> WalkLoop
            if (PlayerBrainBlackboard.AnimationCompleted && PlayerBrainBlackboard.WantToMove)
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
