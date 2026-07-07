using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// Attack_3 状态
    /// </summary>
    public class Attack_3 : AttackBaseState
    {
        private const float HoldThreshold = 0.1f;

        private float _heldStartTime = -1f;

        /// <summary>
        /// 创建 Attack_3 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_3(PlayerController player) : base(player) { }

        #region BaseState

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_3;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _heldStartTime = -1f;
        }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (PlayerBrainBlackboard.AttackHeld && _heldStartTime < 0f)
                _heldStartTime = Time.time;
            else if (!PlayerBrainBlackboard.AttackHeld)
                _heldStartTime = -1f;

            if (PlayerBrainBlackboard.WantToAttack && PlayerBrainBlackboard.CurrentNormalizedTime >= AttackCancelThreshold)
            {
                if (PlayerBrainBlackboard.AttackHeld && _heldStartTime >= 0f && Time.time - _heldStartTime >= HoldThreshold)
                {
                    _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_4_Prefect));
                    return;
                }

                if (!PlayerBrainBlackboard.AttackHeld)
                {
                    _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_4_Normal));
                    return;
                }

                return;
            }

            if (PlayerBrainBlackboard.AnimationCompleted)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_3_End));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }

        #endregion

        #region AttackBaseState

        /// <inheritdoc />
        protected override float AttackCancelThreshold => 0.5f;

        #endregion
    }
}
