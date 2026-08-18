using UnityEngine;

namespace SPAI.Core
{
    /// <summary>
    /// 感知工具 - XZ 平面距离与视锥判定 忽略高度差 纯静态零依赖
    /// </summary>
    internal static class PerceptionUtility
    {
        /// <summary>
        /// 判定目标是否处于视野内 - 需同时满足视野距离与视锥角 无遮挡判定
        /// </summary>
        /// <returns>目标是否可见</returns>
        public static bool IsInViewCone(Vector3 selfPosition, Vector3 selfForward, Vector3 targetPosition, float viewDistance, float viewAngle)
        {
            Vector3 offset = targetPosition - selfPosition;
            offset.y = 0f;

            float sqrDistance = offset.sqrMagnitude;
            if (sqrDistance > viewDistance * viewDistance)
                return false;

            // 与目标重合视为可见 零向量无夹角定义
            if (sqrDistance <= 0f)
                return true;

            Vector3 forward = selfForward;
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0f)
                return true;

            float angle = Vector3.Angle(forward, offset);
            return angle <= viewAngle * 0.5f;
        }

        /// <summary>
        /// 计算两点 XZ 平面距离 单位 米
        /// </summary>
        public static float DistanceXZ(Vector3 from, Vector3 to)
        {
            Vector3 offset = to - from;
            offset.y = 0f;
            return offset.magnitude;
        }

        /// <summary>
        /// 计算起点指向终点的 XZ 平面归一化方向 XY 分量分别对应世界 XZ 轴 两点重合时返回零向量
        /// </summary>
        public static Vector2 DirectionXZ(Vector3 from, Vector3 to)
        {
            Vector3 offset = to - from;
            offset.y = 0f;
            if (offset.sqrMagnitude <= 0f)
                return Vector2.zero;

            offset.Normalize();
            return new Vector2(offset.x, offset.z);
        }
    }
}
