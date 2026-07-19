using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 纯数据状态节点 ScriptableObject - 只负责存储资源引用和元数据，绝不包含任何切换逻辑
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/StateLogic/StateNode", fileName = "StateNode")]
    public class StateNodeSO : ScriptableObject
    {
        [Header("动画")]
        [Tooltip("状态的动画剪辑引用")]
        public SPAnimClip Animation;

        [Tooltip("该状态是否循环，循环状态不会产生动画完成意图")]
        public bool IsLooping;

        [Header("打断窗口")]
        [Tooltip("是否使用动画进度限制状态切换，未启用时满足规则即可切换")]
        public bool UseInterruptWindow;

        [Tooltip("允许状态切换的动画归一化进度闭区间")]
        public NormalizedTimeRange InterruptWindow = new NormalizedTimeRange(0f, 1f);

        [Header("移动元数据")]
        [Tooltip("此状态使用的主动水平运动来源")]
        public CharacterMotionMode MotionMode = CharacterMotionMode.CodeDriven;

        [Tooltip("此状态下是否允许转向")]
        public bool AllowRotation = true;

        [Tooltip("从此状态切出至 AllowRotation=true 的状态时，是否把角色朝向立即同步到输入方向（避免补转）")]
        public bool SnapRotationOnExit;

        [Tooltip("根运动位移倍率（0 = 原地，1 = 原始）")]
        [Min(0f)]
        public float RootMotionScale = 1f;

        [Tooltip("此状态是否使用动画产出的根旋转")]
        public bool UseRootMotionRotation;

        [Tooltip("根运动旋转倍率（0 = 忽略，1 = 原始）")]
        [Min(0f)]
        public float RootMotionRotationScale = 1f;
    }
}
