using GamePlay.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Player
{
    /// <summary>调试工具：按 F1 对玩家造成伤害，用于测试受击系统</summary>
    public class PlayerDebugDamage : MonoBehaviour
    {
        [Tooltip("每次伤害值")]
        [SerializeField] private float _damageAmount = 10f;

        private PlayerController _player;

        private void Awake()
        {
            _player = FindObjectOfType<PlayerController>();
        }

        private void Update()
        {
            if (Keyboard.current.f1Key.wasPressedThisFrame)
                DealDamage();
        }

        private void DealDamage()
        {
            if (_player == null) return;

            var info = new DamageInfo
            {
                Amount = _damageAmount,
            };

            _player.TakeDamage(info);
        }
    }
}
