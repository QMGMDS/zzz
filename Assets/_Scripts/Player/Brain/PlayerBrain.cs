using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 玩家角色大脑黑板——统一存放运行时的数据中枢
    /// 玩家角色各个模块统一通过该数据中枢进行交流
    /// </summary>
    public class PlayerBrain
    {
        #region 摄像机板块 (PlayerController 负责写入)

        /// <summary>玩家摄像机 Transform 引用</summary>
        public Transform CameraTransform { get; set; }

        #endregion

        #region 输入板块 (输入处理器负责写入)

        /// <summary>攻击意图标记</summary>
        public bool WantToAttack { get; set; }

        /// <summary>攻击按键本帧是否持续按住（每帧写入，不跨帧残留）</summary>
        public bool AttackHeld { get; set; }

        /// <summary>闪避意图标记</summary>
        public bool WantToEvade { get; set; }

        /// <summary>移动意图标记</summary>
        public bool WantToMove { get; set; }

        /// <summary>移动输入轴</summary>
        public Vector2 MoveInput { get; set; }

        /// <summary>本帧移动方向 (默认联系摄像机)</summary>
        public Vector3 CurrentMoveDirection { get; set; }

        /// <summary>上一帧移动方向 (默认联系摄像机)</summary>
        public Vector3 LastMoveDirection { get; set; }

        /// <summary>
        /// 每帧 LateUpdate 末尾调用——清除输入意图脑区的所有标记，防止跨帧残留。
        /// </summary>
        public void ResetInputBrain()
        {
            WantToAttack = false;
            WantToEvade = false;
            WantToMove = false;
            AttackHeld = false;
            MoveInput = Vector2.zero;
        }

        #endregion

        #region 逻辑状态板块 (状态逻辑层负责写入)

        /// <summary>当前逻辑状态类型</summary>
        public PlayerStateType CurrentPlayerState { get; set; }

        #endregion

        #region 动画信息板块 (动画表现层负责写入)

        /// <summary>当前动画归一化时间 0~1</summary>
        public float CurrentNormalizedTime { get; set; }

        /// <summary>当前状态动画是否播放完毕</summary>
        public bool AnimationCompleted { get; set; }

        #endregion
    }
}
