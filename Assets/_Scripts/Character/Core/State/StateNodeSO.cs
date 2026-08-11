using Animancer;
using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 纯数据状态节点 ScriptableObject，只存储控制器状态所需的动画与运动配置
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacter/State/StateNode", fileName = "StateNode")]
    internal sealed class StateNodeSO : ScriptableObject
    {
        [SerializeField, Tooltip("状态唯一标识，在同一个 CharacterStateConfigSO 内必须唯一，供转移规则引用")]
        private string _id;

        [Header("意图")]
        [SerializeField, Tooltip("该状态是否循环，循环状态不会产生动画完成意图")]
        private bool _isLooping;

        [Header("动画")]
        [SerializeField, Tooltip("状态的动画 Transition 资源")]
        private TransitionAssetBase _animation;

        [Header("运动")]
        [SerializeField, Tooltip("离线烘焙的根运动位移曲线资产，留空表示该状态无根运动位移")]
        private RootMotionProfileSO _rootMotionProfile;
        [SerializeField, Min(0f), Tooltip("该状态每秒最大转向角度，单位为度/秒；0 表示不主动旋转")]
        private float _turnSpeedDegreesPerSecond = 720f;
        [SerializeField, Tooltip("该状态动画完成时一次性施加的相对 Y 轴旋转角度，单位为度")]
        private float _completionRotationDegrees;

        /// <summary>状态唯一标识</summary>
        public string Id => _id;

        /// <summary>该状态是否循环</summary>
        public bool IsLooping => _isLooping;

        /// <summary>状态的动画引用</summary>
        public TransitionAssetBase Animation => _animation;

        /// <summary>离线烘焙的根运动位移曲线资产</summary>
        public RootMotionProfileSO RootMotionProfile => _rootMotionProfile;

        /// <summary>该状态每秒最大转向角度</summary>
        public float TurnSpeedDegreesPerSecond => _turnSpeedDegreesPerSecond;

        /// <summary>该状态动画完成时一次性施加的相对 Y 轴旋转角度</summary>
        public float CompletionRotationDegrees => _completionRotationDegrees;
    }
}
