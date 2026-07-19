using UnityEngine;

namespace SPCharacterController
{
    /// <summary>
    /// 角色运行时黑板 - 子系统间数据交流的中枢，按数据写入者隔离修改入口。
    /// </summary>
    public class CharacterRunTimeData
    {
        private CharacterIntention _intentions;

        private void SetIntention(CharacterIntention intention, bool value)
        {
            _intentions = value ? _intentions | intention : _intentions & ~intention;
        }

        /// <summary>
        /// 评估角色是否满足全部指定意图。
        /// </summary>
        /// <param name="condition">需要同时满足的意图条件</param>
        /// <returns>满足全部指定意图时返回 true</returns>
        public bool EvaluateCondition(CharacterIntention condition) => (_intentions & condition) == condition;

        #region InputSource 写入

        /// <summary>死区处理后的二维移动输入</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>移动输入强度，范围为 0 到 1</summary>
        public float MoveInputMagnitude { get; private set; }

        /// <summary>写入本帧移动输入。</summary>
        internal void WriteInput(Vector2 moveInput, float moveInputMagnitude)
        {
            MoveInput = moveInput;
            MoveInputMagnitude = moveInputMagnitude;
        }

        /// <summary>设置或清除输入源产生的意图。</summary>
        internal void SetInputIntention(CharacterIntention intention, bool value) => SetIntention(intention, value);

        #endregion

        #region StateMachine 写入

        /// <summary>当前状态节点</summary>
        public StateNodeSO CurrentStateNode { get; private set; }

        /// <summary>当前状态版本 - 每次切换到不同节点时递增，供动画层判断状态是否发生变动</summary>
        public uint StateVersion { get; private set; }

        /// <summary>发布当前状态并递增状态版本。</summary>
        internal void PublishState(StateNodeSO stateNode)
        {
            CurrentStateNode = stateNode;
            StateVersion++;
        }

        #endregion

        #region AnimationDriver 写入

        /// <summary>当前动画播放时刻（秒）</summary>
        public float AnimationTime { get; private set; }

        /// <summary>当前动画归一化播放进度</summary>
        public float AnimationNormalizedTime { get; private set; }

        /// <summary>重置当前动画进度。</summary>
        internal void ResetAnimationProgress()
        {
            AnimationTime = 0f;
            AnimationNormalizedTime = 0f;
        }

        /// <summary>写入当前动画进度。</summary>
        internal void PublishAnimationProgress(float time, float normalizedTime)
        {
            AnimationTime = time;
            AnimationNormalizedTime = normalizedTime;
        }

        /// <summary>报告当前非循环动画已经完成。</summary>
        internal void ReportAnimationCompleted() => SetIntention(CharacterIntention.AnimationCompleted, true);

        #endregion

        #region MotionDriver 写入

        /// <summary>角色是否着地</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>写入角色当前是否着地。</summary>
        internal void PublishGrounded(bool isGrounded) => IsGrounded = isGrounded;

        #endregion

        #region Root 写入

        /// <summary>清空本帧所有角色意图。</summary>
        internal void ResetIntentions() => _intentions = CharacterIntention.None;

        #endregion

    }
}
