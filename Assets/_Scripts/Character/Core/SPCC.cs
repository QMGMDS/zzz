using System;
using Animancer;
using UnityEngine;
using SPCharacter.Contract;

namespace SPCharacter.Core
{
    /// <summary>
    /// 角色控制器 - Root MonoBehaviour 驱动源。
    /// 不包含任何具体游戏逻辑，仅负责子系统装配和严格的时序指令分发。
    /// </summary>
    [DefaultExecutionOrder(-300)]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AnimancerComponent))]
    public class SPCC : MonoBehaviour
    {
        [Header("必要组件引用")]
        [SerializeField, Tooltip("角色模型使用的 Animator 组件")]
        private Animator _animator;
        [SerializeField, Tooltip("角色模型使用的 Animancer 组件")]
        private AnimancerComponent _animancer;

        [Header("控制器配置")]
        [SerializeField, Tooltip("角色状态配置资产")]
        private CharacterStateConfigSO _config;

        [Header("意图注入")]
        [Tooltip("意图供给者资产")]
        [SerializeField] private CharacterIntentionProviderAsset _directProvider;

        private CharacterRunTimeData _blackboard;
        private StateMachine _stateMachine;
        private AnimationDriver _animationDriver;
        private MotionDriver _motionDriver;
        private IntentionProcessor _intentionProcessor;

        private void Awake()
        {
            if (_animancer == null) throw new InvalidOperationException($"{name}: 未设置 Animancer 组件。");
            if (_config == null) throw new InvalidOperationException($"{name}: 未设置角色状态配置资产。");
            if (_animator == null) throw new InvalidOperationException($"{name}: 未设置 Animator 组件。");

            // 关闭引擎根运动 - 位移完全交由 MotionDriver 用烘焙曲线驱动。
            _animator.applyRootMotion = false;

            _blackboard = new CharacterRunTimeData();
            _stateMachine = new StateMachine(_config, _blackboard);
            _animationDriver = new AnimationDriver(_blackboard, _animancer, _stateMachine.NodesById);
            _motionDriver = new MotionDriver(_blackboard, _stateMachine.NodesById, transform);
            _intentionProcessor = new IntentionProcessor(_blackboard);
        }

        private void Update()
        {
            // 意图写入黑板
            if (_directProvider != null) _intentionProcessor.Process(_directProvider.CurrentFrame);

            // 状态更新
            _stateMachine.LogicUpdate();

            // 动画指令下发
            _animationDriver.LogicUpdate();

            // 黑板意图刷新
            _blackboard.ResetIntentions();
        }

        // *Animator 应用骨骼 Transform，动画更新*
        // 占位实现 - applyRootMotion 已关闭，此处仅阻断默认根运动处理，位移逻辑在 LateUpdate 落位。
        void OnAnimatorMove()
        {
        }

        private void LateUpdate()
        {
            // 动画进度回写黑板
            _animationDriver.SyncAnimProgress();

            // 旋转更新
            _motionDriver.RotationUpdate();

            // 位移更新
            _motionDriver.PositionUpdate();
        }
    }
}
