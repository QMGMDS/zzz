using UnityEngine;

namespace SPCamera.Contract
{
    /// <summary>
    /// 坐标转换器 - 将输入模块产出的平面方向转换为角色可直接消费的世界 XZ 目标方向。
    /// </summary>
    public interface ICoordinateConverter
    {
        /// <summary>
        /// 将输入平面方向转换为世界 XZ 目标方向。
        /// </summary>
        /// <param name="inputDirection">输入模块产出的平面方向</param>
        /// <returns>角色可直接消费的世界 XZ 目标方向，XY 分量分别对应世界 XZ 轴。</returns>
        Vector2 ConvertToWorldMoveDirection(Vector2 inputDirection);
    }
}
