using UnityEngine;

namespace SPEffects
{
    /// <summary>
    /// 特效销毁策略 - 控制特效实例生命周期结束时机。
    /// </summary>
    public enum EffectDestroyPolicy
    {
        /// <summary>延迟自动销毁</summary>
        AutoDestroy,

        /// <summary>由调用方通过实例句柄手动销毁</summary>
        Manual
    }

    /// <summary>
    /// 特效播放请求 - 描述特效 ID、世界空间姿态与生命周期策略。
    /// </summary>
    public readonly struct EffectPlayRequest
    {
        /// <summary>特效唯一 ID</summary>
        public string EffectId { get; }

        /// <summary>世界空间生成位置</summary>
        public Vector3 Position { get; }

        /// <summary>世界空间生成旋转</summary>
        public Quaternion Rotation { get; }

        /// <summary>实例缩放</summary>
        public Vector3 Scale { get; }

        /// <summary>可选跟随节点</summary>
        public Transform Parent { get; }

        /// <summary>销毁策略</summary>
        public EffectDestroyPolicy DestroyPolicy { get; }

        /// <summary>自动销毁延迟（秒）</summary>
        public float AutoDestroyDelay { get; }

        /// <summary>
        /// 创建特效播放请求。
        /// </summary>
        /// <param name="effectId">特效唯一 ID</param>
        /// <param name="position">世界空间生成位置</param>
        /// <param name="rotation">世界空间生成旋转</param>
        /// <param name="scale">实例缩放</param>
        /// <param name="parent">可选跟随节点</param>
        /// <param name="destroyPolicy">销毁策略</param>
        /// <param name="autoDestroyDelay">自动销毁延迟（秒）</param>
        public EffectPlayRequest(
            string effectId,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Transform parent,
            EffectDestroyPolicy destroyPolicy,
            float autoDestroyDelay)
        {
            EffectId = effectId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Parent = parent;
            DestroyPolicy = destroyPolicy;
            AutoDestroyDelay = autoDestroyDelay;
        }
    }

    /// <summary>
    /// 特效实例句柄 - 向调用方提供只读实例信息与显式销毁入口。
    /// </summary>
    public interface IEffectInstance
    {
        /// <summary>实例是否仍然存活</summary>
        bool IsAlive { get; }

        /// <summary>特效实例对象</summary>
        GameObject GameObject { get; }

        /// <summary>
        /// 销毁特效实例。
        /// </summary>
        void Destroy();
    }
}
