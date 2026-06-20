using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// RunTurn 状态
    /// </summary>
    public class RunTurnState : BaseState
    {
        /// <summary>
        /// 创建 RunTurn 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public RunTurnState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.RunTurn;

        /// <inheritdoc />
        protected override void OnEnter() { }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            // 族内过渡：RunTurn -> RunLoop
            if (PlayerBrainBlackboard.AnimationCompleted && PlayerBrainBlackboard.WantToMove)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.RunLoop));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit()
        {
            // 旋转补偿
            var direction = PlayerBrainBlackboard.CurrentMoveDirection;
            if (direction.sqrMagnitude > 0.0001f)
            {
                _player.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}
