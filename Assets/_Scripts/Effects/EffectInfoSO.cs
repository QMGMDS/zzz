using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 状态特效配置容器 ScriptableObject - 承载一个状态下全部特效配置。
    /// 纯数据资产，不包含任何运行逻辑。
    /// </summary>
    [CreateAssetMenu(menuName = "SPCharacterController/Effects/EffectInfo", fileName = "EffectInfo")]
    public class EffectInfoSO : ScriptableObject
    {
        [Tooltip("该状态下所有特效配置")]
        public EffectInfo[] Effects;
    }
}