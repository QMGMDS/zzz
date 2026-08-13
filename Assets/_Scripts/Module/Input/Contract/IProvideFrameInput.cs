using SPFramework.Service;

namespace SPInput.Contract
{
    /// <summary>
    /// 提供行为 - 提供输入模块产出的帧输入
    /// 外部读取当前帧玩家输入的唯一途径
    /// </summary>
    public interface IProvideFrameInput : IModuleService
    {
        /// <summary>当前帧的原始输入 - 纯硬件事实，无手感处理</summary>
        RawFrameInput CurrentFrame { get; }

        /// <summary>当前帧的后处理输入 - 含手感处理结果，角色模块特供输入</summary>
        ProcessedFrameInput CurrentProcessed { get; }
    }
}
