using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 离线烘焙的动画根运动曲线资产 - 存采样后的位移曲线，可在 Inspector 手动二次修改
    /// 位移以动画本地坐标系记录，运行时按角色朝向变换到世界空间后落位
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacter/Motion/RootMotionProfile", fileName = "RootMotionProfile")]
    internal sealed class RootMotionProfileSO : ScriptableObject
    {
        [SerializeField, Tooltip("烘焙进动画本地坐标系 X 轴的位移曲线（米）")]
        private AnimationCurve _localX = new AnimationCurve();

        [SerializeField, Tooltip("烘焙进动画本地坐标系 Z 轴的位移曲线（米）")]
        private AnimationCurve _localZ = new AnimationCurve();

        /// <summary>本地坐标系 X 轴位移曲线</summary>
        public AnimationCurve LocalX => _localX;

        /// <summary>本地坐标系 Z 轴位移曲线</summary>
        public AnimationCurve LocalZ => _localZ;
    }
}