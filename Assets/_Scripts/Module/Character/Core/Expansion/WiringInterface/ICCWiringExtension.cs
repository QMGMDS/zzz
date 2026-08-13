namespace SPCharacter.Core
{
    /// <summary>
    /// 角色控制器胶水扩展 - 读取运行时上下文并提交外部控制意图
    /// </summary>
    internal interface ICCWiringExtension
    {
        /// <summary>更新胶水扩展并提交当前帧角色意图</summary>
        void UpdateWiring(CCWiringContext context, IWriteIntention writer);
    }
}
