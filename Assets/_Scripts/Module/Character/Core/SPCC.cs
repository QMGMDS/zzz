using System;

using UnityEngine;

using Animancer;

namespace SPCharacter.Core
{
    /// <summary>
    /// 角色控制器 - Root MonoBehaviour 驱动源
    /// 绝不包含任何具体游戏逻辑，仅负责子系统装配和严格的时序指令分发
    /// </summary>
    [DefaultExecutionOrder(-300)]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AnimancerComponent))]
    [RequireComponent(typeof(CharacterController))]
    internal sealed class SPCC : MonoBehaviour
    {
        [Header("必要组件引用")]
        [SerializeField, Tooltip("角色模型使用的 Animator 组件")]
        private Animator _animator;
        [SerializeField, Tooltip("角色模型使用的 Animancer 组件")]
        private AnimancerComponent _animancer;
        [SerializeField, Tooltip("角色模型使用的 CharacterController 组件")]
        private CharacterController _characterController;

        [Header("控制器配置")]
        [SerializeField, Tooltip("角色状态配置资产")]
        private CCStateConfigSO _config;

        #region 私有依赖

        private CCRunTimeBlackboard _blackboard;
        private CCStateMachine _stateMachine;
        private AnimationDriver _animationDriver;
        private MotionDriver _motionDriver;
        private CCWiringExtensionPipeline _wiringExtensionPipeline;

        #endregion

        #region 配置检查

        private void Awake()
        {
            if (_animator == null) throw new InvalidOperationException($"{name}: 未设置 Animator 组件");
            if (_animancer == null) throw new InvalidOperationException($"{name}: 未设置 Animancer 组件");
            if (_characterController == null) throw new InvalidOperationException($"{name}: 未设置 CharacterController 组件");
            if (_config == null) throw new InvalidOperationException($"{name}: 未设置角色状态配置资产");
        }

        #endregion

        #region 内部初始化

        private void Start()
        {
            // 关闭引擎根运动 - 位移完全交由 MotionDriver 用烘焙曲线驱动
            _animator.applyRootMotion = false;

            _blackboard = new CCRunTimeBlackboard();
            CCStateGraph stateGraph = _config.BuildRuntimeGraph();
            _stateMachine = new CCStateMachine(stateGraph, _blackboard);
            _animationDriver = new AnimationDriver(_blackboard, _animancer, stateGraph.NodesById);
            _motionDriver = new MotionDriver(_blackboard, stateGraph.NodesById, transform, _characterController);

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            _wiringExtensionPipeline = new CCWiringExtensionPipeline(_blackboard, transform, behaviours);
        }

        #endregion

        #region 帧时序管理

        private void Update()
        {
            // 胶水扩展写入当前帧外部控制意图
            _wiringExtensionPipeline.LogicUpdate();

            // 状态机更新
            _stateMachine.LogicUpdate();

            // 黑板意图擦除
            _blackboard.ResetIntentions();

            // 动画指令下发
            _animationDriver.LogicUpdate();
        }

        // *Animator 应用骨骼 Transform，动画更新*
        // 占位实现 - applyRootMotion 已关闭，此处仅阻断默认根运动处理
        private void OnAnimatorMove() { }

        private void LateUpdate()
        {
            // 动画进度回写黑板
            _animationDriver.SyncAnimProgress();

            // 旋转更新
            _motionDriver.RotationUpdate();

            // 位移更新
            _motionDriver.PositionUpdate();
        }

        #endregion
    }
}
