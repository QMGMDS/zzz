using BehaviorDesigner.Runtime.Tasks;
using SPTeam;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace SPCharacterController
{
    /// <summary>
    /// 敌人行为树任务公共上下文 - 提供当前激活玩家 Transform 的访问入口。
    /// </summary>
    internal static class SPAITaskContext
    {
        /// <summary>
        /// 查找场景队伍控制器，任务初始化时调用一次并缓存。
        /// </summary>
        /// <returns>场景中的 TeamController 实例</returns>
        public static TeamController FindTeamController()
        {
            var teamController = UnityEngine.Object.FindAnyObjectByType<TeamController>();
            if (teamController == null)
                throw new System.InvalidOperationException("AI 行为树任务初始化失败：场景中没有 TeamController。");
            return teamController;
        }

        /// <summary>
        /// 获取当前激活玩家的 Transform，每帧动态取值以跟随队伍角色切换。
        /// </summary>
        /// <param name="teamController">场景队伍控制器</param>
        /// <returns>当前激活玩家的 Transform</returns>
        public static Transform GetPlayerTransform(TeamController teamController)
        {
            return teamController.GetCharacterTransform(teamController.RuntimeTeamInfo.ActiveCharacterIndex);
        }
    }

    /// <summary>
    /// 敌人行为树动作节点基类 - 缓存队伍控制器并提供玩家定位。
    /// </summary>
    public abstract class SPAIAction : Action
    {
        private TeamController _teamController;

        /// <summary>当前激活玩家的 Transform</summary>
        protected Transform PlayerTransform => SPAITaskContext.GetPlayerTransform(_teamController);

        /// <inheritdoc />
        public override void OnAwake()
        {
            _teamController = SPAITaskContext.FindTeamController();
        }
    }

    /// <summary>
    /// 敌人行为树条件节点基类 - 缓存队伍控制器并提供玩家定位。
    /// </summary>
    public abstract class SPAIConditional : Conditional
    {
        private TeamController _teamController;

        /// <summary>当前激活玩家的 Transform</summary>
        protected Transform PlayerTransform => SPAITaskContext.GetPlayerTransform(_teamController);

        /// <inheritdoc />
        public override void OnAwake()
        {
            _teamController = SPAITaskContext.FindTeamController();
        }
    }
}
