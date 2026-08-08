using System;
using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>角色子拓展 ScriptableObject 基类。</summary>
    public abstract class CharacterSubExpanderSO : ScriptableObject
    {
        /// <summary>当前绑定角色的运行时黑板。</summary>
        protected CharacterRunTimeData Blackboard { get; private set; }
        /// <summary>当前绑定角色的 Transform。</summary>
        protected Transform CharacterTransform { get; private set; }

        internal void Initialize(CharacterRunTimeData blackboard, Transform characterTransform)
        {
            Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            CharacterTransform = characterTransform ?? throw new ArgumentNullException(nameof(characterTransform));
        }

        internal void SubUpdate()
        {
            if (Blackboard == null || CharacterTransform == null)
                throw new InvalidOperationException($"角色子拓展未绑定角色：{name}");

            OnSubUpdate();
        }

        /// <summary>执行子拓展当前帧逻辑。</summary>
        protected abstract void OnSubUpdate();
    }
}
