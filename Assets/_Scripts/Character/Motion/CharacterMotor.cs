using System;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色运动电机 - 统一提交 CharacterController 位移和角色旋转。
    /// </summary>
    public class CharacterMotor
    {
        private readonly CharacterController _controller;
        private readonly Transform _transform;

        /// <summary>
        /// 创建角色运动电机。
        /// </summary>
        /// <param name="controller">负责角色碰撞移动的 CharacterController</param>
        /// <param name="transform">角色根 Transform</param>
        public CharacterMotor(CharacterController controller, Transform transform)
        {
            _controller = controller != null ? controller : throw new ArgumentNullException(nameof(controller));
            _transform = transform != null ? transform : throw new ArgumentNullException(nameof(transform));
        }

        /// <summary>角色当前是否着地。</summary>
        public bool IsGrounded => _controller.isGrounded;

        /// <summary>
        /// 提交本帧唯一一次旋转和碰撞位移。
        /// </summary>
        /// <param name="displacement">本帧世界空间位移</param>
        /// <param name="rotation">本帧目标世界旋转</param>
        /// <returns>CharacterController 碰撞标记</returns>
        public CollisionFlags Move(Vector3 displacement, Quaternion rotation)
        {
            _transform.rotation = rotation;
            return _controller.Move(displacement);
        }
    }
}
