using System;
using System.Collections;
using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// 玩家队伍卡片视图 - 展示队伍卡片并循环切换选中状态，切换动作由外部 Controller 驱动。
    /// </summary>
    public sealed class TeamCharacterSelectionView : MonoBehaviour
    {
        [Header("人物卡片引用")]
        [Tooltip("参与切换的卡片集合 - 至少需要 2 张，索引 0 为选中位，索引 1 为覆盖位基准。")]
        [SerializeField] private RectTransform[] _cards;

        [Header("过渡动画参数")]
        [Tooltip("卡片切换过渡时长 - 单位秒。")]
        [SerializeField, Range(0.1f, 2f)] private float _transitionDuration = 0.45f;

        [Tooltip("选中卡片入场时的浮动幅度 - 单位像素。")]
        [SerializeField] private float _floatAmplitude = 64f;

        [Tooltip("浮动的正弦周期数 - 值越大抖动次数越多。")]
        [SerializeField, Range(0f, 10f)] private float _floatCycles = 3f;

        private Vector2[] _basePositions;
        private Vector2[] _transitionStartPositions;

        private int _selectedIndex;
        private float _selectedPositionX;
        private float _coveredPositionX;
        private Coroutine _transitionCoroutine;

        private int _cardCount;

        /// <summary>
        /// 卡片总数。
        /// </summary>
        public int CardCount => _cards != null ? _cards.Length : 0;

        private void Awake()
        {
            if (_cards == null || _cards.Length < 2)
            {
                throw new InvalidOperationException(
                    $"TeamCharacterSelectionView ({name}): _cards 至少需要 2 张卡片。");
            }

            _cardCount = _cards.Length;

            _basePositions = new Vector2[_cardCount];
            _transitionStartPositions = new Vector2[_cardCount];

            for (int index = 0; index < _cardCount; index++)
            {
                _basePositions[index] = _cards[index].anchoredPosition;
            }

            _selectedPositionX = _basePositions[0].x;
            _coveredPositionX = _basePositions[1].x;

            ApplyStateImmediately();
        }

        private void OnDisable()
        {
            if (_transitionCoroutine == null)
            {
                return;
            }

            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
            ApplyStateImmediately();
        }

        /// <summary>
        /// 播放切换到指定卡片的过渡动画。
        /// </summary>
        /// <param name="selectedIndex">目标选中卡片索引。</param>
        public void PlaySelection(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _cardCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(selectedIndex), selectedIndex, "selectedIndex 超出卡片索引范围。");
            }

            _selectedIndex = selectedIndex;

            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
            }

            for (int index = 0; index < _cardCount; index++)
            {
                _transitionStartPositions[index] = _cards[index].anchoredPosition;
            }

            _transitionCoroutine = StartCoroutine(AnimateSelection());
        }

        private IEnumerator AnimateSelection()
        {
            float elapsedTime = 0f;
            while (elapsedTime < _transitionDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / _transitionDuration);
                float easeOutTime = 1f - Mathf.Pow(1f - normalizedTime, 3f);

                for (int index = 0; index < _cardCount; index++)
                {
                    float targetPositionX = index == _selectedIndex ? _selectedPositionX : _coveredPositionX;
                    Vector2 targetPosition = new Vector2(targetPositionX, _basePositions[index].y);
                    Vector2 position = Vector2.LerpUnclamped(
                        _transitionStartPositions[index],
                        targetPosition,
                        easeOutTime);

                    if (index == _selectedIndex)
                    {
                        float damping = 1f - normalizedTime;
                        float floatOffset = Mathf.Sin(normalizedTime * Mathf.PI * 2f * _floatCycles)
                                            * _floatAmplitude
                                            * damping;
                        position.x += floatOffset;
                    }

                    _cards[index].anchoredPosition = position;
                }

                yield return null;
            }

            ApplyStateImmediately();
            _transitionCoroutine = null;
        }

        private void ApplyStateImmediately()
        {
            for (int index = 0; index < _cardCount; index++)
            {
                float positionX = index == _selectedIndex ? _selectedPositionX : _coveredPositionX;
                _cards[index].anchoredPosition = new Vector2(positionX, _basePositions[index].y);
            }
        }
    }
}
