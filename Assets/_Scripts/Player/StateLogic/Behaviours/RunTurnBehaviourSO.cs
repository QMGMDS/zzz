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
            private Vector3 _enterDirection;

            public void OnEnter(PlayerController player)
            {
                _enterDirection = player.transform.forward;
                _enterDirection.y = 0f;

                if (_enterDirection.sqrMagnitude < 0.0001f)
                    _enterDirection = Vector3.forward;
                else
                    _enterDirection.Normalize();
            }

            public bool OnUpdate(PlayerController player)
            {
                return false;
            }

            public void OnExit(PlayerController player)
            {
                var currentDir = player.PlayerBrainBlackboard.CurrentMoveDirection;
                if (currentDir.sqrMagnitude <= 0.0001f) return;

                currentDir.y = 0f;
                currentDir.Normalize();

                var enterAngle = Mathf.Atan2(_enterDirection.x, _enterDirection.z) * Mathf.Rad2Deg;
                var currentAngle = Mathf.Atan2(currentDir.x, currentDir.z) * Mathf.Rad2Deg;
                var delta = Mathf.DeltaAngle(currentAngle, enterAngle);

                player.transform.Rotate(Vector3.up, delta, Space.World);
            }
        }
    }
}
