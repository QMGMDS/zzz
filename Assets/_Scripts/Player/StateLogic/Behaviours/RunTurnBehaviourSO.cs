using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 奔跑转身行为——进入时保存当前朝向，退出时补偿旋转差量。
    /// </summary>
    [CreateAssetMenu(fileName = "RunTurnBehaviour", menuName = "Player/Behaviours/RunTurn")]
    public class RunTurnBehaviourSO : StateBehaviourSO
    {
        /// <inheritdoc />
        public override IStateBehaviour CreateRuntime()
        {
            return new Runtime();
        }

        private class Runtime : IStateBehaviour
        {
            private Quaternion _targetRotation;

            public void OnEnter(PlayerController player)
            {
                var dir = player.PlayerBrainBlackboard.CurrentMoveDirection;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;
                _targetRotation = Quaternion.LookRotation(dir);
            }

            public bool OnUpdate(PlayerController player)
            {
                return false;
            }

            public void OnExit(PlayerController player)
            {
                player.transform.rotation = _targetRotation;
            }
        }
    }
}
