using System;
using SPCharacterController;
using UnityEngine;

namespace SPTeam
{
    /// <summary>
    /// 队伍控制器 - 根据 TeamInfoSO 实例化角色预制体，管理激活角色索引。
    /// 切换触发逻辑待重写 - 原输入直读已移除，暂不触发切换。
    /// </summary>
    [DefaultExecutionOrder(-350)]
    public class TeamController : MonoBehaviour
    {
        [Header("队伍配置")]
        [Tooltip("队伍数据 ScriptableObject，作为角色构成与激活索引的单一数据源。")]
        [SerializeField] private TeamInfoSO _teamInfo;

        [Tooltip("角色实例化的父节点与初始位置，未设置时使用自身 Transform。")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("两次角色切换之间的最短间隔（秒）。")]
        [SerializeField] private float _switchCooldown = 0.5f;

        private float _cooldownTimer;
        private TeamInfoSO _runtimeTeamInfo;
        private SPCharacterController.SPCC[] _characterControllers;

        private void Awake()
        {
            ValidateConfiguration();
            _runtimeTeamInfo = Instantiate(_teamInfo);
            InstantiateCharacters();
        }

        private void Start()
        {
            for (int i = 0; i < _characterControllers.Length; i++)
            {
                if (i == _runtimeTeamInfo.ActiveCharacterIndex)
                    _characterControllers[i].EnterTeam();
                else
                    _characterControllers[i].LeaveTeam();
            }
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer -= Time.deltaTime;

            // TODO: 切换触发逻辑待重写 - 原直接读输入已移除，
            //       后续由角色意图或事件模块驱动切换。
        }

        private void ValidateConfiguration()
        {
            if (_teamInfo == null)
                throw new InvalidOperationException($"{name}: TeamInfoSO 未设置。");

            for (int i = 0; i < TeamInfoSO.CharacterCount; i++)
            {
                CharacterInfoSO info = _teamInfo.Characters[i];
                if (info == null)
                    throw new InvalidOperationException($"{name}: TeamInfoSO 索引 {i} 的角色信息未设置。");
                if (info.Prefab == null)
                    throw new InvalidOperationException($"{name}: TeamInfoSO 索引 {i} 的角色 {info.name} 未配置预制体。");
            }
        }

        private void InstantiateCharacters()
        {
            Transform parent = _spawnPoint != null ? _spawnPoint : transform;
            _characterControllers = new SPCharacterController.SPCC[TeamInfoSO.CharacterCount];

            for (int i = 0; i < TeamInfoSO.CharacterCount; i++)
            {
                GameObject prefab = _runtimeTeamInfo.GetPrefab(i);
                GameObject instance = Instantiate(prefab, parent.position, parent.rotation, parent);
                _characterControllers[i] = instance.GetComponent<SPCharacterController.SPCC>();
            }
        }

        private void SwitchToNextCharacter()
        {
            int oldIndex = _runtimeTeamInfo.ActiveCharacterIndex;
            Transform leaving = _characterControllers[oldIndex].transform;
            Vector3 position = leaving.position;
            Quaternion rotation = leaving.rotation;
            _characterControllers[oldIndex].LeaveTeam();

            int newIndex = _runtimeTeamInfo.SwitchCharacter();
            _characterControllers[newIndex].EnterTeam(position, rotation);

            _cooldownTimer = _switchCooldown;
            SPEvent.GameEvent.OnCharacterSwitched(newIndex);
        }

        /// <summary>
        /// 运行时队伍数据副本，持有可变状态。
        /// </summary>
        public TeamInfoSO RuntimeTeamInfo => _runtimeTeamInfo;

        /// <summary>
        /// 获取指定索引角色的 Transform。
        /// </summary>
        /// <param name="index">角色在队伍中的索引</param>
        /// <returns>对应角色的 Transform</returns>
        public Transform GetCharacterTransform(int index)
        {
            if (index < 0 || index >= _characterControllers.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _characterControllers[index].transform;
        }

        /// <summary>
        /// 获取指定索引角色的运行时属性副本。
        /// </summary>
        /// <param name="index">角色在队伍中的索引</param>
        /// <returns>对应角色的运行时属性</returns>
        public CharacterStats GetCharacterStats(int index)
        {
            if (index < 0 || index >= _characterControllers.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _characterControllers[index].Stats;
        }
    }
}
