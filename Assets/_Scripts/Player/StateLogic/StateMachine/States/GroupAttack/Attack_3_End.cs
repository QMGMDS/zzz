using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// Attack_3_End 状态——第三段攻击收尾，根据玩家按住时长决定第四段攻击路线。
    /// 短按（释放快于 Hold 阈值）→ Attack_4_Normal
    /// 长按（按住超过 Hold 阈值）→ Attack_4_Prefect
    /// </summary>
    public class Attack_3_End : BaseState
    {
        /// <summary>长按判定阈值（秒）</summary>
        private const float HoldThreshold = 0.15f;

        /// <summary>本状态内首次检测到按住攻击的时间戳，-1 表示尚未按下</summary>
        private float _holdStartTime = -1f;

        /// <summary>是否已通过 WantToAttack 边沿信号确认本状态内发生过攻击按键按下</summary>
        private bool _attackPressedInState;

        /// <summary>
        /// 创建 Attack_3_End 状态实例
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        public Attack_3_End(PlayerController player) : base(player) { }

        /// <inheritdoc />
        protected override PlayerStateType StateType => PlayerStateType.Attack_3_End;

        /// <inheritdoc />
        protected override void OnEnter()
        {
            _holdStartTime = -1f;
            _attackPressedInState = false;
        }

        /// <inheritdoc />
        protected override void UpdateStateLogic()
        {
            if (!PlayerBrainBlackboard.AttackHeld && !PlayerBrainBlackboard.WantToAttack)
                return;

            if (PlayerBrainBlackboard.WantToAttack)
                _attackPressedInState = true;

            if (PlayerBrainBlackboard.AttackHeld && _holdStartTime < 0f)
                _holdStartTime = Time.time;

            if (PlayerBrainBlackboard.AttackHeld && _holdStartTime >= 0f && Time.time - _holdStartTime >= HoldThreshold)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_4_Prefect));
                return;
            }

            if (_attackPressedInState && !PlayerBrainBlackboard.AttackHeld)
            {
                _player.StateMachine.ChangeState(_player.StateMachine.GetState(PlayerStateType.Attack_4_Normal));
            }
        }

        /// <inheritdoc />
        public override void PhysicsUpdate() { }

        /// <inheritdoc />
        public override void Exit() { }
    }
}
