using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 特效销毁策略 - 控制特效实例生命周期结束时机。
    /// </summary>
    public enum EffectDestroyPolicy
    {
        /// <summary>延迟自动销毁（由 AutoDestroyDelay 决定）。</summary>
        AutoDestroy,

        /// <summary>随状态退出而销毁。</summary>
        WithState,

        /// <summary>手动控制，驱动器不介入销毁。</summary>
        Manual
    }

    /// <summary>
    /// 特效纯数据模型 - 描述单个特效的预制体、释放时机与 Transform 配置。
    /// 一个状态可在不同时机、不同位置释放多个特效。
    /// </summary>
    [System.Serializable]
    public class EffectInfo
    {
        [Tooltip("特效预制体")]
        public GameObject Prefab;

        [Tooltip("特效释放时机（动画归一化时间闭区间）")]
        public NormalizedTimeRange ReleaseWindow = new NormalizedTimeRange(0f, 1f);

        [Tooltip("挂载到的骨骼路径（相对角色根，如 \"Hips/Spine/Chest\"），为空则使用角色根")]
        public string ParentBoneName;

        [Tooltip("是否作为父节点子物体（跟随父节点移动旋转），取消则在生成后脱离父节点")]
        public bool AttachToParent = true;

        [Tooltip("释放位置偏移（父节点本地空间）")]
        public Vector3 LocalPosition = Vector3.zero;

        [Tooltip("释放旋转偏移（父节点本地空间欧拉角）")]
        public Vector3 LocalRotation = Vector3.zero;

        [Tooltip("释放缩放（父节点本地空间）")]
        public Vector3 LocalScale = Vector3.one;

        [Tooltip("特效销毁策略")]
        public EffectDestroyPolicy DestroyPolicy = EffectDestroyPolicy.AutoDestroy;

        [Tooltip("自动销毁延迟（秒），仅在 AutoDestroy 策略下生效")]
        [Min(0f)]
        public float AutoDestroyDelay = 3f;
    }
}