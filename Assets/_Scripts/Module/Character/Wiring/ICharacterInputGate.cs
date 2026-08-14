namespace SPCharacter.Wiring
{
    /// <summary>
    /// 角色输入门 - 控制玩家操作是否对角色生效
    /// </summary>
    internal interface ICharacterInputGate
    {
        /// <summary>
        /// 设置玩家操作锁
        /// </summary>
        /// <param name="isLocked">是否锁定玩家操作</param>
        void SetOperationLocked(bool isLocked);
    }
}
