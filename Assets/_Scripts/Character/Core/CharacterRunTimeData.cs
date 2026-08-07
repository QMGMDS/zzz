using UnityEngine;
using SPCharacter.Contract;

namespace SPCharacter.Core
{
    /// <summary>
    /// 角色运行时黑板，隔离意图、状态、动画驱动之间的数据写入入口。
    /// 角色内部持有，绝不提供向外部系统提供修改方法。
    /// </summary>
    public class CharacterRunTimeData
    {
        private CharacterIntention _intentions;

        private void SetIntention(CharacterIntention intention, bool value)
        {
            _intentions = value ? _intentions | intention : _intentions & ~intention;
        }

        /// <summary>
        /// 评估角色当前意图是否满足转移条件。
        /// Required 中的位必须全部为 1，Forbidden 中的位必须全部为 0，
        /// 未出现在任一组中的位视为自由，不影响判定。
        /// </summary>
        /// <param name="condition">转移条件（必须为 1 与必须为 0 的两组意图位）</param>
        /// <returns>同时满足 Required 与 Forbidden 约束时返回 true</returns>
        public bool EvaluateCondition(StateTransitionCondition condition)
            => (_intentions & condition.Required) == condition.Required
               && (_intentions & condition.Forbidden) == CharacterIntention.None;

        #region 意图写入

        /// <summary>角色目标方向，XY 分量分别对应世界 XZ 轴。</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>写入本帧移动意图。</summary>
        internal void WriteInput(Vector2 moveInput)
        {
            MoveInput = moveInput;
        }

        /// <summary>设置或清除本帧控制意图。</summary>
        internal void SetControlIntention(CharacterIntention intention, bool value) => SetIntention(intention, value);

        #endregion

        #region StateMachine 写入

        /// <summary>当前状态节点 Id（轻量标识，不持有 ScriptableObject 资源句柄）</summary>
        public string CurrentStateId { get; private set; }

        /// <summary>当前状态版本 - 每次切换到不同节点时递增，供动画层判断状态是否发生变动</summary>
        public uint StateVersion { get; private set; }

        /// <summary>
        /// 发布当前状态 Id 并递增状态版本。
        /// </summary>
        /// <param name="stateId">状态节点唯一标识</param>
        /// <exception cref="System.ArgumentException">stateId 为 null 或空字符串</exception>
        internal void PublishState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId))
                throw new System.ArgumentException("状态节点 Id 不能为 null 或空字符串。", nameof(stateId));

            CurrentStateId = stateId;
            StateVersion++;
        }

        /// <summary>等待运动驱动器消费的状态结束相对 Y 轴旋转补偿，单位为度。</summary>
        public float PendingCompletionRotationDegrees { get; private set; }

        /// <summary>写入状态结束时的一次性相对 Y 轴旋转补偿。</summary>
        internal void PublishCompletionRotation(float rotationDegrees)
        {
            PendingCompletionRotationDegrees = rotationDegrees;
        }

        #endregion

        #region AnimationDriver 写入

        /// <summary>当前动画播放时刻（秒）</summary>
        public float AnimationTime { get; private set; }

        /// <summary>当前动画归一化播放进度</summary>
        public float AnimationNormalizedTime { get; private set; }

        /// <summary>当前状态动画开始播放时的归一化进度，供运动层建立首帧采样基线。</summary>
        public float AnimationEntryNormalizedTime { get; private set; }

        /// <summary>发布新状态动画的实际播放入口。</summary>
        internal void BeginAnimationProgress(float entryTime, float entryNormalizedTime)
        {
            AnimationTime = entryTime;
            AnimationNormalizedTime = entryNormalizedTime;
            AnimationEntryNormalizedTime = entryNormalizedTime;
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

        /// <summary>清空已经应用的状态结束旋转补偿。</summary>
        internal void ClearCompletionRotation()
        {
            PendingCompletionRotationDegrees = 0f;
        }

        #endregion

        #region Root 写入

        /// <summary>清空本帧所有角色意图。</summary>
        internal void ResetIntentions() => _intentions = CharacterIntention.None;

        #endregion
    }
}