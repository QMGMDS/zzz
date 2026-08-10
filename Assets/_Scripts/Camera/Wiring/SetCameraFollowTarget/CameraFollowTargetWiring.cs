using UnityEngine;

using SPCamera.Core;

namespace SPCamera.Wiring
{
    /// <summary>
    /// 摄像机跟随目标接线胶水 - 将内部摄像机跟随器注入对外信箱
    /// </summary>
    [DefaultExecutionOrder(-380)]
    internal sealed class CameraFollowTargetWiring : MonoBehaviour
    {
        [Header("接线")]
        [Tooltip("摄像机跟随器，通常与胶水挂于同一根物体")]
        [SerializeField] private CameraFollower _follower;

        [Tooltip("存放信箱")]
        [SerializeField] private CameraFollowTargetProviderSO _providerSO;

        private void Awake()
        {
            if (_follower != null && _providerSO != null)
                _providerSO.Bind(_follower);   // 隐式转为 ISetCameraFollowTarget
        }

        private void OnDestroy()
        {
            if (_providerSO != null) _providerSO.Clear();
        }
    }
}

