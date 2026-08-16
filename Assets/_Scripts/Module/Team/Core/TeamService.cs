using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPTeam.Core
{
    /// <summary>
    /// 队伍数据层 - 持有名册与切换状态机 向接线胶水提供切换判定与状态转换入口
    /// </summary>
    [DefaultExecutionOrder(-350)]
    internal sealed class TeamService : MonoBehaviour
    {
        [Header("队伍配置")]
        [SerializeField, Tooltip("队伍配置资产 包含角色槽位与初始上场索引")]
        private TeamConfigSO _config;

        [Header("切换兜底")]
        [SerializeField, Range(1f, 30f), Tooltip("切换会话超时时间 单位秒 超时未完成则强制收尾")]
        private float _switchTimeout = 5f;

        private TeamRoster _roster;
        private TeamSwitchCoordinator _coordinator;
        private bool _isInitialized;

        /// <summary>切换会话超时时间 单位秒</summary>
        public float SwitchTimeout => _switchTimeout;

        /// <summary>名册是否已初始化 - 初始化前不响应切换请求与状态查询</summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>当前上场角色 Id</summary>
        public string ActiveCharacterId => _coordinator.ActiveCharacterId;

        /// <summary>切换锁 - 切换会话存续期间为真</summary>
        public bool IsSwitchLocked => _coordinator.IsSwitchLocked;

        /// <summary>操作锁 - 目标入场完成前为真</summary>
        public bool IsOperationLocked => _coordinator.IsOperationLocked;

        /// <summary>待退场角色 Id - 无切换时为空</summary>
        public string SwitchOutCharacterId => _coordinator.SwitchOutCharacterId;

        /// <summary>待入场角色 Id - 无切换时为空</summary>
        public string SwitchInCharacterId => _coordinator.SwitchInCharacterId;

        /// <summary>当前是否允许发起切换 - 含初始化与内部状态判定</summary>
        public bool CanRequestSwitch => _isInitialized && _coordinator.CanRequestSwitch;

        /// <summary>
        /// 获取队伍槽位清单 - 校验配置后返回
        /// </summary>
        /// <returns>按切换顺序排列的槽位清单</returns>
        public IReadOnlyList<TeamCharacterSlot> GetSlots()
        {
            ValidateConfig();
            return _config.Slots;
        }

        /// <summary>
        /// 计算顺序切换的下一个角色 Id
        /// </summary>
        /// <returns>下一个角色 Id</returns>
        public string ResolveNextCharacterId()
        {
            return _coordinator.ResolveNextCharacterId();
        }

        /// <summary>
        /// 获取角色实例对象
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <returns>角色实例对象</returns>
        public GameObject GetCharacterObject(string characterId)
        {
            return _roster.GetCharacterObject(characterId);
        }

        /// <summary>
        /// 尝试获取角色实例对象
        /// </summary>
        /// <param name="characterId">角色 Id</param>
        /// <param name="instance">角色实例对象</param>
        /// <returns>Id 是否已登记</returns>
        public bool TryGetCharacterObject(string characterId, out GameObject instance)
        {
            return _roster.TryGetCharacterObject(characterId, out instance);
        }

        /// <summary>
        /// 提交一次切换 - 校验通过后进入切换状态
        /// </summary>
        /// <param name="activeCharacterId">当前上场角色 Id</param>
        /// <param name="targetCharacterId">目标角色 Id</param>
        /// <returns>是否提交成功</returns>
        public bool TryCommitSwitch(string activeCharacterId, string targetCharacterId)
        {
            return _coordinator.TryCommitSwitch(activeCharacterId, targetCharacterId);
        }

        /// <summary>
        /// 回推退场完成
        /// </summary>
        /// <param name="characterId">完成退场的角色 Id</param>
        /// <returns>是否匹配当前切换</returns>
        public bool CompleteSwitchOut(string characterId)
        {
            return _coordinator.CompleteSwitchOut(characterId);
        }

        /// <summary>
        /// 回推入场完成 - 完成后切换当前上场角色
        /// </summary>
        /// <param name="characterId">完成入场的角色 Id</param>
        /// <returns>是否匹配当前切换</returns>
        public bool CompleteSwitchIn(string characterId)
        {
            return _coordinator.CompleteSwitchIn(characterId);
        }

        /// <summary>
        /// 强制完成当前切换会话 - 超时兜底用
        /// </summary>
        public void ForceCompleteSession()
        {
            _coordinator.ForceCompleteSession();
        }

        /// <summary>
        /// 初始化队伍数据 - 登记装配结果并激活初始角色
        /// </summary>
        /// <param name="characters">角色移交列表 必须与队伍配置槽位一一对应</param>
        public void Initialize(IReadOnlyList<TeamCharacterHandover> characters)
        {
            ValidateConfig();

            if (characters == null || characters.Count != _config.Slots.Count)
                throw new InvalidOperationException($"{name}: 装配结果数量与队伍配置不符");

            for (int i = 0; i < characters.Count; i++)
            {
                if (!string.Equals(characters[i].CharacterId, _config.Slots[i].CharacterId, StringComparison.Ordinal))
                    throw new InvalidOperationException($"{name}: 装配结果顺序与队伍配置不符");
            }

            _roster = new TeamRoster();

            foreach (TeamCharacterHandover character in characters)
                _roster.Register(character.CharacterId, character.Instance, character.Release);

            _coordinator = new TeamSwitchCoordinator(_roster.OrderedCharacterIds, _config.InitialIndex);
            _roster.GetCharacterObjectAt(_config.InitialIndex).SetActive(true);
            _isInitialized = true;
        }

        private void ValidateConfig()
        {
            if (_config == null)
                throw new InvalidOperationException($"{name}: 未配置队伍资产");

            if (!_config.TryValidate(out string errorMessage))
                throw new InvalidOperationException($"{name}: 队伍配置无效 - {errorMessage}");
        }

        private void OnDestroy()
        {
            // 装配失败时名册可能未创建 资源释放仍需兜底
            _roster?.Release();
        }
    }

    /// <summary>
    /// 角色移交项 - Core 内部登记名册用的单个角色实例
    /// </summary>
    internal sealed class TeamCharacterHandover
    {
        /// <summary>
        /// 创建角色移交项
        /// </summary>
        /// <param name="characterId">角色唯一标识</param>
        /// <param name="instance">角色实例对象</param>
        /// <param name="release">释放实例的委托</param>
        public TeamCharacterHandover(string characterId, GameObject instance, Action release)
        {
            CharacterId = characterId;
            Instance = instance;
            Release = release;
        }

        /// <summary>角色唯一标识</summary>
        public string CharacterId { get; }

        /// <summary>角色实例对象</summary>
        public GameObject Instance { get; }

        /// <summary>释放实例的委托</summary>
        public Action Release { get; }
    }
}
