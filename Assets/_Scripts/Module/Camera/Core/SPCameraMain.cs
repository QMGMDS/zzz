using UnityEngine;

namespace SPCamera.Core
{
    /// <summary>
    /// 摄像机主入口 - 独立自主的摄像机实体
    /// </summary>
    [DefaultExecutionOrder(-50)]
    internal sealed class SPCameraMain : MonoBehaviour
    {
        [Header("参考系")]
        [Tooltip("持 yaw 的参考物体，留空时使用本物体自身")]
        [SerializeField] private Transform _reference;

        [Header("目标")]
        [Tooltip("当前跟随目标")]
        [SerializeField] private Transform _targetCharacter;

        [Header("过渡参数")]
        [Tooltip("平滑时间，值越大过渡越慢")]
        [SerializeField] private float _smoothTime = 0.3f;

        [Tooltip("最大移动速度")]
        [SerializeField] private float _maxSpeed = 40f;

        private Vector3 _velocity;

        private Transform Reference => _reference != null ? _reference : transform;

        /// <summary>
        /// 将输入方向转换为世界 XZ 平面方向 - 以参考物体 yaw 为基准
        /// </summary>
        /// <param name="inputDirection">角色输入方向，调用者需保证其合法</param>
        /// <returns>世界 XZ 平面方向</returns>
        public Vector2 ConvertCameraTransform(Vector2 inputDirection)
        {
            // 不对 inputDirection 进行防御性处理，方法的调用者需保证其合法

            Quaternion yawRotation = Quaternion.Euler(0f, Reference.eulerAngles.y, 0f);

            Vector3 forward = yawRotation * Vector3.forward;
            Vector3 right = yawRotation * Vector3.right;

            Vector3 world = forward * inputDirection.y + right * inputDirection.x;
            Vector2 xz = new Vector2(world.x, world.z);
            return xz.normalized;
        }

        /// <summary>
        /// 切换跟随目标 - 清空过渡速度并立即吸附，随后逐帧平滑跟随
        /// </summary>
        /// <param name="target">新跟随目标</param>
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
