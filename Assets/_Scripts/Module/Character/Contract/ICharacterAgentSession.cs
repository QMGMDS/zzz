using UnityEngine;

using SPFramework.Service;

namespace SPCharacter.Contract
{
    /// <summary>
    /// 角色代理会话 - 实例级契约 由每个可被代理驱动的角色实现并按角色 Id 自注册
    /// 供 AI 等代理驱动源控制角色移动 转向与攻击
    /// 所有写入均为每帧语义 - 当帧有效 消费后即清空 需持续调用才能维持效果
    /// 服务未注册时消费方应降级为不驱动角色
    /// </summary>
    public interface ICharacterAgentSession : IInstanceService
    {
        /// <summary>
        /// 写入本帧移动方向 - 非零时提交移动意图并驱动角色朝该方向转向 零向量停止移动
        /// </summary>
        /// <param name="worldDirection">世界 XZ 方向 XY 分量分别对应世界 XZ 轴 须为归一化或零向量</param>
        void SetMoveAxis(Vector2 worldDirection);

        /// <summary>
        /// 写入本帧朝向方向 - 仅驱动角色转向 不提交移动意图 本帧移动方向为零时生效
        /// 当前状态转身速度为零时不转向
        /// </summary>
        /// <param name="worldDirection">世界 XZ 方向 XY 分量分别对应世界 XZ 轴 须为归一化向量</param>
        void SetFacingDirection(Vector2 worldDirection);

        /// <summary>
        /// 请求角色发起一次攻击 - 请求语义 每次调用提交一帧攻击意图
        /// 攻击动画与连击由角色状态机消化 连续调用等价于玩家连续输入
        /// </summary>
        void RequestAttack();
    }
}
