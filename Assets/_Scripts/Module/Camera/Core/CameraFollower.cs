using UnityEngine;

using SPCamera.Contract;

namespace SPCamera.Core
{
    /// <summary>
    /// 令摄像机平滑跟随目标 - 被挂载物体持续平滑跟随当前目标 Transform （XZ 平面跟随，Y 不变）
    /// 摄像机跟随移动需要考虑时序问题，每帧确保在目标完成本帧移动后再跟随
    /// </summary>
    [DefaultExecutionOrder(-50)]
    internal sealed class CameraFollower : MonoBehaviour, ISetCameraFollowTarget
    {
        [Header("目标")]
        [Tooltip("当前跟随目标")]
        [SerializeField] private Transform _targetCharacter;

        [Header("过渡参数")]
        [Tooltip("平滑时间，值越大过渡越慢")]
        [SerializeField] private float _smoothTime = 0.3f;

        [Tooltip("最大移动速度")]
        [SerializeField] private float _maxSpeed = 40f;

        private Vector3 _velocity;

        /// <inheritdoc />
        public void SetCameraFollowTarget(Transform target)
        {
            _targetCharacter = target;
            _velocity = Vector3.zero;
            SnapToTarget();
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (_targetCharacter == null) return;

            Vector3 current = transform.position;
            Vector3 target = _targetCharacter.position;
            target.y = current.y;
            transform.position = Vector3.SmoothDamp(current, target, ref _velocity, _smoothTime, _maxSpeed);
            _velocity.y = 0f;
        }

        private void SnapToTarget()
        {
            if (_targetCharacter == null) return;

            Vector3 target = _targetCharacter.position;
            transform.position = new Vector3(target.x, transform.position.y, target.z);
        }
    }
}
