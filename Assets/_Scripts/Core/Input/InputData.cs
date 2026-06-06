namespace Core.Input
{
    /// <summary>
    /// 堆内存输入数据容器——由 InputCollector 写入，外部系统通过只读引用读取。
    /// 持有当前帧与上一帧的快照，支持帧级差分分析。
    /// </summary>
    public class InputData
    {
        /// <summary>当前帧输入快照</summary>
        public FrameInputData CurrentFrameData;

        /// <summary>上一帧输入快照</summary>
        public FrameInputData LastFrameData;
    }
}
