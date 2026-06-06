namespace Core.Input
{
    /// <summary>
    /// 输入源接口——所有输入来源（玩家/AI/调试）统一通过此接口
    /// 将原始硬件数据填充至 RawInputData 结构体。
    /// </summary>
    public interface IInputSource
    {
        /// <summary>
        /// 从输入源采样原始数据并写入提供的 RawInputData 结构体
        /// </summary>
        /// <param name="rawData">接收原始数据的结构体引用（ref 避免栈复制）</param>
        void FetchRawInput(ref RawInputData rawData);
    }
}
