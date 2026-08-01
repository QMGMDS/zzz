using System;
using System.Collections.Generic;
using SPEffects;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 特效触发驱动器 - 检测动画进度窗口并提交特效播放请求。
    /// </summary>
    public sealed class EffectTriggerDriver
    {
        private readonly CharacterRunTimeData _blackboard;
        private readonly Transform _characterRoot;
        private readonly IEffectService _effectService;
        private readonly HashSet<int> _releasedIndices = new HashSet<int>();
        private uint _observedStateVersion;
        private float _previousNormalizedTime;
        private bool _hasPreviousProgress;

        /// <summary>
        /// 创建特效触发驱动器。
        /// </summary>
        /// <param name="blackboard">角色运行时黑板</param>
        /// <param name="characterRoot">角色根节点</param>
        /// <param name="effectService">特效服务</param>
        public EffectTriggerDriver(
            CharacterRunTimeData blackboard,
            Transform characterRoot,
            IEffectService effectService)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _characterRoot = characterRoot ?? throw new ArgumentNullException(nameof(characterRoot));
            _effectService = effectService ?? throw new ArgumentNullException(nameof(effectService));
        }

        /// <summary>
        /// 同步动画进度并检测触发窗口。
        /// 必须在 AnimationDriver.SyncAnimProgress 之后调用，黑板中的归一化时间由动画层统一归一化。
        /// </summary>
        public void LogicUpdate()
        {
            StateNodeSO stateNode = _blackboard.CurrentStateNode;
            if (stateNode == null)
                return;

            EffectTrigger[] triggers = stateNode.EffectTriggers;
            if (triggers == null || triggers.Length == 0)
                return;

            if (_observedStateVersion != _blackboard.StateVersion)
            {
                _observedStateVersion = _blackboard.StateVersion;
                ResetReleaseTracking();
                ValidateCurrentTriggers(stateNode, triggers);
            }

            float currentTime = _blackboard.AnimationNormalizedTime;
            bool isLooping = stateNode.IsLooping;
            float previousTime = _hasPreviousProgress ? _previousNormalizedTime : 0f;
            int crossedLoopCount = isLooping && _hasPreviousProgress
                ? GetCrossedLoopCount(_previousNormalizedTime, currentTime)
                : 0;

            if (crossedLoopCount > 0)
                _releasedIndices.Clear();

            for (int i = 0; i < triggers.Length; i++)
            {
                if (_releasedIndices.Contains(i))
                    continue;
                if (!WasReleaseTimeReached(
                        triggers[i].ReleaseTime,
                        previousTime,
                        currentTime,
                        crossedLoopCount))
                {
                    continue;
                }

                _releasedIndices.Add(i);
                SubmitRequest(triggers[i]);
            }

            _previousNormalizedTime = currentTime;
            _hasPreviousProgress = true;
        }

        /// <summary>
        /// 解绑驱动器 - 清空释放记录，不销毁任何特效实例。
        /// </summary>
        public void Cleanup()
        {
            ResetReleaseTracking();
        }

        private void ResetReleaseTracking()
        {
            _releasedIndices.Clear();
            _previousNormalizedTime = 0f;
            _hasPreviousProgress = false;
        }

        private void ValidateCurrentTriggers(StateNodeSO stateNode, EffectTrigger[] triggers)
        {
            for (int i = 0; i < triggers.Length; i++)
            {
                EffectTrigger trigger = triggers[i];
                if (trigger == null)
                    throw new InvalidOperationException($"状态 {stateNode.name} 的 EffectTriggers[{i}] 为空。");
                if (string.IsNullOrWhiteSpace(trigger.EffectId))
                    throw new InvalidOperationException($"状态 {stateNode.name} 的 EffectTriggers[{i}] 未设置 EffectId。");
                if (trigger.ReleaseTime < 0f || trigger.ReleaseTime > 1f)
                    throw new InvalidOperationException($"状态 {stateNode.name} 的 EffectTriggers[{i}] 释放时刻非法。");
            }
        }

        private void SubmitRequest(EffectTrigger trigger)
        {
            Vector3 position;
            Quaternion rotation;
            Transform parent = null;

            switch (trigger.Space)
            {
                case EffectTriggerSpace.World:
                    position = trigger.PositionOffset;
                    rotation = Quaternion.Euler(trigger.RotationOffset);
                    break;
                case EffectTriggerSpace.CharacterRoot:
                    position = _characterRoot.TransformPoint(trigger.PositionOffset);
                    rotation = _characterRoot.rotation * Quaternion.Euler(trigger.RotationOffset);
                    parent = trigger.AttachToSource ? _characterRoot : null;
                    break;
                default:
                    throw new InvalidOperationException($"未知特效触发空间：{trigger.Space}。");
            }

            var request = new EffectPlayRequest(
                trigger.EffectId,
                position,
                rotation,
                trigger.Scale,
                parent,
                trigger.DestroyPolicy,
                trigger.AutoDestroyDelay);
            _effectService.Play(request);
        }

        private static bool WasReleaseTimeReached(
            float releaseTime,
            float previousTime,
            float currentTime,
            int crossedLoopCount)
        {
            if (crossedLoopCount > 0)
                return IsReleaseTimeInLoop(
                    releaseTime,
                    previousTime,
                    currentTime,
                    crossedLoopCount);

            return previousTime <= releaseTime && currentTime >= releaseTime;
        }

        private static bool IsReleaseTimeInLoop(
            float releaseTime,
            float previousTime,
            float currentTime,
            int crossedLoopCount)
        {
            if (releaseTime >= previousTime)
                return true;

            if (crossedLoopCount > 1)
                return true;

            return currentTime >= releaseTime;
        }

        private static int GetCrossedLoopCount(float previousRawTime, float currentRawTime)
        {
            if (currentRawTime < previousRawTime)
                return 1;

            int previousLoop = Mathf.FloorToInt(previousRawTime);
            int currentLoop = Mathf.FloorToInt(currentRawTime);
            return Mathf.Max(0, currentLoop - previousLoop);
        }
    }
}
