using UnityEngine;

namespace SPAI.Core
{
    /// <summary>
    /// 敌人 AI 配置资产 - 定义巡逻 视野 追击与攻击的决策参数
    /// </summary>
    [CreateAssetMenu(menuName = "SPAI/Enemy Config", fileName = "EnemyConfig")]
    internal sealed class EnemyConfigSO : ScriptableObject
    {
        [Header("巡逻")]
        [SerializeField, Range(0.5f, 50f), Tooltip("巡逻半径 单位 米 以出生锚点为中心的随机巡逻范围")]
        private float _patrolRadius = 5f;

        [SerializeField, Range(0.05f, 2f), Tooltip("巡逻到达阈值 单位 米 与巡逻点距离小于该值视为到达")]
        private float _patrolArriveDistance = 0.3f;

        [SerializeField, Range(0f, 30f), Tooltip("巡逻停留时间 单位 秒 到达巡逻点后停留的时长")]
        private float _patrolWaitSeconds = 2f;

        [SerializeField, Range(0f, 50f), Tooltip("两次随机巡逻取点的最小间距 单位 米 新巡逻点与上一巡逻点的 XZ 距离小于该值将重新取点")]
        private float _patrolMinStepDistance = 2f;

        [Header("感知")]
        [SerializeField, Range(0.5f, 100f), Tooltip("视野距离 单位 米 超过该距离无法发现目标")]
        private float _viewDistance = 10f;

        [SerializeField, Range(1f, 360f), Tooltip("视锥角 单位 度 以自身朝向为中心的全角")]
        private float _viewAngle = 120f;

        [Header("追击")]
        [SerializeField, Range(0.5f, 200f), Tooltip("最大追击距离 单位 米 相对巡逻锚点 超出强制脱战")]
        private float _maxChaseDistance = 20f;

        [SerializeField, Range(0f, 30f), Tooltip("抵达最后目击点后的停留时间 单位 秒 抵达最后一次目击位置后原地停留的时长 停留期间重新看到目标则立即恢复追击")]
        private float _lastSeenWaitSeconds = 2f;

        [Header("攻击")]
        [SerializeField, Range(0.1f, 20f), Tooltip("攻击范围 单位 米 与目标距离不大于该值时可发动攻击")]
        private float _attackRange = 2f;

        /// <summary>巡逻半径 单位 米</summary>
        public float PatrolRadius => _patrolRadius;

        /// <summary>巡逻到达阈值 单位 米</summary>
        public float PatrolArriveDistance => _patrolArriveDistance;

        /// <summary>巡逻停留时间 单位 秒</summary>
        public float PatrolWaitSeconds => _patrolWaitSeconds;

        /// <summary>两次随机巡逻取点的最小间距 单位 米</summary>
        public float PatrolMinStepDistance => _patrolMinStepDistance;

        /// <summary>视野距离 单位 米</summary>
        public float ViewDistance => _viewDistance;

        /// <summary>视锥角 单位 度 全角</summary>
        public float ViewAngle => _viewAngle;

        /// <summary>最大追击距离 单位 米 相对巡逻锚点</summary>
        public float MaxChaseDistance => _maxChaseDistance;

        /// <summary>敌人抵达最后一次目击位置后的原地停留秒数</summary>
        public float LastSeenWaitSeconds => _lastSeenWaitSeconds;

        /// <summary>攻击范围 单位 米</summary>
        public float AttackRange => _attackRange;
    }
}
