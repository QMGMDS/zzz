using System;
using UnityEngine;

namespace SPEffects
{
    /// <summary>
    /// 特效触发空间 - 指定位置与旋转偏移使用的参考节点。
    /// </summary>
    public enum EffectTriggerSpace
    {
        /// <summary>角色根节点空间</summary>
        CharacterRoot,

        /// <summary>世界空间</summary>
        World
    }

    /// <summary>
    /// 特效触发配置 - 描述动画释放时刻、特效 ID 与生成空间参数。
    /// </summary>
    [Serializable]
    public sealed class EffectTrigger
    {
        [SerializeField, Range(0f, 1f), Tooltip("特效释放时刻（动画归一化时间）")]
        private float _releaseTime;

        [SerializeField, Tooltip("特效目录中的唯一 ID")]
        private string _effectId;

        [SerializeField, Tooltip("位置与旋转偏移使用的参考空间")]
        private EffectTriggerSpace _space = EffectTriggerSpace.CharacterRoot;

        [SerializeField, Tooltip("是否让生成实例继续跟随参考节点")]
        private bool _attachToSource = true;

        [SerializeField, Tooltip("参考空间中的位置偏移（米）")]
        private Vector3 _positionOffset;

        [SerializeField, Tooltip("参考空间中的旋转偏移（欧拉角）")]
        private Vector3 _rotationOffset;

        [SerializeField, Tooltip("特效实例缩放")]
        private Vector3 _scale = Vector3.one;

        [SerializeField, Tooltip("特效销毁策略")]
        private EffectDestroyPolicy _destroyPolicy = EffectDestroyPolicy.AutoDestroy;

        [SerializeField, Min(0f), Tooltip("自动销毁延迟（秒），仅自动销毁策略使用")]
        private float _autoDestroyDelay = 3f;

        /// <summary>特效释放时刻</summary>
        public float ReleaseTime => _releaseTime;

        /// <summary>特效唯一 ID</summary>
        public string EffectId => _effectId;

        /// <summary>参考空间</summary>
        public EffectTriggerSpace Space => _space;

        /// <summary>是否跟随参考节点</summary>
        public bool AttachToSource => _attachToSource;

        /// <summary>位置偏移</summary>
        public Vector3 PositionOffset => _positionOffset;

        /// <summary>旋转偏移</summary>
        public Vector3 RotationOffset => _rotationOffset;

        /// <summary>实例缩放</summary>
        public Vector3 Scale => _scale;

        /// <summary>销毁策略</summary>
        public EffectDestroyPolicy DestroyPolicy => _destroyPolicy;

        /// <summary>自动销毁延迟（秒）</summary>
        public float AutoDestroyDelay => _autoDestroyDelay;
    }
}
