using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 攻击蓄力分支行为——处理 Attack_3 按住/松开的 Normal/Prefect 分支逻辑。
    /// </summary>
    [CreateAssetMenu(fileName = "AttackHoldBranchBehaviour", menuName = "Player/Behaviours/AttackHoldBranch")]
    public class AttackHoldBranchBehaviourSO : StateBehaviourSO
    {
        [Header("分支目标")]
        [Tooltip("松开按键时跳转的节点索引")]
        public int NormalTargetIndex;

        [Tooltip("按住超时后跳转的节点索引")]
        public int PrefectTargetIndex;

        [Header("蓄力阈值")]
        [Tooltip("按住超过多少秒判定为蓄力攻击")]
        [Min(0f)]
        public float HoldThreshold = 0.1f;

        /// <inheritdoc />
        public override IStateBehaviour CreateRuntime()
        {
            return new Runtime(this);
        }

        private class Runtime : IStateBehaviour
        {
            private readonly AttackHoldBranchBehaviourSO _config;
            private float _heldStartTime = -1f;

            public Runtime(AttackHoldBranchBehaviourSO config)
            {
                _config = config;
            }

            public void OnEnter(PlayerController player)
            {
                _heldStartTime = -1f;
            }

            public bool OnUpdate(PlayerController player)
            {
                var brain = player.PlayerBrainBlackboard;

                if (brain.AttackHeld && _heldStartTime < 0f)
                    _heldStartTime = Time.time;
                else if (!brain.AttackHeld)
                    _heldStartTime = -1f;

                if (!brain.WantToAttack) return false;

                var node = player.GroupStateMachine.CurrentNode;
                if (node == null || brain.CurrentNormalizedTime < node.CancelWindowStart)
                    return false;

                if (brain.AttackHeld && _heldStartTime >= 0f && Time.time - _heldStartTime >= _config.HoldThreshold)
                {
                    player.GroupStateMachine.TransitionToNode(_config.PrefectTargetIndex);
                    return true;
                }

                if (!brain.AttackHeld)
                {
                    player.GroupStateMachine.TransitionToNode(_config.NormalTargetIndex);
                    return true;
                }

                return false;
            }

            public void OnExit(PlayerController player)
            {
                _heldStartTime = -1f;
            }
        }
    }
}
