using UnityEngine;

namespace SPCharacter.Core
{
    /// <summary>
    /// 运行时黑板，角色控制器内部系统的数据交流中枢
    /// 角色控制器内部持有，绝不向外部模块提供修改方法
    /// </summary>
    internal sealed class CCRunTimeBlackboard
    {
        private CCIntention _intentions;

        private void SetIntention(CCIntention intention, bool value)
        {
            _intentions = value ? _intentions | intention : _intentions & ~intention;
        }

        /// <summary>
        /// 评估角色当前意图是否满足目标转移条件
        /// </summary>
        /// <param name="condition">目标转移条件</param>
        public bool EvaluateCondition(StateTransitionCondition condition)
            => (_intentions & condition.Required) == condition.Required
               && (_intentions & condition.Forbidden) == CCIntention.None;

        #region 意图写入

        /// <summary>角色目标方向，XY 分量分别对应世界 XZ 轴</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>写入本帧移动意图</summary>
        public void WriteInput(Vector2 moveInput)
        {
            MoveInput = moveInput;
        }

        /// <summary>设置或清除本帧控制意图</summary>
        public void SetControlIntention(CCIntention intention, bool value) => SetIntention(intention, value);

        #endregion

        #region StateMachine 写入

        /// <summary>当前状态节点 Id（轻量标识，不持有 ScriptableObject 资源句柄）</summary>
        public string CurrentStateId { get; private set; }

        /// <summary>当前状态版本 - 每次切换到不同节点时递增，供动画层判断状态是否发生变动</summary>
        public uint StateVersion { get; private set; }

        /// <summary>
        /// 发布当前状态 Id 并递增状态版本
        /// </summary>
        public void PublishState(string stateId)
        {
            CurrentStateId = stateId;
            StateVersion++;
        }

        #endregion

        #region AnimationDriver 写入

        /// <summary>当前动画播放时刻（秒）</summary>
        public float AnimationTime { get; private set; }

        /// <summary>当前动画归一化播放进度</summary>
        public float AnimationNormalizedTime { get; private set; }

        /// <summary>当前状态动画开始播放时的归一化进度，供运动层建立首帧采样基线</summary>
        public float AnimationEntryNormalizedTime { get; private set; }

        /// <summary>发布新状态动画的实际播放入口</summary>
        public void BeginAnimationProgress(float entryTime, float entryNormalizedTime)
        {
            AnimationTime = entryTime;
            AnimationNormalizedTime = entryNormalizedTime;
            AnimationEntryNormalizedTime = entryNormalizedTime;
        }

        /// <summary>写入当前动画进度</summary>
        public void PublishAnimationProgress(float time, float normalizedTime)
        {
            AnimationTime = time;
            AnimationNormalizedTime = normalizedTime;
        }

        /// <summary>报告当前非循环动画已经完成</summary>
        public void ReportAnimationCompleted() => SetIntention(CCIntention.AnimationCompleted, true);

        #endregion

        #region Root 写入

        /// <summary>清空本帧所有角色意图</summary>
        public void ResetIntentions() => _intentions = CCIntention.None;

        #endregion
    }
}
