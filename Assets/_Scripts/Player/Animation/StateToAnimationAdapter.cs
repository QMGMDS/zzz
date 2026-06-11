using System.Collections.Generic;

namespace SPPlayer
{
    /// <summary>
    /// 状态→动画数据适配器——读取 PlayerAnimationConfigSO 构建映射表，将 PlayerStateType 翻译为 AnimationStateConfig。
    /// AnimationDriver 通过它获取动画驱动数据。
    /// </summary>
    public class StateToAnimationAdapter
    {
        private readonly Dictionary<PlayerStateType, AnimationStateConfig> _map;

        /// <summary>
        /// 创建适配器并构建映射表
        /// </summary>
        /// <param name="config">动画配置 SO</param>
        public StateToAnimationAdapter(PlayerAnimationConfigSO config)
        {
            var bindings = config != null ? config.Bindings : null;
            _map = new Dictionary<PlayerStateType, AnimationStateConfig>(bindings?.Length ?? 0);

            if (bindings != null)
            {
                foreach (var b in bindings)
                {
                    if (!_map.ContainsKey(b.StateType))
                        _map[b.StateType] = b.Config;
                }
            }
        }

        /// <summary>
        /// 将逻辑状态类型翻译为动画播放配置
        /// </summary>
        /// <param name="stateType">逻辑状态枚举值</param>
        /// <param name="config">输出的动画播放配置</param>
        /// <returns>true = 找到对应配置</returns>
        public bool TryTranslate(PlayerStateType stateType, out AnimationStateConfig config)
        {
            return _map.TryGetValue(stateType, out config);
        }
    }
}
