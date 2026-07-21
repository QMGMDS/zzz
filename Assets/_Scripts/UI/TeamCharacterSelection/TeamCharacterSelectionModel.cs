using System;

namespace SPUI
{
    /// <summary>
    /// 队伍卡片选择模型 - 持有当前选中索引并提供循环切换逻辑，选中变化时通知订阅者。
    /// </summary>
    public sealed class TeamCharacterSelectionModel
    {
        /// <summary>
        /// 当前选中的卡片索引。
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// 卡片总数。
        /// </summary>
        public int CardCount { get; }

        /// <summary>
        /// 选中索引发生变化时触发，参数为新的选中索引。
        /// </summary>
        public event Action<int> SelectionChanged;

        /// <summary>
        /// 构造队伍选择模型。
        /// </summary>
        /// <param name="cardCount">卡片总数，必须大于等于 2。</param>
        public TeamCharacterSelectionModel(int cardCount)
        {
            if (cardCount < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cardCount), cardCount, "cardCount 必须大于等于 2。");
            }

            CardCount = cardCount;
        }

        /// <summary>
        /// 切换到下一张卡片并通知订阅者。
        /// </summary>
        public void SelectNext()
        {
            CurrentIndex = (CurrentIndex + 1) % CardCount;
            SelectionChanged?.Invoke(CurrentIndex);
        }
    }
}
