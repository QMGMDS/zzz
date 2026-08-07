using UnityEngine;

namespace SPCamera
{
    /// <summary>
    /// 平滑摄像机目标 - 挂载于 CameraLook 物体，持续平滑跟随当前目标 Transform。
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class SmoothCameraTarget : MonoBehaviour
    {
        [Header("目标")]
        [Tooltip("当前跟随目标。")]
        [SerializeField] private Transform _targetCharacter;

        [Header("过渡参数")]
        [Tooltip("平滑时间，值越大过渡越慢。")]
        [SerializeField] private float _smoothTime = 0.3f;

        [Tooltip("最大移动速度，防止远距离切换时过渡过慢。")]
        [SerializeField] private float _maxSpeed = 40f;

        private Vector3 _velocity;

        private void Start()
        {
            SnapToTarget();
        }

        private void Update()
        {
            if (_targetCharacter == null) return;

            Vector3 current = transform.position;
            Vector3 target = _targetCharacter.position;
            target.y = current.y;
            transform.position = Vector3.SmoothDamp(current, target, ref _velocity, _smoothTime, _maxSpeed);
            _velocity.y = 0f;
        }

        /// <summary>
        /// 设置新的摄像机跟随目标。
        /// </summary>
        /// <param name="target">新的跟随目标 Transform</param>
        public void SetTarget(Transform target)
        {
            _targetCharacter = target;
            _velocity = Vector3.zero;
            SnapToTarget();
        }

        private void SnapToTarget()
        {
            if (_targetCharacter == null) return;

            Vector3 target = _targetCharacter.position;
            transform.position = new Vector3(target.x, transform.position.y, target.z);
        }
    }
}
