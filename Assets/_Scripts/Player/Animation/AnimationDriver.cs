namespace SPPlayer
{
    /// <summary>
    /// 动画驱动器——监听黑板的 CurrentPlayerState，通过适配器获取动画配置并播放。
    /// 每帧将当前动画进度回写黑板。
    /// </summary>
    public class AnimationDriver
    {
        private readonly PlayerBrain _blackboard;
        private readonly AnimationSource _animFacade;
        private readonly StateToAnimationAdapter _adapter;

        private PlayerStateType _lastState;
        private AnimationStateConfig _lastConfig;
        private bool _initialized;

        /// <summary>
        /// 创建动画驱动器
        /// </summary>
        /// <param name="blackboard">角色大脑黑板</param>
        /// <param name="animFacade">动画源外观</param>
        /// <param name="adapter">状态→动画适配器</param>
        public AnimationDriver(PlayerBrain blackboard, AnimationSource animFacade, StateToAnimationAdapter adapter)
        {
            _blackboard = blackboard;
            _animFacade = animFacade;
            _adapter = adapter;
        }

        /// <summary>
        /// 每帧主驱动——由 PlayerController.Update 显式调用。
        /// 顺序：检测状态变化并播放动画 → 回写动画进度到黑板。
        /// </summary>
        public void Update()
        {
            if (_blackboard == null || _animFacade == null || _adapter == null) return;

            var currentState = _blackboard.CurrentPlayerState;

            if (!_initialized || currentState != _lastState)
            {
                _lastState = currentState;
                _initialized = true;

                if (_adapter.TryTranslate(currentState, out _lastConfig))
                    _animFacade.Play(_lastConfig.Clip, _lastConfig.FadeDuration, _lastConfig.Speed);
            }

            _blackboard.CurrentNormalizedTime = _animFacade.CurrentNormalizedTime;

            if (!_lastConfig.IsLooping)
                _blackboard.AnimationCompleted = _animFacade.CurrentNormalizedTime >= 1f;
        }
    }
}
