using System;
using System.Collections.Generic;

using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 收集并调度角色控制器胶水扩展
    /// </summary>
    internal sealed class CCWiringExtensionPipeline
    {
        private readonly CCRunTimeBlackboard _blackboard;
        private readonly Transform _characterTransform;
        private readonly IntentionWritePort _intentionWriter;
        private readonly List<ICCWiringExtension> _extensions = new List<ICCWiringExtension>();

        /// <summary>
        /// 创建胶水扩展调度管线
        /// </summary>
        public CCWiringExtensionPipeline(
            CCRunTimeBlackboard blackboard,
            Transform characterTransform,
            IReadOnlyList<MonoBehaviour> behaviours)
        {
            _blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            _characterTransform = characterTransform == null
                ? throw new ArgumentNullException(nameof(characterTransform))
                : characterTransform;
            _intentionWriter = new IntentionWritePort(_blackboard);
            CollectExtensions(behaviours);
        }

        /// <summary>
        /// 按组件顺序执行胶水扩展窗口
        /// </summary>
        public void LogicUpdate()
        {
            _intentionWriter.SetMoveAxis(Vector2.zero);

            CCWiringContext context = CreateContext();
            for (int i = 0; i < _extensions.Count; i++)
            {
                ICCWiringExtension extension = _extensions[i];
                if (extension is MonoBehaviour behaviour && (behaviour == null || !behaviour.isActiveAndEnabled))
                    continue;

                extension.UpdateWiring(context, _intentionWriter);
            }
        }

        private void CollectExtensions(IReadOnlyList<MonoBehaviour> behaviours)
        {
            if (behaviours == null)
                throw new ArgumentNullException(nameof(behaviours));

            _extensions.Clear();
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i] is ICCWiringExtension extension)
                    _extensions.Add(extension);
            }
        }

        private CCWiringContext CreateContext()
        {
            return new CCWiringContext(
                _characterTransform,
                _blackboard.CurrentStateId,
                _blackboard.StateVersion,
                _blackboard.AnimationTime,
                _blackboard.AnimationNormalizedTime);
        }
    }
}
