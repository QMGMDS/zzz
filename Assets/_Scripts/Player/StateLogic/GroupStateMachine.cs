using System;
using UnityEngine;

namespace SPPlayer
{
    /// <summary>
    /// 族长状态机，统一管理跨族拦截和族内状态转移。
    /// 跨族切换由 NodeInterceptor[] 负责（精确节点匹配），族内切换由 StateGroupSO.Rules 负责。
    /// </summary>
    public class GroupStateMachine
    {
        private readonly PlayerController _player;
        private readonly NodeInterceptor[] _interceptors;
        private readonly PlayerBrain _brain;

        private StateGroupSO _currentGroup;
        private int _currentNodeIndex;
        private IStateBehaviour _activeBehaviour;

        /// <summary>当前激活的状态族</summary>
        public StateGroupSO CurrentGroup => _currentGroup;

        /// <summary>当前激活的数据节点</summary>
        public StateNodeSO CurrentNode => _currentGroup != null && _currentNodeIndex >= 0 && _currentNodeIndex < _currentGroup.Nodes.Length
            ? _currentGroup.Nodes[_currentNodeIndex]
            : null;

        /// <summary>
        /// 创建族长状态机
        /// </summary>
        /// <param name="player">角色控制器引用</param>
        /// <param name="interceptors">拦截器数组（按 Priority 升序排列，越小越先检测）</param>
        public GroupStateMachine(PlayerController player, NodeInterceptor[] interceptors)
        {
            _player = player;
            _brain = player.PlayerBrainBlackboard;

            if (interceptors != null)
            {
                _interceptors = new NodeInterceptor[interceptors.Length];
                Array.Copy(interceptors, _interceptors, interceptors.Length);
                Array.Sort(_interceptors, (a, b) => a.Priority.CompareTo(b.Priority));
            }
            else
            {
                _interceptors = new NodeInterceptor[0];
            }
        }

        /// <summary>
        /// 进入一个状态族
        /// </summary>
        /// <param name="group">目标状态族</param>
        /// <param name="entryIndex">入口节点索引</param>
        public void EnterGroup(StateGroupSO group, int entryIndex)
        {
            ExitNode();

            if (group == null) return;

            _currentGroup = group;
            _currentNodeIndex = Mathf.Clamp(entryIndex, 0, group.Nodes.Length - 1);
            ApplyNode();
        }

        /// <summary>
        /// 将当前节点切换到同族内指定索引的节点
        /// </summary>
        /// <param name="nodeIndex">目标节点索引</param>
        public void TransitionToNode(int nodeIndex)
        {
            if (_currentGroup == null) return;
            if (nodeIndex < 0 || nodeIndex >= _currentGroup.Nodes.Length) return;

            ExitNode();
            _currentNodeIndex = nodeIndex;
            ApplyNode();
        }

        /// <summary>
        /// 每帧 LogicUpdate：
        /// 1) 检查跨族节点拦截器
        /// 2) 若节点有行为插件，交给插件处理
        /// 3) 检查族内转移规则（输入型条件会附加打断窗口检查）
        /// </summary>
        public void LogicUpdate()
        {
            if (_currentGroup == null) return;

            if (TryNodeIntercept()) return; // 跨族状态切换 -> 拦截器负责

            if (_activeBehaviour != null && _activeBehaviour.OnUpdate(_player))
                return;

            TryTransitionRule(); // 同族状态切换 -> StateGroupSO 负责
        }

        #region Internal

        private bool TryNodeIntercept()
        {
            var currentNode = CurrentNode;
            if (currentNode == null || _interceptors.Length == 0) return false;

            foreach (var interceptor in _interceptors)
            {
                if (interceptor.FromNodes == null)
                    continue;

                if (!ContainsNode(interceptor.FromNodes, currentNode))
                    continue;

                if (!EvaluateCondition(interceptor.Condition))
                    continue;

                EnterGroup(interceptor.TargetGroup, interceptor.TargetNodeIndex);
                return true;
            }

            return false;
        }

        private static bool ContainsNode(StateNodeSO[] nodes, StateNodeSO target)
        {
            foreach (var node in nodes)
                if (node == target) return true;
            return false;
        }

        private bool TryTransitionRule()
        {
            if (_currentGroup.Rules == null) return false;

            foreach (var rule in _currentGroup.Rules)
            {
                if (rule.FromIndex != _currentNodeIndex) continue;
                if (rule.Condition == TransitionCondition.Custom) continue;
                if (!EvaluateCondition(rule.Condition)) continue;

                if (IsInputDrivenCondition(rule.Condition))
                {
                    var node = CurrentNode;
                    if (!node.HasCancelWindow) continue;
                    if (_brain.CurrentNormalizedTime < node.CancelWindowStart ||
                        _brain.CurrentNormalizedTime > node.CancelWindowEnd)
                        continue;
                }

                TransitionToNode(rule.ToIndex);
                return true;
            }

            return false;
        }

        // 这些切换条件下才会进行打断窗口判定
        private static bool IsInputDrivenCondition(TransitionCondition condition)
        {
            switch (condition)
            {
                case TransitionCondition.WantToAttack:
                case TransitionCondition.WantToEvade:
                case TransitionCondition.WantToMove:
                case TransitionCondition.NotWantToMove:
                case TransitionCondition.MoveDirectionFlipped:
                    return true;
                default:
                    return false;
            }
        }

        private bool EvaluateCondition(TransitionCondition condition)
        {
            switch (condition)
            {
                case TransitionCondition.Immediate:
                    return true;

                case TransitionCondition.AnimationCompleted:
                    return _brain.AnimationCompleted;

                case TransitionCondition.AnimationCompleted_And_WantToMove:
                    return _brain.AnimationCompleted && _brain.WantToMove;

                case TransitionCondition.AnimationCompleted_And_NotWantToMove:
                    return _brain.AnimationCompleted && !_brain.WantToMove;

                case TransitionCondition.WantToAttack:
                    return _brain.WantToAttack;

                case TransitionCondition.WantToEvade:
                    return _brain.WantToEvade;

                case TransitionCondition.WantToMove:
                    return _brain.WantToMove;

                case TransitionCondition.NotWantToMove:
                    return !_brain.WantToMove;

                case TransitionCondition.MoveDirectionFlipped:
                    {
                        var dir = _brain.CurrentMoveDirection;
                        if (dir.sqrMagnitude <= 0.0001f) return false;
                        return Vector3.Dot(dir.normalized, _player.transform.forward) <= -0.75f;
                    }

                case TransitionCondition.Custom:
                    return false;

                default:
                    return false;
            }
        }

        private void ApplyNode()
        {
            var node = CurrentNode;
            if (node == null) return;

            _brain.CurrentStateNode = node;

            if (node.Behaviour != null)
                _activeBehaviour = node.Behaviour.CreateRuntime();

            _activeBehaviour?.OnEnter(_player);
        }

        private void ExitNode()
        {
            _activeBehaviour?.OnExit(_player);
            _activeBehaviour = null;
        }

        #endregion
    }
}
