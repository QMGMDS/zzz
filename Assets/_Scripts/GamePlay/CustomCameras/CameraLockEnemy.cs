using Cinemachine;
using Core.Event;
using UnityEngine;

namespace CustomCameras
{
    /// <summary>
    /// 鼠标中键锁敌摄像机组件，挂载在 Cinemachine Virtual Camera 上。
    /// 锁定最近 Enemy 标签敌人，通过调整 POV 水平轴让摄像机水平正对目标，
    /// 锁定时冻结 POV 输入，解锁后恢复鼠标控制。
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CameraLockEnemy : MonoBehaviour
    {
        [Header("锁敌参数")]
        [Tooltip("搜敌球形半径（米）")]
        [SerializeField] private float _lockRange = 15f;

        [Tooltip("目标标签")]
        [SerializeField] private string _targetTag = "Enemy";

        [Header("事件通道")]
        [Tooltip("锁敌切换事件通道，收到事件时执行 ToggleLock")]
        [SerializeField] private VoidEventChannelSO _lockToggleChannel;

        [Tooltip("锁目标变化事件通道，锁定/解锁时 Broadcast 当前目标 Transform")]
        [SerializeField] private TransformEventChannelSO _lockTargetChangedChannel;

        [Header("阻尼")]
        [Tooltip("锁定时摄像机跟踪敌人的平滑速度")]
        [SerializeField] private float _lockDamping = 8f;

        private CinemachineVirtualCamera _vcam;
        private CinemachinePOV _pov;
        private float _originalHorizontalMaxSpeed;
        private float _originalVerticalMaxSpeed;
        private Transform _currentTarget;

        /// <summary>当前是否处于锁敌状态</summary>
        public bool IsLocking => _currentTarget != null;

        /// <summary>当前锁定目标的 Transform，未锁定时为 null</summary>
        public Transform CurrentTarget => _currentTarget;

        private void OnEnable()
        {
            if (_lockToggleChannel != null)
            {
                _lockToggleChannel.Subscribe(ToggleLock);
            }
        }

        private void OnDisable()
        {
            if (_lockToggleChannel != null)
            {
                _lockToggleChannel.Unsubscribe(ToggleLock);
            }
        }

        private void Awake()
        {
            _vcam = GetComponent<CinemachineVirtualCamera>();
            _pov = _vcam.GetCinemachineComponent<CinemachinePOV>();
            if (_pov != null)
            {
                _originalHorizontalMaxSpeed = _pov.m_HorizontalAxis.m_MaxSpeed;
                _originalVerticalMaxSpeed = _pov.m_VerticalAxis.m_MaxSpeed;
            }
        }

        private void LateUpdate()
        {
            if (!IsLocking) return;

            if (_currentTarget == null)
            {
                Unlock();
                return;
            }

            if (_pov == null) return;

            Camera cam = GetActiveCamera();
            if (cam == null) return;

            UpdatePOVToTrackTarget(cam);
        }

        /// <summary>
        /// 以摄像机当前实际朝向为基准，计算水平方向还需旋转多少度
        /// 才能正对敌人，将差值累加到 POV 水平轴并平滑过渡。
        /// 垂直轴不调整，保持玩家原有视角。
        /// </summary>
        private void UpdatePOVToTrackTarget(Camera cam)
        {
            Vector3 camPos = cam.transform.position;
            Vector3 camForward = cam.transform.forward;
            Vector3 dirToEnemy = (_currentTarget.position - camPos).normalized;

            Vector3 camFlat = Vector3.ProjectOnPlane(camForward, Vector3.up);
            Vector3 enemyFlat = Vector3.ProjectOnPlane(dirToEnemy, Vector3.up);

            float deltaH = 0f;
            if (camFlat.sqrMagnitude > 0.0001f && enemyFlat.sqrMagnitude > 0.0001f)
                deltaH = Vector3.SignedAngle(camFlat, enemyFlat, Vector3.up);

            _pov.m_HorizontalAxis.Value += deltaH * Time.deltaTime * _lockDamping;
        }

        /// <summary>
        /// 切换锁敌状态：锁定则搜索并锁定最近敌人，已锁定则解锁
        /// </summary>
        public void ToggleLock()
        {
            if (IsLocking)
            {
                Unlock();
            }
            else
            {
                TryLock();
            }
        }

        private void TryLock()
        {
            Transform target = FindNearestEnemy();
            if (target == null) return;

            _currentTarget = target;
            BroadcastTargetChanged(target);

            if (_pov != null)
            {
                _pov.m_HorizontalAxis.m_MaxSpeed = 0f;
                _pov.m_VerticalAxis.m_MaxSpeed = 0f;
            }
        }

        private void Unlock()
        {
            _currentTarget = null;
            BroadcastTargetChanged(null);

            if (_pov != null)
            {
                _pov.m_HorizontalAxis.m_MaxSpeed = _originalHorizontalMaxSpeed;
                _pov.m_VerticalAxis.m_MaxSpeed = _originalVerticalMaxSpeed;
            }
        }

        private void BroadcastTargetChanged(Transform target)
        {
            if (_lockTargetChangedChannel != null)
            {
                _lockTargetChangedChannel.Raise(target);
            }
        }

        private Transform FindNearestEnemy()
        {
            Camera cam = GetActiveCamera();
            if (cam == null) return null;

            Vector3 camPos = cam.transform.position;

            Collider[] hits = Physics.OverlapSphere(camPos, _lockRange);
            Transform nearest = null;
            float nearestDist = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (!hit.CompareTag(_targetTag)) continue;

                float dist = (hit.transform.position - camPos).sqrMagnitude;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = hit.transform;
                }
            }

            return nearest;
        }

        private Camera GetActiveCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                CinemachineBrain brain = CinemachineCore.Instance.GetActiveBrain(0);
                if (brain != null)
                    cam = brain.OutputCamera;
            }

            return cam;
        }
    }
}
