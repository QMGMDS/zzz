using System;
using UnityEngine;

namespace SPUI
{
    /// <summary>
    /// PlayerHUD 视图模型 - Team 模块重写期间输出空 HUD 投影，避免依赖旧队伍控制器。
    /// 新 Team Contract 稳定后再接入真实队伍数据源。
    /// </summary>
    public class PlayerHUD_ViewModel
    {
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

        /// <summary>槽位 0 红底血条的瞬时目标 fillAmount。</summary>
        public float RedSetpoint0 => _redSetpoint0;

        /// <summary>
        /// 启动视图模型并发布一次空投影。
        /// </summary>
        public void Start()
        {
            RecomputeEmptyProjection();
        }

        /// <summary>
        /// 停止视图模型。当前临时实现没有外部订阅需要释放。
        /// </summary>
        public void Stop()
        {
        }

        private void RecomputeEmptyProjection()
        {
            Avatar0 = null;
            Avatar1 = null;
            Avatar2 = null;
            HpFill0 = 0f;
            HpFill1 = 0f;
            HpFill2 = 0f;
            _redSetpoint0 = 0f;
            Updated?.Invoke();
        }
    }
}
