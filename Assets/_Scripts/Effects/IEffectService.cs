namespace SPEffects
{
    /// <summary>
    /// 特效服务接口 - 负责按请求播放与清理特效实例。
    /// </summary>
    public interface IEffectService
    {
        /// <summary>
        /// 播放特效请求。
        /// </summary>
        /// <param name="request">特效播放请求</param>
        /// <returns>成功时返回实例句柄，失败时返回 null</returns>
        IEffectInstance Play(in EffectPlayRequest request);

        /// <summary>
        /// 清理服务创建的全部实例。
        /// </summary>
        void CleanupAll();
    }
}
