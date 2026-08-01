namespace SPInput_Contract
{
    /// <summary>
    /// 外部读取当前帧原始输入的唯一途径。
    /// 输入模块仅通过此接口对外供给帧数据，不做推送、不分发。
    /// </summary>
    public interface IFrameInputProvider
    {
        /// <summary>当前帧的原始输入。</summary>
        FrameRawInput CurrentFrame { get; }
    }
}
