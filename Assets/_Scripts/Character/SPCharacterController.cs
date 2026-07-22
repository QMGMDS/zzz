using System;
using Animancer;
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
    public class SPCharacterController : MonoBehaviour
    {
        [Header("自定义配置")]
        [Tooltip("角色基础信息资产")]
        public CharacterInfoSO _characterInfo;

        [Tooltip("角色状态配置资产")]
        [SerializeField] private CharacterStateConfigSO _config;
        [Tooltip("角色输入源")]
        [SerializeField] private CCSourceSO _inputSource;
        [Tooltip("角色移动配置资产")]
        [SerializeField] private CharacterMotionConfigSO _motionConfig;

        [Header("必要组件引用")]
        [Tooltip("角色模型使用的 Animancer 组件")]
        [SerializeField] private AnimancerComponent _animancer;

        [Tooltip("负责角色碰撞移动的 CharacterController")]
        [SerializeField] private CharacterController _characterController;

        [Tooltip("相机相对移动的方向参考，未设置时在 Awake 获取 Main Camera")]
        [SerializeField] private Transform _movementReference;

        #region 私有依赖

        private CharacterRunTimeData _blackboard;
        private StateMachine _stateMachine;
        private AnimationDriver _animationDriver;
        private CharacterMotionDriver _motionDriver;
        private Animator _animator;
        private CCSourceSO _configuredInputSource;
        private bool _isLeaving;

        #endregion

        #region Life Cycle

        private void Awake()
        {
            _configuredInputSource = _inputSource;

            if (_characterInfo == null) throw new InvalidOperationException($"{name}: 未设置角色基础信息资产。");
            if (_config == null) throw new InvalidOperationException($"{name}: 未设置角色状态配置资产。");
            if (_inputSource == null) Debug.LogWarning($"{name}: 未设置角色输入源。");
            if (_animancer == null) throw new InvalidOperationException($"{name}: 未设置 Animancer 组件。");
            if (_motionConfig == null) throw new InvalidOperationException($"{name}: 未设置角色移动配置资产。");

            _animator = GetComponent<Animator>();
            if (_characterController == null) throw new InvalidOperationException($"{name}: 未设置 CharacterController 组件。");
            if (_movementReference == null && Camera.main != null) _movementReference = Camera.main.transform;
            if (_movementReference == null) throw new InvalidOperationException($"{name}: 未设置移动方向参考，场景中也没有 Main Camera。");
            _motionConfig.Validate();

            _blackboard = new CharacterRunTimeData();
            _stateMachine = new StateMachine(_config, _blackboard, 0);
            var animationSource = new AnimationSource(_animancer);
            _animationDriver = new AnimationDriver(_blackboard, animationSource);
            var motor = new CharacterMotor(_characterController, transform);
            _motionDriver = new CharacterMotionDriver(_blackboard, _motionConfig, motor, transform, _movementReference);
        }

        private void Update()
        {
            // 输入源更新角色意图
            _inputSource?.WriteIntentions(_blackboard);

            // 状态机逻辑更新
            _stateMachine.LogicUpdate();

            // 动画驱动器响应状态变化，下达动画指令
            _animationDriver.LogicUpdate();

            // 移动驱动器响应输入和状态运动政策
            _motionDriver.LogicUpdate(Time.deltaTime);

            // 清除已消费的意图，LateUpdate 产生的动画意图留给下一帧
            _blackboard.ResetIntentions();
        }

        //* Animator 自动更新骨骼，产出本帧根位移

        private void OnAnimatorMove()
        {
            if (!_motionDriver.UsesRootMotion) return;

            _motionDriver.ApplyRootMotion(_animator.deltaPosition, _animator.deltaRotation, Time.deltaTime);
        }

        private void LateUpdate()
        {
            // 动画进度回写黑板
            _animationDriver.SyncAnimProgress();

            if (_isLeaving && _blackboard.EvaluateCondition(CharacterIntention.AnimationCompleted))
            {
                _isLeaving = false;
                gameObject.SetActive(false);
            }
        }

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
