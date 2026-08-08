using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>按角色拓展列表顺序更新子拓展。</summary>
    internal sealed class MainExpander
    {
        private readonly IReadOnlyList<CharacterSubExpanderSO> _subExpanders;

        public MainExpander(
            CharacterRunTimeData blackboard,
            Transform characterTransform,
            CharacterExpanderListSO expanderList)
        {
            if (blackboard == null) throw new ArgumentNullException(nameof(blackboard));
            if (characterTransform == null) throw new ArgumentNullException(nameof(characterTransform));

            _subExpanders = expanderList == null
                ? Array.Empty<CharacterSubExpanderSO>()
                : expanderList.SubExpanders;

            for (int i = 0; i < _subExpanders.Count; i++)
            {
                CharacterSubExpanderSO subExpander = _subExpanders[i];
                if (subExpander == null)
                    throw new InvalidOperationException("角色拓展列表包含空的子拓展资产。");

                subExpander.Initialize(blackboard, characterTransform);
            }
        }

        /// <summary>按配置顺序更新所有子拓展。</summary>
        public void LogicUpdate()
        {
            for (int i = 0; i < _subExpanders.Count; i++)
            {
                _subExpanders[i].SubUpdate();
            }
        }
    }
}
