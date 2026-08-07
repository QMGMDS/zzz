// C#9 init-only 属性所需的支持类型 shim。
// Unity .NET Standard 2.1 程序集不含本类型，需自行定义以启用 init 访问器。
// 放在 _Scripts 根，随业务脚本一起编译；纯编译期标记，无运行时逻辑。
namespace System.Runtime.CompilerServices
{
    /// <summary>C#9 init 访问器所需编译期标记类型 - Unity 程序集缺省提供，在此自定义以启用 init 语义。</summary>
    internal static class IsExternalInit { }
}