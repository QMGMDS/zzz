using System;
using SPCharacterController;
using SPEvent;
using SPTeam;
using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 视图模型 - 订阅队伍数据源，按视图槽位投影状态并广播刷新，不依赖 UnityEngine 渲染层。
    /// </summary>
    public class PlayerHUD_ViewModel
    {
        private readonly TeamController _teamController;
        private readonly CharacterStats[] _stats;
        private bool _subscribed;

        private int _slot0DataIndex = -1;
        private float _redSetpoint0;

        /// <summary>投影重算后广播，View 据此刷新。</summary>
        public event Action Updated;

        /// <summary>槽位 0 头像。</summary>
        public Sprite Avatar0 { get; private set; }

        /// <summary>槽位 1 头像。</summary>
        public Sprite Avatar1 { get; private set; }

        /// <summary>槽位 2 头像。</summary>
        public Sprite Avatar2 { get; private set; }

        /// <summary>槽位 0 血条绿条 fillAmount（0-1）。</summary>
        public float HpFill0 { get; private set; }

        /// <summary>槽位 1 血条绿条 fillAmount（0-1）。</summary>
        public float HpFill1 { get; private set; }

        /// <summary>槽位 2 血条绿条 fillAmount（0-1）。</summary>
        public float HpFill2 { get; private set; }

        /// <summary>槽位 0 红底血条的瞬时目标 fillAmount，仅上升或角色切换时更新；下降时保留由 View 插值追下来。</summary>
        public float RedSetpoint0 => _redSetpoint0;

        /// <summary>
        /// 构造视图模型，注入队伍控制器并缓存各角色运行时属性。
        /// </summary>
        /// <param name="teamController">队伍控制器，提供队伍状态与角色运行时属性</param>
        public PlayerHUD_ViewModel(TeamController teamController)
        {
            _teamController = teamController ?? throw new ArgumentNullException(nameof(teamController));
            _stats = new CharacterStats[TeamInfoSO.CharacterCount];
            for (int i = 0; i < TeamInfoSO.CharacterCount; i++)
                _stats[i] = _teamController.GetCharacterStats(i);
        }

        /// <summary>
        /// 订阅数据源事件并完成首次投影计算。
        /// </summary>
        public void Start()
        {
            if (_subscribed) return;
            for (int i = 0; i < _stats.Length; i++)
                _stats[i].HPChanged += OnSourceChanged;
            GameEvent.CharacterSwitched += OnSourceChanged;
            _subscribed = true;
            Recompute();
        }

        /// <summary>
        /// 退订全部数据源事件，供 Controller 生命周期调用。
        /// </summary>
        public void Stop()
        {
            if (!_subscribed) return;
            for (int i = 0; i < _stats.Length; i++)
                _stats[i].HPChanged -= OnSourceChanged;
            GameEvent.CharacterSwitched -= OnSourceChanged;
            _subscribed = false;
        }

        private void OnSourceChanged() => Recompute();

        private void OnSourceChanged(int _) => Recompute();

        /// <summary>
        /// 按激活角色槽位轮转，重算全部视图投影并广播。
        /// </summary>
        private void Recompute()
        {
            int activeIndex = _teamController.RuntimeTeamInfo.ActiveCharacterIndex;
            CharacterInfoSO[] characters = _teamController.RuntimeTeamInfo.Characters;

            for (int slot = 0; slot < TeamInfoSO.CharacterCount; slot++)
            {
                int dataIndex = (activeIndex + slot) % TeamInfoSO.CharacterCount;
                CharacterStats stats = _stats[dataIndex];
                float fill = stats.MaxHP > 0 ? (float)stats.CurrentHP / stats.MaxHP : 0f;

                switch (slot)
                {
                    case 0: Avatar0 = characters[dataIndex].Avatar; HpFill0 = fill; break;
                    case 1: Avatar1 = characters[dataIndex].Avatar; HpFill1 = fill; break;
                    case 2: Avatar2 = characters[dataIndex].Avatar; HpFill2 = fill; break;
                }
            }

            int slot0Data = activeIndex % TeamInfoSO.CharacterCount;
            if (slot0Data != _slot0DataIndex)
            {
                _redSetpoint0 = HpFill0;
                _slot0DataIndex = slot0Data;
            }
            else if (HpFill0 >= _redSetpoint0)
            {
                _redSetpoint0 = HpFill0;
            }

            Updated?.Invoke();
        }
    }
}