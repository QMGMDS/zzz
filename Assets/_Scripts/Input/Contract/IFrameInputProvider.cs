namespace SPInput.Contract
{
    /// <summary>
    /// 外部读取当前帧输入的唯一途径。
    /// 输入模块仅通过此接口对外供给帧数据，不做推送、不分发。
    /// 同时供给原始帧数据与后处理特供数据，下游按需取用。
    /// </summary>
    public interface IFrameInputProvider
    {
        /// <summary>当前帧的原始输入 - 纯硬件事实，无手感处理。</summary>
        FrameRawInput CurrentFrame { get; }

        /// <summary>当前帧的后处理输入 - 含死区/防抖/归一化等手感处理结果。</summary>
        ProcessedFrameInput CurrentProcessed { get; }
    }
}
