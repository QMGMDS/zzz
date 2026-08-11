using System;
using System.Collections.Generic;

using UnityEngine;

using Animancer;

namespace SPCharacter.Core
{
    /// <summary>
    /// 动画驱动器 - 使用指令源指令驱动角色动画
    /// 1) 监听黑板的状态变化，更新动画
    /// 2) 将当前动画进度回写黑板
    /// </summary>
    internal sealed class AnimationDriver
    {
        private readonly CCRunTimeBlackboard _blackboard;
        private readonly AnimationSource _animationSource;
        private readonly IReadOnlyDictionary<string, StateNodeSO> _nodesById;
        private uint _observedStateVersion;
        private bool _hasCompletionReported;

        /// <summary>
        /// 创建动画驱动器
        /// </summary>
        /// <param name="blackboard">角色运行时黑板</param>
        /// <param name="animancer">角色使用的 Animancer 组件</param>
        /// <param name="nodesById">状态机提供的只读节点解析表</param>
        public AnimationDriver(
            CCRunTimeBlackboard blackboard,
            AnimancerComponent animancer,
            IReadOnlyDictionary<string, StateNodeSO> nodesById)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _animationSource = new AnimationSource(animancer);
            _nodesById = nodesById ?? throw new ArgumentNullException(nameof(nodesById));
        }

        /// <summary>
        /// 检测状态节点变化，下达动画指令
        /// </summary>
        public void LogicUpdate()
        {
            if (_observedStateVersion == _blackboard.StateVersion)
                return;

            StateNodeSO stateNode = ResolveCurrentNode();
            _animationSource.Play(stateNode.Animation);

            float entryNormalizedTime = NormalizePlaybackTime(
                _animationSource.CurrentNormalizedTime,
                stateNode.IsLooping);

            _blackboard.BeginAnimationProgress(
                _animationSource.CurrentTime,
                entryNormalizedTime);

            _observedStateVersion = _blackboard.StateVersion;
            _hasCompletionReported = false;
        }

        /// <summary>
        /// 将最新动画进度回写黑板，确保动画数据新鲜
        /// </summary>
        public void SyncAnimProgress()
        {
            StateNodeSO stateNode = ResolveCurrentNode();
            float normalizedTime = NormalizePlaybackTime(
                _animationSource.CurrentNormalizedTime,
                stateNode.IsLooping);

            _blackboard.PublishAnimationProgress(
                _animationSource.CurrentTime,
                normalizedTime);

            if (_hasCompletionReported || stateNode.IsLooping)
                return;
            if (_blackboard.AnimationNormalizedTime < 1f)
                return;

            _blackboard.ReportAnimationCompleted();
            _hasCompletionReported = true;
        }

        private static float NormalizePlaybackTime(float normalizedTime, bool isLooping)
        {
            if (!isLooping)
                return normalizedTime;

            normalizedTime -= Mathf.Floor(normalizedTime);
            return normalizedTime >= 1f ? 0f : normalizedTime;
        }

        /// <summary>
        /// 按黑板当前状态 Id 反查节点数据
        /// </summary>
        /// <returns>黑板当前状态 Id 对应的节点</returns>
        /// <exception cref="InvalidOperationException">黑板没有当前状态 Id 或 Id 无对应节点</exception>
        private StateNodeSO ResolveCurrentNode()
        {
            string id = _blackboard.CurrentStateId;
            if (string.IsNullOrEmpty(id))
                throw new InvalidOperationException("黑板没有当前状态 Id");
            if (!_nodesById.TryGetValue(id, out StateNodeSO node) || node == null)
                throw new InvalidOperationException($"黑板当前状态 Id 无对应节点：{id}");
            return node;
        }
    }
}
