using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家动画配置 ScriptableObject——纯数据容器，
    /// 定义所有 PlayerStateType 到 AnimationStateConfig 的映射。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerAnimationConfig", menuName = "Player/PlayerAnimationConfig")]
    public class PlayerAnimationConfigSO : ScriptableObject
    {
        [Tooltip("状态→动画绑定列表，每个逻辑状态对应一个动画配置")]
        [SerializeField] private AnimationStateBinding[] _bindings;

        /// <summary>绑定列表——供 StateToAnimationAdapter 构建映射表</summary>
        public AnimationStateBinding[] Bindings => _bindings;
    }
}
