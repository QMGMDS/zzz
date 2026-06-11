namespace SPPlayer
{
    /// <summary>
    /// 玩家角色大脑黑板——统一存放运行时的数据中枢
    /// 玩家角色各个模块 (玩家输入/逻辑状态机/动画表示层) 统一通过该数据中枢进行交流
    /// </summary>
    public class PlayerBrain
    {
        #region 输入意图 脑区

        /// <summary>攻击意图标记</summary>
        public bool WantToAttack { get; set; }

        /// <summary>闪避意图标记</summary>
        public bool WantToEvade { get; set; }

        #endregion

        #region 逻辑-动画 脑区

        /// <summary>当前逻辑状态类型——状态逻辑层在状态切换时写入，动画表现层据此自行选择并播放动画</summary>
        public PlayerStateType CurrentPlayerState { get; set; }

        /// <summary>当前动画归一化时间 0~1——动画层每帧回写</summary>
        public float CurrentNormalizedTime { get; set; }

        /// <summary>当前状态动画已播放完毕——动画层写入，状态逻辑读取以决定退出时机</summary>
        public bool AnimationCompleted { get; set; }

        #endregion

        /// <summary>
        /// 每帧 LateUpdate 末尾调用——清除输入意图脑区的所有标记，防止跨帧残留。
        /// </summary>
        public void ResetInputBrain()
        {
            WantToAttack = false;
            WantToEvade = false;
        }
    }
}
