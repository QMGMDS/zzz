using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// RunTurn 状态
    /// </summary>
    public class RunTurnState : BaseState
    {
        private Vector3 _enteredMoveDirection;

        /// <summary>
        /// 创建 RunTurn 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public RunTurnState(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.RunTurn;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _enteredMoveDirection = PlayerBrainBlackboard.CurrentMoveDirection;
        }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            /* 这里不判断是否输入的说明 DA☆ZE
                原本有输入 RunTurn -> RunLoop，无输入 RunTurn -> Stop
                由于 RunTurn 动画的特殊性，如果无输入 RunTurn -> Stop，会造成不自然的跳变，于是用 RunLoop 做中间态衔接
                无输入 RunTurn -> RunLoop -> Stop
                有输入 Runturn -> RunLoop
            */
            // 族内过渡：RunTurn -> RunLoop
            if (PlayerBrainBlackboard.AnimationCompleted)
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
            else
            {
                _player.transform.rotation = Quaternion.LookRotation(_enteredMoveDirection, Vector3.up);
            }
        }
    }
}
