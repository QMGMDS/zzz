using Animancer;
using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 纯数据状态节点 ScriptableObject
    /// 只负责存储资源内容，完全不负责节点间的切换逻辑。
    /// 每个节点包含动画、移动元数据、打断窗口和可选行为插件引用。
    /// </summary>
    [CreateAssetMenu(fileName = "StateNode", menuName = "Player/StateNode")]
    public class StateNodeSO : ScriptableObject
    {
        [Header("动画")]
        [Tooltip("Animancer 过渡资产")]
        public TransitionAssetBase Transition;

        [Tooltip("是否循环播放")]
        public bool IsLooping;

        [Header("移动元数据")]
        [Tooltip("此状态下角色是否可以转向")]
        public bool AllowRotation = true;

        [Tooltip("根运动位移倍率（0 = 禁止位移，1 = 原始）")]
        [Min(0f)]
        public float RootMotionScale = 1f;

        [Header("打断窗口")]
        [Tooltip("可被打断的起始归一化时间（0~1），>= End 时关闭窗口")]
        [Range(0f, 1f)]
        public float CancelWindowStart;

        [Tooltip("可被打断的结束归一化时间（0~1），<= Start 时关闭窗口")]
        [Range(0f, 1f)]
        public float CancelWindowEnd = 1f;

        public bool HasCancelWindow => CancelWindowEnd > CancelWindowStart;

        [Header("行为插件")]
        [Tooltip("可选状态行为——仅少数需要特殊逻辑的状态使用，大多数节点留空即可")]
        public StateBehaviourSO Behaviour;
    }
}
