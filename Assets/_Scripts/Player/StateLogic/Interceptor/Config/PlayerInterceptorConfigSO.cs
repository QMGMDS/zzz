using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家拦截器配置 SO
    /// 定义全局状态拦截器的优先级列表。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerInterceptorConfig", menuName = "Player/PlayerInterceptorConfig")]
    public class PlayerInterceptorConfigSO : ScriptableObject
    {
        [Header("全局拦截器")]
        [Tooltip("按优先级从上到下排列的拦截器列表——谁排前面谁先抢")]
        [SerializeField] private StateInterceptorSO[] _globalInterceptors;

        /// <summary>全局拦截器数组（按优先级排列，索引越小优先级越高）</summary>
        public StateInterceptorSO[] GlobalInterceptors => _globalInterceptors;
    }
}
