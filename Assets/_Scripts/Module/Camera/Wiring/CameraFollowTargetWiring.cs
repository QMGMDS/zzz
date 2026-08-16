using UnityEngine;

using SPCamera.Contract;
using SPCamera.Core;
using SPFramework.Service;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 摄像机跟随目标接线胶水 - 实现跟随契约并注册到模块服务中心，调用转发给摄像机主入口
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class CameraFollowTargetWiring : MonoBehaviour, ISetCameraFollowTarget
    {
        [Header("接线")]
        [Tooltip("摄像机主入口")]
        [SerializeField] private SPCameraMain _main;

        private void OnEnable()
        {
            if (_main != null)
                ModuleServiceHub.Register<ISetCameraFollowTarget>(this);
        }

        private void OnDisable()
        {
            if (_main != null)
                ModuleServiceHub.Unregister<ISetCameraFollowTarget>(this);
        }

        /// <inheritdoc />
        public void SetCameraFollowTarget(Transform target)
        {
            // 主入口未接好线时静默跳过，与空源保护语义一致
            if (_main != null)
                _main.SetCameraFollowTarget(target);
        }
    }
}
