using Cinemachine;
using UnityEngine;

namespace CustomCameras
{
    /// <summary>
    /// 根据 POV 垂直角度动态调整摄像机距离，实现环视顶部/底部时的缩放效果
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CameraOrbitDistance : MonoBehaviour
    {
        [Header("角度阈值")]
        [Tooltip("底部仰角（度）")]
        [SerializeField] private float _minAngle = -15f;

        [Tooltip("中间仰角（度）")]
        [SerializeField] private float _midAngle = 0f;

        [Tooltip("顶部仰角（度）")]
        [SerializeField] private float _maxAngle = 45f;

        [Header("对应距离")]
        [Tooltip("底部时的摄像机距离")]
        [SerializeField] private float _bottomDistance = 1.9f;

        [Tooltip("中间时的摄像机距离")]
        [SerializeField] private float _midDistance = 3.1f;

        [Tooltip("顶部时的摄像机距离")]
        [SerializeField] private float _topDistance = 2.6f;

        [Header("平滑")]
        [Tooltip("距离变化平滑速度")]
        [SerializeField] private float _smoothSpeed = 5f;

        private CinemachineVirtualCamera _vcam;
        private CinemachinePOV _pov;
        private CinemachineFramingTransposer _transposer;
        private float _currentDistance;

        private void Awake()
        {
            _vcam = GetComponent<CinemachineVirtualCamera>();
            _pov = _vcam.GetCinemachineComponent<CinemachinePOV>();
            _transposer = _vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            _currentDistance = _transposer.m_CameraDistance;
        }

        private void LateUpdate()
        {
            if (_pov == null || _transposer == null) return;

            float angle = _pov.m_VerticalAxis.Value;
            float targetDistance = CalculateTargetDistance(angle);
            _currentDistance = Mathf.Lerp(_currentDistance, targetDistance, Time.deltaTime * _smoothSpeed);
            _transposer.m_CameraDistance = _currentDistance;
        }

        /// <summary>
        /// 根据当前仰角计算目标摄像机距离，在两段关键帧之间线性插值
        /// </summary>
        private float CalculateTargetDistance(float angle)
        {
            if (angle <= _midAngle)
            {
                float t = Mathf.InverseLerp(_minAngle, _midAngle, angle);
                return Mathf.Lerp(_bottomDistance, _midDistance, t);
            }

            float t2 = Mathf.InverseLerp(_midAngle, _maxAngle, angle);
            return Mathf.Lerp(_midDistance, _topDistance, t2);
        }
    }
}
