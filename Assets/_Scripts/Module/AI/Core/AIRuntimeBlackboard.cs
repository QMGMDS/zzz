using UnityEngine;

namespace SPAI.Core
{
    /// <summary>
    /// AI 运行时黑板 - 集中维护感知 决策与巡逻状态
    /// </summary>
    internal sealed class AIRuntimeBlackboard
    {
        /// <summary>巡逻锚点 出生位置</summary>
        public Vector3 AnchorPosition { get; private set; }

        /// <summary>当前是否持有有效目标</summary>
        public bool HasTarget { get; private set; }

        /// <summary>目标是否处于当前视野内 丢失视野期间为 false</summary>
        public bool IsTargetVisible { get; private set; }

        /// <summary>是否已脱战且尚未回归巡逻范围</summary>
        public bool IsReturning { get; private set; }

        /// <summary>当前目标位置 丢失视野期间为最后一次目击位置 无目标时为零向量</summary>
        public Vector3 TargetPosition { get; private set; }

        /// <summary>当前巡逻目标点</summary>
        public Vector3 PatrolPoint { get; private set; }

        /// <summary>是否已生成过巡逻点</summary>
        public bool HasLastPatrolPoint { get; private set; }

        /// <summary>上一次生成的巡逻点</summary>
        public Vector3 LastPatrolPoint { get; private set; }

        /// <summary>
        /// 初始化运行时状态
        /// </summary>
        /// <param name="anchorPosition">巡逻锚点</param>
        public void Initialize(Vector3 anchorPosition)
        {
            AnchorPosition = anchorPosition;
            PatrolPoint = anchorPosition;
            LastPatrolPoint = Vector3.zero;
            HasLastPatrolPoint = false;
            TargetPosition = Vector3.zero;
            HasTarget = false;
            IsTargetVisible = false;
            IsReturning = false;
        }

        /// <summary>
        /// 写入当前可见目标
        /// </summary>
        /// <param name="targetPosition">目标当前位置</param>
        public void SetVisibleTarget(Vector3 targetPosition)
        {
            HasTarget = true;
            IsTargetVisible = true;
            TargetPosition = targetPosition;
        }

        /// <summary>
        /// 标记当前目标不可见并保留最后目击位置
        /// </summary>
        public void MarkTargetNotVisible()
        {
            IsTargetVisible = false;
        }

        /// <summary>
        /// 清除当前目标并进入脱战回归状态
        /// </summary>
        public void ClearTargetAndBeginReturning()
        {
            HasTarget = false;
            IsTargetVisible = false;
            TargetPosition = Vector3.zero;
            IsReturning = true;
        }

        /// <summary>
        /// 完成脱战回归并恢复常规巡逻
        /// </summary>
        public void CompleteReturning()
        {
            IsReturning = false;
        }

        /// <summary>
        /// 写入新的巡逻点并记录为上一巡逻点
        /// </summary>
        /// <param name="patrolPoint">新的巡逻点</param>
        public void SetPatrolPoint(Vector3 patrolPoint)
        {
            PatrolPoint = patrolPoint;
            LastPatrolPoint = patrolPoint;
            HasLastPatrolPoint = true;
        }
    }
}
