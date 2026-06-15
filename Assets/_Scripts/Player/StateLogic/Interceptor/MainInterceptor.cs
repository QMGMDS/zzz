namespace SPPlayer
{
    /// <summary>
    /// 主拦截处理器——负责在状态逻辑之前执行全局优先级的状态转移。
    /// 让逻辑层的状态与状态之间不必相互认识。
    /// 遍历拦截器列表，谁先抢到就算谁的，每帧至多成功一次拦截。
    /// </summary>
    public class MainInterceptor
    {
        private readonly PlayerController _player;
        private readonly StateInterceptorSO[] _interceptors;

        /// <summary>
        /// 创建全局拦截处理器
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <param name="interceptors">拦截器数组（按优先级排列，索引越小优先级越高）</param>
        public MainInterceptor(PlayerController player, StateInterceptorSO[] interceptors)
        {
            _player = player;
            _interceptors = interceptors ?? new StateInterceptorSO[0];
        }

        /// <summary>
        /// 尝试处理全局拦截——依次遍历拦截器，首个返回 true 的拦截器触发状态切换。
        /// 每次调用至多一次成功拦截（一帧只能被抢一次）。
        /// </summary>
        /// <param name="currentState">当前激活的状态</param>
        /// <returns>true = 发生了拦截</returns>
        public bool TryProcessInterrupts(BaseState currentState)
        {
            if (_interceptors == null || _interceptors.Length == 0)
                return false;

            for (int i = 0; i < _interceptors.Length; i++)
            {
                var interceptor = _interceptors[i];
                if (interceptor == null) continue;

                if (interceptor.TryIntercept(_player, currentState, out var nextState))
                {
                    _player.StateMachine.ChangeState(nextState);
                    return true;
                }
            }

            return false;
        }
    }
}
