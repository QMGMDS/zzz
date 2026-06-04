using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 角色运动驱动器，集中管理旋转策略与 Root Motion 位移。
    /// 状态通过调用不同的旋转方法来声明当前帧的运动意图，
    /// 避免旋转逻辑在多个状态间重复。
    /// </summary>
    public class MotionDriver
    {
        private Transform _transform;
        private Camera _camera;
        private float _rotationVelocity;
        private Quaternion _lockedRotation;

        /// <summary>
        /// 初始化运动驱动器，绑定角色 Transform 和主摄像机
        /// </summary>
        /// <param name="transform">角色 Transform</param>
        /// <param name="camera">主摄像机</param>
        public void Initialize(Transform transform, Camera camera)
        {
            _transform = transform;
            _camera = camera;
        }

        /// <summary>
        /// 自由视角旋转：根据输入方向和摄像机朝向平滑旋转角色面朝方向。
        /// 由 WalkState / RunState 的 LateUpdate 调用。
        /// </summary>
        /// <param name="inputDir">输入方向（归一化 Vector2）</param>
        /// <param name="smoothTime">平滑时间（秒），默认 0.1</param>
        public void UpdateFreeLookRotation(Vector2 inputDir, float smoothTime = 0.1f)
        {
            if (_transform == null || _camera == null) return;
            if (inputDir.sqrMagnitude < 0.0001f) return;

            Vector3 cameraForward = _camera.transform.forward;
            Vector3 cameraRight = _camera.transform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            Vector3 worldMoveDir = (cameraForward * inputDir.y + cameraRight * inputDir.x).normalized;
            float targetAngle = Mathf.Atan2(worldMoveDir.x, worldMoveDir.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(
                _transform.eulerAngles.y,
                targetAngle,
                ref _rotationVelocity,
                smoothTime
            );
            _transform.eulerAngles = new Vector3(0f, smoothedAngle, 0f);
        }

        /// <summary>
        /// 锁敌旋转：平滑旋转角色面朝锁定目标。
        /// 由 NormalAttackState 的 LateUpdate 调用。
        /// </summary>
        /// <param name="target">锁定的目标 Transform，为 null 时跳过</param>
        /// <param name="smoothTime">平滑时间（秒），默认 0.1</param>
        public void UpdateLockEnemyRotation(Transform target, float smoothTime = 0.1f)
        {
            if (_transform == null || target == null) return;

            Vector3 dirToEnemy = target.position - _transform.position;
            dirToEnemy.y = 0f;
            if (dirToEnemy.sqrMagnitude <= 0.0001f) return;

            float targetAngle = Mathf.Atan2(dirToEnemy.x, dirToEnemy.z) * Mathf.Rad2Deg;
            float smoothedAngle = Mathf.SmoothDampAngle(
                _transform.eulerAngles.y,
                targetAngle,
                ref _rotationVelocity,
                smoothTime
            );
            _transform.eulerAngles = new Vector3(0f, smoothedAngle, 0f);
        }

        #region 修复动画根运动错误的旋转（貌似不需要修复，动画旋转是正确的）

        /// <summary>
        /// 快照当前旋转为锁定值。由 IdleState.Enter 调用一次。
        /// </summary>
        public void SnapCurrentRotation()
        {
            if (_transform == null) return;
            _lockedRotation = _transform.rotation;
        }

        /// <summary>
        /// 强制覆盖为之前快照的锁定旋转。由 IdleState.LateUpdate 每帧调用。
        /// </summary>
        public void ApplyLockedRotation()
        {
            if (_transform == null) return;
            _transform.rotation = _lockedRotation;
        }

        #endregion

        /// <summary>
        /// 应用 Root Motion 位移到 CharacterController。
        /// </summary>
        /// <param name="cc">角色 CharacterController</param>
        /// <param name="animator">角色 Animator</param>
        /// <param name="scale">Root Motion 缩放系数，1 为原始速度</param>
        public void ApplyRootMotion(CharacterController cc, Animator animator, float scale = 1f)
        {
            if (cc == null || animator == null) return;
            Vector3 delta = animator.deltaPosition * scale;
            cc.Move(delta);
        }
    }
}
