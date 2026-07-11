using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 动画驱动器——监听黑板的 CurrentStateNode，直接从节点读取动画数据并播放。
    /// 每帧将当前动画进度回写黑板。
    /// </summary>
    public class AnimationDriver
    {
        private readonly PlayerBrain _blackboard;
        private readonly AnimationSource _animSource;

        private StateNodeSO _lastNode;
        private bool _initialized;

        /// <summary>
        /// 创建动画驱动器
        /// </summary>
        /// <param name="blackboard">角色大脑黑板</param>
        /// <param name="animSource">动画源外观</param>
        public AnimationDriver(PlayerBrain blackboard, AnimationSource animSource)
        {
            _blackboard = blackboard;
            _animSource = animSource;
        }

        /// <summary>
        /// 检测状态节点变化，变化时播放对应动画
        /// </summary>
        public void Update()
        {
            if (_blackboard == null || _animSource == null) return;

            var currentNode = _blackboard.CurrentStateNode;

            if (!_initialized || currentNode != _lastNode)
            {
                _lastNode = currentNode;
                _initialized = true;

                if (currentNode != null && currentNode.Transition != null)
                    _animSource.Play(currentNode.Transition);
            }
        }

        /// <summary>
        /// 将 Animancer 最新动画进度回写黑板
        /// </summary>
        public void SyncAnimProgress()
        {
            if (_blackboard == null || _animSource == null || !_initialized) return;

            var time = _animSource.CurrentNormalizedTime;
            if (_lastNode != null && _lastNode.IsLooping)
                time -= Mathf.Floor(time);
            _blackboard.CurrentNormalizedTime = time;

            if (_lastNode != null && !_lastNode.IsLooping)
                _blackboard.AnimationCompleted = _animSource.CurrentNormalizedTime >= 1f;
            else
                _blackboard.AnimationCompleted = false;
        }
    }
}
