namespace Core.Input
{
    /// <summary>
    /// 单帧输入快照——持有原始数据与处理后数据的完整副本。
    /// 配合 InputData 中的 LastFrameData 可实现帧级历史比较。
    /// </summary>
    public struct FrameInputData
    {
        /// <summary>物理帧计数器</summary>
        public ulong FrameIndex;

        /// <summary>本帧原始硬件数据</summary>
        public RawInputData Raw;

        /// <summary>本帧处理后数据</summary>
        public ProcessedInputData Processed;
    }
}
