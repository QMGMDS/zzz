using System;
using Animancer;
using SPEffects;
using SPEvent;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色控制器 - Root MonoBehaviour 驱动源。
    /// 不包含任何具体游戏逻辑，仅负责子系统装配和严格的时序指令分发。
    /// </summary>
    [DefaultExecutionOrder(-300)]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AnimancerComponent))]
    [RequireComponent(typeof(CharacterController))]
    public class SPCC : MonoBehaviour
    {
        [Header("必要组件引用")]
        [SerializeField, Tooltip("角色模型使用的 Animator 组件")]
        private Animator _animator;
        [SerializeField, Tooltip("角色模型使用的 Animancer 组件")]
        private AnimancerComponent _animancer;
        [SerializeField, Tooltip("负责角色碰撞移动的 CharacterController")]
        private CharacterController _characterController;

        [Header("自定义配置")]
        [SerializeField, Tooltip("角色基础信息资产")]
        private CharacterInfoSO _characterInfo;

        [SerializeField, Tooltip("角色状态配置资产")]
        private CharacterStateConfigSO _config;

        [SerializeField, Tooltip("角色输入源")]
        private CCSourceSO _inputSource;

        [SerializeField, Tooltip("角色移动配置资产")]
        private CharacterMotionConfigSO _motionConfig;

        [SerializeField, Tooltip("特效目录资产（可选，未配置时该角色不释放特效）")]
        private EffectCatalogSO _effectCatalog;

        #region 私有依赖

        private CharacterRunTimeData _blackboard;
        private StateMachine _stateMachine;
        private AnimationDriver _animationDriver;
        private CharacterMotionDriver _motionDriver;
        private EffectTriggerDriver _effectTriggerDriver;
        private IEffectService _effectService;
        private CCSourceSO _configuredInputSource;
        private bool _isLeaving;

        #endregion

        #region Public API

        /// <summary>角色运行时属性副本，供 UI 与战斗系统读写当前状态。</summary>
        public CharacterStats Stats { get; private set; }

        #endregion

        #region 时序指令分发

        private void Awake()
        {
            // 必要组件检查
            if (_animancer == null) throw new InvalidOperationException($"{name}: 未设置 Animancer 组件。");
            if (_characterController == null) throw new InvalidOperationException($"{name}: 未设置 CharacterController 组件。");
            // 自定义配置检查
            if (_characterInfo == null) throw new InvalidOperationException($"{name}: 未设置角色基础信息资产。");
            if (_characterInfo.Stats == null) throw new InvalidOperationException($"{name}: 未设置角色属性资产。");
            if (_config == null) throw new InvalidOperationException($"{name}: 未设置角色状态配置资产。");
            if (_inputSource == null) throw new InvalidOperationException($"{name}: 未设置角色输入源。");
            if (_motionConfig == null) throw new InvalidOperationException($"{name}: 未设置角色移动配置资产。");

            _inputSource = Instantiate(_inputSource);
            _configuredInputSource = _inputSource;
            _motionConfig.Validate();

            if (_inputSource is CCSource_PlayerSO playerSource)
                playerSource.Initialize();
            if (_inputSource is CCSource_AISO aiSource)
                aiSource.Initialize(transform);

            Stats = new CharacterStats(_characterInfo.Stats);
            _blackboard = new CharacterRunTimeData();
            _stateMachine = new StateMachine(_config, _blackboard, 0);
            var animationSource = new AnimationSource(_animancer);
            _animationDriver = new AnimationDriver(_blackboard, animationSource);
            var motor = new CharacterMotor(_characterController, transform);
            _motionDriver = new CharacterMotionDriver(_blackboard, _motionConfig, motor, transform);

            // 可选配置装配
            if (_effectCatalog != null)
            {
                _effectService = new EffectService(_effectCatalog);
                _effectTriggerDriver = new EffectTriggerDriver(_blackboard, transform, _effectService);
            }
        }

        private void Update()
        {
            _inputSource?.WriteIntentions(_blackboard);
            _stateMachine.LogicUpdate();
            _animationDriver.LogicUpdate();
            _motionDriver.LogicUpdate(Time.deltaTime);
            _blackboard.ResetIntentions();
        }

        private void OnAnimatorMove()
        {
            if (!_motionDriver.UsesRootMotion) return;

            _motionDriver.ApplyRootMotion(_animator.deltaPosition, _animator.deltaRotation, Time.deltaTime);
        }

        private void LateUpdate()
        {
            _animationDriver.SyncAnimProgress();
            _effectTriggerDriver?.LogicUpdate();

            if (_isLeaving && _blackboard.EvaluateCondition(CharacterIntention.AnimationCompleted))
            {
                _isLeaving = false;
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region 事件

        private void OnEnable()
        {
            GameEvent.RoundEnded += HandleRoundEnded;
        }
        private void OnDisable()
        {
            GameEvent.RoundEnded -= HandleRoundEnded;
            _effectTriggerDriver?.Cleanup();
        }

        /// <summary>
        /// 本局结束 - 清理本角色创建的全部特效实例。
        /// </summary>
        private void HandleRoundEnded() => _effectService?.CleanupAll();

        #endregion

        #region TeamController

        /// <summary>
        /// 角色下场 - 写入 SwitchOut 意图并滞空输入源，由状态配置驱动退场动画。
        /// </summary>
        public void LeaveTeam()
        {
            _isLeaving = true;
            _blackboard.SetInputIntention(CharacterIntention.SwitchOut, true);
            _inputSource = null;
        }

        /// <inheritdoc cref="EnterTeam(Vector3, Quaternion)"/>
        public void EnterTeam() => EnterTeam(transform.position, transform.rotation);

        /// <summary>
        /// 角色登场 - 在指定位置和朝向恢复输入源并写入 SwitchIn 意图，由状态配置驱动入场动画。
        /// </summary>
        /// <param name="position">登场位置</param>
        /// <param name="rotation">登场朝向</param>
        public void EnterTeam(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            gameObject.SetActive(true);
            _isLeaving = false;
            _inputSource = _configuredInputSource;
            _blackboard.SetInputIntention(CharacterIntention.SwitchIn, true);
        }

        #endregion
    }
}

