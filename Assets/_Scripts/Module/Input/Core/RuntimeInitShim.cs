namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 编译期兼容补丁 - Unity 未内置 IsExternalInit 时补充该类型，支撑 init 访问器语法
    /// </summary>
    internal static class IsExternalInit { }
}