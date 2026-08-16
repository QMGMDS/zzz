using UnityEngine;

using SPFramework.Service;

namespace SPCamera.Contract
{
    /// <summary>
    /// 转换行为 - 平面方向与摄像机坐标系相关联，产出世界 XZ 方向
    /// 注意：不是产出摄像机物体坐标系下的方向，而是世界方向，只不过关联了摄像机
    /// </summary>
    public interface IConvertCameraTransform : IModuleService
    {
        /// <summary>
        /// 将平面方向关联摄像机，产出世界 XZ 方向
        /// </summary>
        /// <param name="inputDirection">输入模块产出的平面方向</param>
        /// <returns>世界 XZ 方向</returns>
        Vector2 ConvertCameraTransform(Vector2 inputDirection);
    }
}
