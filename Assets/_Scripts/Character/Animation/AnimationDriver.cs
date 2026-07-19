using System;

namespace SPCharacterController
{
    /// <summary>
    /// 动画驱动器 - 使用指令源指令驱动角色动画。
    /// 1) 监听黑板的状态变化，更新动画。
    /// 2) 将当前动画进度回写黑板。
    /// </summary>
    public class AnimationDriver
    {
        private readonly CharacterRunTimeData _blackboard;
        private readonly AnimationSource _animationSource;
        private uint _observedStateVersion;
        private bool _completionReported;

        public AnimationDriver(CharacterRunTimeData blackboard, AnimationSource animationSource)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _animationSource = animationSource ?? throw new ArgumentNullException(nameof(animationSource));
        }

        /// <summary>
        /// 检测状态节点变化，下达动画指令
        /// </summary>
        public void LogicUpdate()
        {
            if (_observedStateVersion == _blackboard.StateVersion)
                return;

            StateNodeSO stateNode = _blackboard.CurrentStateNode;
            if (stateNode == null) throw new InvalidOperationException("黑板没有当前状态节点。");

            _animationSource.Play(stateNode.Animation);
            _observedStateVersion = _blackboard.StateVersion;
            _completionReported = false;
            _blackboard.ResetAnimationProgress();
        }

        /// <summary>
        /// 将最新动画进度回写黑板，确保动画数据新鲜
        /// </summary>
        public void SyncAnimProgress()
        {
            _blackboard.PublishAnimationProgress(
                _animationSource.CurrentTime,
                _animationSource.CurrentNormalizedTime);

            if (_completionReported || _blackboard.CurrentStateNode.IsLooping)
                return;
            if (_blackboard.AnimationNormalizedTime < 1f)
                return;

            _blackboard.ReportAnimationCompleted();
            _completionReported = true;
        }
    }
}
