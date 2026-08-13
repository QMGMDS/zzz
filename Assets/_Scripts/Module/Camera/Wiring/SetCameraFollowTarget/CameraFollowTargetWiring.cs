using UnityEngine;

using SPCamera.Contract;
using SPCamera.Core;
using SPFramework.Service;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 摄像机跟随目标接线胶水 - 将摄像机跟随器注册到模块服务中心
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class CameraFollowTargetWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("摄像机跟随器，通常与胶水挂于同一根物体")]
        [SerializeField] private CameraFollower _follower;

        private void Awake()
        {
            if (_follower != null)
                ModuleServiceHub.Register<ISetCameraFollowTarget>(_follower);
        }

        private void OnDestroy()
        {
            ModuleServiceHub.Unregister<ISetCameraFollowTarget>();
        }
    }
}

