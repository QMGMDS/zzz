namespace Core.Input.Config
{
    /// <summary>
    /// 输入后处理参数配置——InputCollector 的唯一配置来源。
    /// </summary>
    public static class InputPostProcessConfig
    {
        /// <summary>移动轴防抖缓存时间（秒）</summary>
        public const float InputFlickerBuffer = 0.05f;

        /// <summary>攻击按键缓存时间（秒）</summary>
        public const float AttackBufferTime = 0.2f;

        /// <summary>闪避按键缓存时间（秒）</summary>
        public const float EvadeBufferTime = 0.2f;
    }
}
