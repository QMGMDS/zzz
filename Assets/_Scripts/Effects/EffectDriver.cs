using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 特效驱动器 - 监听角色黑板状态变化与动画进度，按配置时机释放特效。
    /// 纯 C# 驱动器，由 Root MonoBehaviour 在 LateUpdate 装配调度。
    /// </summary>
    public class EffectDriver
    {
        private readonly CharacterRunTimeData _blackboard;
        private readonly Transform _characterRoot;
        private readonly HashSet<int> _releasedIndices = new HashSet<int>();
        private readonly List<GameObject> _withStateInstances = new List<GameObject>();
        private uint _observedStateVersion;

        /// <summary>
        /// 创建特效驱动器。
        /// </summary>
        /// <param name="blackboard">角色运行时黑板</param>
        /// <param name="characterRoot">角色根 Transform</param>
        public EffectDriver(CharacterRunTimeData blackboard, Transform characterRoot)
        {
            if (blackboard == null) throw new ArgumentNullException(nameof(blackboard));
            if (characterRoot == null) throw new ArgumentNullException(nameof(characterRoot));

            _blackboard = blackboard;
            _characterRoot = characterRoot;
        }

        /// <summary>
        /// 每帧响应状态变化与动画进度，到达释放窗口时生成对应特效。
        /// 必须在 AnimationDriver.SyncAnimProgress 之后调用以保证读到当前帧进度。
        /// </summary>
        public void LogicUpdate()
        {
            CharacterRunTimeData blackboard = _blackboard;
            StateNodeSO stateNode = blackboard.CurrentStateNode;

            if (_observedStateVersion != blackboard.StateVersion)
            {
                _observedStateVersion = blackboard.StateVersion;
                _releasedIndices.Clear();
                CleanupWithStateInstances();
                ValidateCurrentEffects(stateNode);
            }

            EffectInfoSO effectSo = stateNode.Effects;
            if (effectSo == null) return;

            EffectInfo[] infos = effectSo.Effects;
            if (infos == null || infos.Length == 0) return;

            float normalizedTime = blackboard.AnimationNormalizedTime;
            if (stateNode.IsLooping)
                normalizedTime -= Mathf.Floor(normalizedTime);

            for (int i = 0; i < infos.Length; i++)
            {
                if (_releasedIndices.Contains(i)) continue;
                if (!infos[i].ReleaseWindow.Contains(normalizedTime)) continue;

                _releasedIndices.Add(i);
                SpawnEffect(infos[i]);
            }
        }

        /// <summary>
        /// 在状态切换时校验新状态的特效配置，发现非法配置立即抛出 InvalidOperationException。
        /// </summary>
        /// <param name="stateNode">当前状态节点</param>
        private void ValidateCurrentEffects(StateNodeSO stateNode)
        {
            EffectInfoSO effectSo = stateNode.Effects;
            if (effectSo == null) return;

            EffectInfo[] infos = effectSo.Effects;
            if (infos == null) return;
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i] == null)
                    throw new InvalidOperationException($"状态特效配置 Effects[{i}] 为空。");
                if (infos[i].Prefab == null)
                    throw new InvalidOperationException($"状态特效配置 Effects[{i}].Prefab 为空。");
                if (!string.IsNullOrEmpty(infos[i].ParentBoneName)
                    && _characterRoot.Find(infos[i].ParentBoneName) == null)
                    throw new InvalidOperationException(
                        $"状态特效配置 Effects[{i}] 指定的骨骼 \"{infos[i].ParentBoneName}\" 在角色层级中不存在。");
            }
        }

        /// <summary>
        /// 按 EffectInfo 配置生成特效实例。配置已在状态入口校验，此函数保持逻辑纯净。
        /// </summary>
        /// <param name="info">单个特效配置</param>
        private void SpawnEffect(EffectInfo info)
        {
            Transform parent = string.IsNullOrEmpty(info.ParentBoneName)
                ? _characterRoot
                : _characterRoot.Find(info.ParentBoneName);

            GameObject instance = UnityEngine.Object.Instantiate(info.Prefab, parent, false);
            Transform t = instance.transform;
            t.localPosition = info.LocalPosition;
            t.localRotation = Quaternion.Euler(info.LocalRotation);
            t.localScale = info.LocalScale;

            if (!info.AttachToParent)
                t.SetParent(null, true);

            switch (info.DestroyPolicy)
            {
                case EffectDestroyPolicy.AutoDestroy:
                    UnityEngine.Object.Destroy(instance, info.AutoDestroyDelay);
                    break;
                case EffectDestroyPolicy.WithState:
                    _withStateInstances.Add(instance);
                    break;
                case EffectDestroyPolicy.Manual:
                    break;
            }
        }

        /// <summary>
        /// 销毁所有随状态存活的特效实例并清空集合。
        /// </summary>
        private void CleanupWithStateInstances()
        {
            for (int i = 0; i < _withStateInstances.Count; i++)
            {
                if (_withStateInstances[i] != null)
                    UnityEngine.Object.Destroy(_withStateInstances[i]);
            }
            _withStateInstances.Clear();
        }
    }
}