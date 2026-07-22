using System;
using SPPlayerInput;
using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// 玩家队伍卡片切换控制器 - 读取切换角色输入驱动模型，并将模型的选中变化同步到视图。
    /// </summary>
    public sealed class TeamCharacterSelectionController : MonoBehaviour
    {
        [Header("视图引用")]
        [Tooltip("队伍卡片视图，Controller 通过此引用驱动其切换。")]
        [SerializeField] private TeamCharacterSelectionView _view;

        private TeamCharacterSelectionModel _model;

        private void Start()
        {
            if (_view == null)
            {
                throw new InvalidOperationException(
                    $"TeamCharacterSelectionController ({name}): _view 未设置。");
            }

            _model = new TeamCharacterSelectionModel(_view.CardCount);
            _model.SelectionChanged += OnSelectionChanged;

            _view.PlaySelection(_model.CurrentIndex);
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void Update()
        {
            SPPlayerInputCenter inputCenter = SPPlayerInputCenter.Instance;
            if (inputCenter == null)
            {
                return;
            }

            if (!inputCenter.CurrentFrameInput.SwitchCharacterPressed)
            {
                return;
            }

            _model.SelectNext();
        }

        private void OnSelectionChanged(int selectedIndex)
        {
            _view.PlaySelection(selectedIndex);
        }
    }
}
