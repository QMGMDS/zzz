using UnityEngine;

namespace GamePlay.Player
{
    /// <summary>
    /// 玩家意图黑板，担当输入管线与状态机之间的数据桥梁。
    /// 输入层写入意图标记，状态层只读并消费，未被消费的标记跨帧保留以实现输入缓冲。
    /// </summary>
    public class PlayerBlackboard
    {
        /// <summary>当前帧移动方向（已归一化），由输入层每帧更新</summary>
        public Vector2 MoveDirection { get; set; }

        /// <summary>攻击输入缓冲标记，按下后为 true，被状态消费后复位</summary>
        public bool IsAttackBuffered { get; private set; }

        /// <summary>闪避输入缓冲标记，按下后为 true，被状态消费后复位</summary>
        public bool IsEvadeBuffered { get; private set; }

        /// <summary>当前锁定的敌人 Transform，未锁定时为 null</summary>
        public Transform LockTarget { get; set; }

        /// <summary>输入缓冲时间（秒），在该时间内持续无输入才判定为停止</summary>
        public float InputBufferTime { get; private set; }

        /// <summary>连击窗口持续时间（秒），攻击动画结束后在该时间内再次按下攻击键可进入下一段连击</summary>
        public float ComboWindowDuration { get; private set; }

        /// <summary>初始化只读配置参数</summary>
        /// <param name="inputBufferTime">输入缓冲时间（秒）</param>
        /// <param name="comboWindowDuration">连击窗口时长（秒）</param>
        public void Initialize(float inputBufferTime, float comboWindowDuration)
        {
            InputBufferTime = inputBufferTime;
            ComboWindowDuration = comboWindowDuration;
        }

        /// <summary>由输入层调用，标记攻击输入已按下</summary>
        public void SetAttackPressed()
        {
            IsAttackBuffered = true;
        }

        /// <summary>由输入层调用，标记闪避输入已按下</summary>
        public void SetEvadePressed()
        {
            IsEvadeBuffered = true;
        }

        /// <summary>由状态层调用，消费攻击输入缓冲</summary>
        public void ConsumeAttack()
        {
            IsAttackBuffered = false;
        }

        /// <summary>由状态层调用，消费闪避输入缓冲</summary>
        public void ConsumeEvade()
        {
            IsEvadeBuffered = false;
        }
    }
}
