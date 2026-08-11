using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>角色控制器胶水窗口可读取的运行时快照</summary>
    internal readonly struct CCWiringContext
    {
        /// <summary>
        /// 创建胶水窗口运行时快照
        /// </summary>
        public CCWiringContext(
            Transform characterTransform,
            string currentStateId,
            uint stateVersion,
            float animationTime,
            float animationNormalizedTime)
        {
            CharacterTransform = characterTransform;
            CurrentStateId = currentStateId;
            StateVersion = stateVersion;
            AnimationTime = animationTime;
            AnimationNormalizedTime = animationNormalizedTime;
        }

        /// <summary>当前角色根 Transform</summary>
        public Transform CharacterTransform { get; }

        /// <summary>当前角色世界坐标</summary>
        public Vector3 Position => CharacterTransform.position;

        /// <summary>当前角色世界旋转</summary>
        public Quaternion Rotation => CharacterTransform.rotation;

        /// <summary>当前状态节点 Id</summary>
        public string CurrentStateId { get; }

        /// <summary>当前状态版本</summary>
        public uint StateVersion { get; }

        /// <summary>当前动画播放时刻</summary>
        public float AnimationTime { get; }

        /// <summary>当前动画归一化播放进度</summary>
        public float AnimationNormalizedTime { get; }
    }
}
