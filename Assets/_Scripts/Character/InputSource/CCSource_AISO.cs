using System;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace SPCharacterController
{
    /// <summary>
    /// 敌人 AI 输入源 - 装配行为树组件并注入参数，
    /// 每帧消费行为树决策输出（移动方向 / 攻击意图）翻译为角色意图写入黑板。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/InputSource/AI", fileName = "CCSource_AISO")]
    public class CCSource_AISO : CCSourceSO
    {
        private const string SightRangeVariableName = "SightRange";
        private const string LostSightDelayVariableName = "LostSightDelay";
        private const string AttackRangeVariableName = "AttackRange";
        private const string PatrolRangeVariableName = "PatrolRange";
        private const string PatrolCooldownVariableName = "PatrolCooldown";
        private const string OutMoveDirectionVariableName = "OutMoveDirection";
        private const string OutWantToAttackVariableName = "OutWantToAttack";

        [Header("行为树")]
        [Tooltip("敌人行为树资产（ExternalBehaviorTree），运行时按其中逻辑决策")]
        [SerializeField] private ExternalBehaviorTree _behaviorTree;

        [Header("感知参数")]
        [Tooltip("视野范围 - 玩家进入此范围后敌人开始追击")]
        [FormerlySerializedAs("_chaseRadius")]
        [SerializeField] private float _sightRange = 10f;

        [Tooltip("攻击范围 - 玩家进入此范围后敌人发动攻击")]
        [FormerlySerializedAs("_attackRadius")]
        [SerializeField] private float _attackRange = 2f;

        [Tooltip("丢失视野等待 - 看不见玩家后原地等待再开始巡逻的秒数")]
        [SerializeField] private float _lostSightDelay = 2f;

        [Header("巡逻参数")]
        [Tooltip("巡逻范围 - 以每次取点时敌人所在位置为圆心的巡逻半径")]
        [SerializeField] private float _patrolRange = 5f;

        [Tooltip("巡逻冷却 - 到达巡逻点后等待再次巡逻的秒数")]
        [SerializeField] private float _patrolCooldown = 3f;

        private BehaviorTree _behaviorTreeInstance;
        private SharedVector3 _outMoveDirection;
        private SharedBool _outWantToAttack;
        private bool _initialized;

        /// <summary>
        /// 初始化 AI 输入源 - 在敌人身上装配行为树组件、加载行为树资产并注入参数变量。
        /// </summary>
        /// <param name="selfTransform">敌人自身 Transform</param>
        public void Initialize(Transform selfTransform)
        {
            if (selfTransform == null) throw new ArgumentNullException(nameof(selfTransform));
            if (_behaviorTree == null)
                throw new InvalidOperationException("AI 输入源初始化失败：未设置行为树资产。");

            _behaviorTreeInstance = selfTransform.gameObject.AddComponent<BehaviorTree>();
            _behaviorTreeInstance.DisableBehavior();
            _behaviorTreeInstance.StartWhenEnabled = false;
            _behaviorTreeInstance.RestartWhenComplete = true;
            _behaviorTreeInstance.ExternalBehavior = _behaviorTree;
            _behaviorTreeInstance.EnableBehavior();

            _behaviorTreeInstance.SetVariableValue(SightRangeVariableName, _sightRange);
            _behaviorTreeInstance.SetVariableValue(LostSightDelayVariableName, _lostSightDelay);
            _behaviorTreeInstance.SetVariableValue(AttackRangeVariableName, _attackRange);
            _behaviorTreeInstance.SetVariableValue(PatrolRangeVariableName, _patrolRange);
            _behaviorTreeInstance.SetVariableValue(PatrolCooldownVariableName, _patrolCooldown);

            _outMoveDirection = GetRequiredVariable<SharedVector3>(OutMoveDirectionVariableName);
            _outWantToAttack = GetRequiredVariable<SharedBool>(OutWantToAttackVariableName);
            _initialized = true;
        }

        /// <inheritdoc />
        public override void WriteIntentions(CharacterRunTimeData blackboard)
        {
            if (blackboard == null) throw new ArgumentNullException(nameof(blackboard));
            if (!_initialized)
                throw new InvalidOperationException("AI 输入源未初始化，请先调用 Initialize()。");

            // 消费行为树上一帧的决策输出，清零后由行为树本帧重写
            Vector3 moveDirection = _outMoveDirection.Value;
            bool wantToAttack = _outWantToAttack.Value;
            _outMoveDirection.Value = Vector3.zero;
            _outWantToAttack.Value = false;

            bool wantsMove = moveDirection.sqrMagnitude > 0.0001f;
            blackboard.WriteInput(wantsMove ? new Vector2(moveDirection.x, moveDirection.z).normalized : Vector2.zero);
            blackboard.SetInputIntention(CharacterIntention.WantToMove, wantsMove);
            blackboard.SetInputIntention(CharacterIntention.NotWantToMove, !wantsMove);
            blackboard.SetInputIntention(CharacterIntention.WantToAttack, wantToAttack);
        }

        /// <summary>
        /// 获取行为树上必须存在的共享变量，缺失时视为资产配置错误。
        /// </summary>
        /// <param name="variableName">共享变量名</param>
        /// <returns>对应类型的共享变量实例</returns>
        private T GetRequiredVariable<T>(string variableName) where T : SharedVariable
        {
            if (_behaviorTreeInstance.GetVariable(variableName) is not T variable)
                throw new InvalidOperationException($"AI 行为树资产缺少共享变量：{variableName}。");
            return variable;
        }
    }
}
