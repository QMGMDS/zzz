using UnityEngine;

using SPFramework.Service;

namespace SPCamera.Contract
{
    /// <summary>
    /// 设置目标行为 - 设置摄像机跟随的目标，摄像机自行平滑移动到新目标
    /// </summary>
    public interface ISetCameraFollowTarget : IModuleService
    {
        /// <summary>
        /// 设置新的摄像机跟随目标
        /// </summary>
        /// <param name="target">跟随目标 Transform</param>
        void SetCameraFollowTarget(Transform target);
    }
}
