using System;
using System.Collections.Generic;
using SPPlayerInput;
using UnityEngine;

namespace SPTeam
{
    /// <summary>
    /// 队伍控制器 - 轮询切换唯一激活角色，并通过角色激活状态交接玩家输入。
    /// </summary>
    [DefaultExecutionOrder(-350)]
    public class TeamController : MonoBehaviour
    {
        [Header("队伍配置")]
        [Tooltip("按切换顺序排列的角色根物体，同一时间仅激活其中一个角色。")]
        [SerializeField] private GameObject[] _characters = Array.Empty<GameObject>();

        [Tooltip("两次角色切换之间的最短间隔（秒）。")]
        [SerializeField] private float _switchCooldown = 0.5f;

        private int _currentCharacterIndex;
        private float _cooldownTimer;
        private SPPlayerInputCenter _inputCenter;
        private SPCharacterController.SPCharacterController[] _characterControllers;

        private void Awake()
        {
            ValidateConfiguration();
            CacheCharacterControllers();
        }

        private void Start()
        {
            for (int i = 0; i < _characterControllers.Length; i++)
            {
                if (i == _currentCharacterIndex)
                    _characterControllers[i].EnterTeam();
                else
                    _characterControllers[i].LeaveTeam();
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            if (_inputCenter.CurrentFrameInput.SwitchCharacterPressed && _cooldownTimer <= 0f)
                SwitchToNextCharacter();
        }

        private void ValidateConfiguration()
        {
            _inputCenter = SPPlayerInputCenter.Instance;
            if (_inputCenter == null)
                throw new InvalidOperationException($"{name}: 场景中没有可用的玩家输入中心。");
            if (_characters == null || _characters.Length < 2)
                throw new InvalidOperationException($"{name}: 队伍至少需要配置两个角色。");

            var uniqueCharacters = new HashSet<GameObject>();
            for (int i = 0; i < _characters.Length; i++)
            {
                GameObject character = _characters[i];
                if (character == null)
                    throw new InvalidOperationException($"{name}: 队伍角色索引 {i} 未设置角色物体。");
                if (!uniqueCharacters.Add(character))
                    throw new InvalidOperationException($"{name}: 角色物体 {character.name} 被重复配置。");
                if (character == gameObject || transform.IsChildOf(character.transform))
                    throw new InvalidOperationException($"{name}: TeamController 不能位于角色物体 {character.name} 上或其子层级中。");
                if (!character.TryGetComponent(out SPCharacterController.SPCharacterController _))
                    throw new InvalidOperationException($"{name}: 角色物体 {character.name} 没有 SPCharacterController 组件。");
            }
        }

        private void SwitchToNextCharacter()
        {
            Transform leaving = _characters[_currentCharacterIndex].transform;
            Vector3 position = leaving.position;
            Quaternion rotation = leaving.rotation;
            _characterControllers[_currentCharacterIndex].LeaveTeam();
            _currentCharacterIndex = (_currentCharacterIndex + 1) % _characters.Length;
            _characterControllers[_currentCharacterIndex].EnterTeam(position, rotation);
            _cooldownTimer = _switchCooldown;

            SPEvent.GameEvent.OnCharacterSwitched(_currentCharacterIndex);
        }

        /// <summary>
        /// 获取指定索引角色的 Transform。
        /// </summary>
        /// <param name="index">角色在队伍中的索引</param>
        /// <returns>对应角色的 Transform</returns>
        public Transform GetCharacterTransform(int index)
        {
            if (index < 0 || index >= _characters.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _characters[index].transform;
        }

        private void CacheCharacterControllers()
        {
            _characterControllers = new SPCharacterController.SPCharacterController[_characters.Length];
            for (int i = 0; i < _characters.Length; i++)
                _characterControllers[i] = _characters[i].GetComponent<SPCharacterController.SPCharacterController>();
        }
    }
}
