using UnityEngine;

using SPFramework.Service;

namespace SPCharacter.Contract
{
    /// <summary>
    /// 角色切换服务 - 实例级契约 由每个可切换角色实现并自注册
    /// </summary>
    public interface ICharacterSwitchService : IInstanceService
    {
        /// <summary>
        /// 请求角色播放退场动画
        /// </summary>
        void BeginSwitchOut();

        /// <summary>
        /// 请求角色播放上场动画并落位
        /// </summary>
        /// <param name="pose">上场位置与旋转</param>
        void BeginSwitchIn(Pose pose);

        /// <summary>
        /// 设置角色玩家操作锁
        /// </summary>
        /// <param name="isLocked">是否锁定玩家操作</param>
        void SetOperationLocked(bool isLocked);
    }
}
