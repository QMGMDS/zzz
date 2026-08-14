using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public sealed class Violation
{
    public string Severity = "ERROR";
    public string Code = "UCG000";
    public string File = "";
    public int Line = 1;
    public string Message = "";
}
internal sealed class TypeContext
{
    public string Kind = "";
    public string Name = "";
    public HashSet<string> Interfaces = new HashSet<string>(StringComparer.Ordinal);
}

internal sealed class ProjectInfo
{
    public Dictionary<string, HashSet<string>> InterfaceMembers = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
    public Dictionary<string, HashSet<string>> InterfaceBases = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
    public Dictionary<string, HashSet<string>> ResolvedInterfaceMembers = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
}
public static class GuardLint
{
    private static readonly string[] ExcludedDirectories = new[]
    {
        "Library", "Temp", "Obj", "obj", "bin", "Build", "Builds", "Logs", ".git", ".vs", ".idea", "node_modules"
    };

    public static int Run(IList<string> args)
    {
        var root = ResolveRoot(args);
        var json = args.Any(a => a == "--json");
        var failOnWarn = args.Any(a => a == "--fail-on-warn");
        var files = ResolveFiles(root, args);
        var projectInfoFiles = ResolveProjectInfoFiles(root, files);
        var projectInfo = BuildProjectInfo(projectInfoFiles);
        var violations = new List<Violation>();

        foreach (var file in files)
        {
            LintFile(root, file, violations, projectInfo);
        }

        var errorCount = violations.Count(v => v.Severity == "ERROR");
        var warnCount = violations.Count(v => v.Severity == "WARN");

        if (json)
        {
            PrintJson(root, files, violations, errorCount, warnCount);
        }
        else
        {
            PrintText(root, files, violations, errorCount, warnCount);
        }

        return errorCount > 0 || (failOnWarn && warnCount > 0) ? 1 : 0;
    }
    private static string ResolveRoot(IList<string> args)
    {
        var positional = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToList();
        var root = positional.Count > 0 ? positional[0] : Directory.GetCurrentDirectory();
        return Path.GetFullPath(root);
    }

    private static List<string> ResolveFiles(string root, IList<string> args)
    {
        var filesIndex = args.IndexOf("--files");
        if (filesIndex >= 0)
        {
            var listed = new List<string>();
            for (var i = filesIndex + 1; i < args.Count; i++)
            {
                var value = args[i];
                if (value.StartsWith("--", StringComparison.Ordinal)) break;
                var fullPath = Path.GetFullPath(Path.IsPathRooted(value) ? value : Path.Combine(root, value));
                if (File.Exists(fullPath) && fullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    listed.Add(fullPath);
                }
            }
            return listed.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToList();
        }

        var scanRoot = ResolveScanRoot(root);
        if (!Directory.Exists(scanRoot)) return new List<string>();

        return Directory.EnumerateFiles(scanRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsExcluded(root, p))
            .OrderBy(p => p)
            .ToList();
    }

    private static List<string> ResolveProjectInfoFiles(string root, List<string> lintFiles)
    {
        var scanRoot = ResolveScanRoot(root);

        if (!Directory.Exists(scanRoot)) return lintFiles;

        var projectFiles = Directory.EnumerateFiles(scanRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsExcluded(root, p))
            .OrderBy(p => p)
            .ToList();

        return projectFiles.Count == 0 ? lintFiles : projectFiles;
    }

    private static string ResolveScanRoot(string root)
    {
        var scriptsRoot = Path.Combine(root, "Assets", "_Scripts");
        if (Directory.Exists(scriptsRoot)) return scriptsRoot;
        return Directory.Exists(Path.Combine(root, "Assets")) ? Path.Combine(root, "Assets") : root;
    }

    private static bool IsExcluded(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path);
        var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => ExcludedDirectories.Contains(p, StringComparer.OrdinalIgnoreCase));
    }

    private static void LintFile(string root, string file, List<Violation> violations, ProjectInfo projectInfo)
    {
        var lines = File.ReadAllLines(file);
        var fileText = string.Join("\n", lines);
        var classNames = new HashSet<string>();
        var typeContexts = BuildTypeContexts(lines);

        for (var i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var line = StripLineCommentForCode(raw);
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
            {
                LintComment(root, file, i, raw, violations);
                continue;
            }

            LintComment(root, file, i, raw, violations);

            var typeMatch = Regex.Match(line, @"^\s*(?:(?:public|private|protected|internal|static|abstract|sealed|partial)\s+)*(class|struct|interface)\s+([A-Za-z_]\w*)\b");
            if (typeMatch.Success)
            {
                var kind = typeMatch.Groups[1].Value;
                var name = typeMatch.Groups[2].Value;
                classNames.Add(name);

                if (!HasSummaryBefore(lines, i))
                {
                    Add(violations, "ERROR", "UCG101", root, file, i + 1, $"{kind} `{name}` 缺少 XML summary");
                }

                if (!IsPascalCase(name))
                {
                    Add(violations, "ERROR", "UCG102", root, file, i + 1, $"{kind} `{name}` 必须使用大驼峰");
                }

                if (kind == "interface" && !Regex.IsMatch(name, @"^I[A-Z]"))
                {
                    Add(violations, "ERROR", "UCG103", root, file, i + 1, $"接口 `{name}` 必须使用 I 前缀");
                }

                if (kind == "class" && Regex.IsMatch(line, @"\babstract\b") && !name.EndsWith("Base", StringComparison.Ordinal))
                {
                    Add(violations, "ERROR", "UCG104", root, file, i + 1, $"抽象类 `{name}` 必须使用 Base 后缀");
                }
            }

            LintMember(root, file, lines, i, line, classNames, typeContexts[i], projectInfo, violations);
        }

        LintEventPairs(root, file, lines, fileText, violations);
    }
    private static void LintComment(string root, string file, int index, string raw, List<Violation> violations)
    {
        var comment = ExtractComment(raw);
        if (comment == null) return;

        if (comment.Contains("——"))
        {
            Add(violations, "ERROR", "UCG201", root, file, index + 1, "注释禁止使用全角破折号 `——`，使用 ` - `");
        }

        if (comment.Contains("。"))
        {
            Add(violations, "ERROR", "UCG202", root, file, index + 1, "注释禁止使用句号 `。`");
        }

        if (Regex.IsMatch(comment, @"\b(TODO|FIXME|HACK|TEMP)\b", RegexOptions.IgnoreCase))
        {
            Add(violations, "ERROR", "UCG203", root, file, index + 1, "禁止交付 TODO/FIXME/HACK/TEMP 注释");
        }
    }

    private static void LintMember(string root, string file, string[] lines, int index, string line, HashSet<string> classNames, TypeContext currentType, ProjectInfo projectInfo, List<Violation> violations)
    {
        var attributeBlock = GetAttributeBlock(lines, index);
        var serialized = attributeBlock.Contains("SerializeField");
        var hasTooltip = attributeBlock.Contains("Tooltip");

        var fieldMatch = Regex.Match(line, @"^\s*(?:\[[^\]]+\]\s*)*(?<mods>(?:(?:public|private|protected|internal|static|readonly|const|new|volatile)\s+)*)(?<type>[A-Za-z_]\w*(?:\s*<[^;=(){}]+>)?(?:\[\])?)\s+(?<name>[A-Za-z_]\w*)\s*(?:=|;|,)");
        if (fieldMatch.Success && !line.Contains("(") && !line.Contains("=>"))
        {
            var mods = " " + fieldMatch.Groups["mods"].Value + " ";
            var type = fieldMatch.Groups["type"].Value.Trim();
            var name = fieldMatch.Groups["name"].Value.Trim();
            var isPrivate = mods.Contains(" private ");
            var isStatic = mods.Contains(" static ");
            var isReadonly = mods.Contains(" readonly ");
            var isConst = mods.Contains(" const ");

            if (serialized)
            {
                if (!isPrivate)
                {
                    Add(violations, "ERROR", "UCG302", root, file, index + 1, $"序列化字段 `{name}` 必须是 private");
                }
                if (!hasTooltip)
                {
                    Add(violations, "ERROR", "UCG303", root, file, index + 1, $"序列化字段 `{name}` 必须带 Tooltip");
                }
                if (!Regex.IsMatch(name, @"^_[a-z][A-Za-z0-9]*$"))
                {
                    Add(violations, "ERROR", "UCG304", root, file, index + 1, $"序列化字段 `{name}` 必须使用 `_` + 小驼峰");
                }
            }

            if (isConst || (isStatic && isReadonly))
            {
                if (!IsPascalCase(name))
                {
                    Add(violations, "ERROR", "UCG305", root, file, index + 1, $"常量或 static readonly `{name}` 必须使用大驼峰");
                }
            }
            else if (isPrivate && isStatic)
            {
                if (!Regex.IsMatch(name, @"^s_[a-z][A-Za-z0-9]*$"))
                {
                    Add(violations, "ERROR", "UCG306", root, file, index + 1, $"私有静态字段 `{name}` 必须使用 `s_` + 小驼峰");
                }
            }
            else if (isPrivate)
            {
                if (!Regex.IsMatch(name, @"^_[a-z][A-Za-z0-9]*$"))
                {
                    Add(violations, "ERROR", "UCG307", root, file, index + 1, $"私有实例字段 `{name}` 必须使用 `_` + 小驼峰");
                }
            }

            if (IsBoolType(type) && !HasBoolPrefix(name))
            {
                Add(violations, "ERROR", "UCG308", root, file, index + 1, $"布尔字段 `{name}` 必须使用 is/has/can/should 语义前缀");
            }
        }

        var methodMatch = Regex.Match(line, @"^\s*(public|protected)\s+(?:(?:static|virtual|override|abstract|async|sealed|extern|new)\s+)*(?<type>[A-Za-z_]\w*(?:\s*<[^;=(){}]+>)?(?:\[\])?)\s+(?<name>[A-Za-z_]\w*)\s*\(");
        if (methodMatch.Success)
        {
            var name = methodMatch.Groups["name"].Value;
            if (!classNames.Contains(name))
            {
                var hasSummary = HasSummaryBefore(lines, index);
                var hasInheritDoc = HasInheritDocBefore(lines, index);
                var canUseInheritDoc = CanUseInheritDoc(line, currentType, projectInfo, name);

                if (canUseInheritDoc)
                {
                    if (!hasSummary && !hasInheritDoc)
                    {
                        Add(violations, "ERROR", "UCG401", root, file, index + 1, $"继承或重写方法 `{name}` 缺少 XML summary 或 inheritdoc");
                    }
                }
                else if (!hasSummary)
                {
                    if (hasInheritDoc)
                    {
                        Add(violations, "ERROR", "UCG401", root, file, index + 1, $"只有继承或重写方法可以使用 inheritdoc：`{name}`");
                    }
                    else
                    {
                        Add(violations, "ERROR", "UCG401", root, file, index + 1, $"公有或受保护方法 `{name}` 缺少 XML summary");
                    }
                }

                if (!IsPascalCase(name))
                {
                    Add(violations, "ERROR", "UCG402", root, file, index + 1, $"方法 `{name}` 必须使用大驼峰");
                }
            }
        }

        var propertyMatch = Regex.Match(line, @"^\s*(public|protected)\s+(?:(?:static|virtual|override|abstract|sealed|new)\s+)*(?<type>[A-Za-z_]\w*(?:\s*<[^;=(){}]+>)?(?:\[\])?)\s+(?<name>[A-Za-z_]\w*)\s*(?:\{|=>)");
        if (propertyMatch.Success && !line.Contains("("))
        {
            var type = propertyMatch.Groups["type"].Value.Trim();
            var name = propertyMatch.Groups["name"].Value.Trim();
            var hasSummary = HasSummaryBefore(lines, index);
            var hasInheritDoc = HasInheritDocBefore(lines, index);
            var canUseInheritDoc = CanUseInheritDoc(line, currentType, projectInfo, name);

            if (canUseInheritDoc)
            {
                if (!hasSummary && !hasInheritDoc)
                {
                    Add(violations, "ERROR", "UCG501", root, file, index + 1, $"继承或重写属性 `{name}` 缺少 XML summary 或 inheritdoc");
                }
            }
            else if (!hasSummary)
            {
                if (hasInheritDoc)
                {
                    Add(violations, "ERROR", "UCG501", root, file, index + 1, $"只有继承或重写属性可以使用 inheritdoc：`{name}`");
                }
                else
                {
                    Add(violations, "ERROR", "UCG501", root, file, index + 1, $"公开或受保护属性 `{name}` 缺少 XML summary");
                }
            }

            if (!IsPascalCase(name))
            {
                Add(violations, "ERROR", "UCG502", root, file, index + 1, $"属性 `{name}` 必须使用大驼峰");
            }
            if (IsBoolType(type) && !HasBoolPrefix(name))
            {
                Add(violations, "ERROR", "UCG503", root, file, index + 1, $"布尔属性 `{name}` 必须使用 Is/Has/Can/Should 语义前缀");
            }
        }
    }
    private static void LintEventPairs(string root, string file, string[] lines, string fileText, List<Violation> violations)
    {
        var added = new Dictionary<string, int>();
        var removed = new HashSet<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = StripLineCommentForCode(lines[i]);
            foreach (Match match in Regex.Matches(line, @"(?<target>[A-Za-z_]\w*(?:\.[A-Za-z_]\w*)+)\s*(?<op>\+=|-=)"))
            {
                var target = match.Groups["target"].Value;
                if (match.Groups["op"].Value == "+=")
                {
                    if (!added.ContainsKey(target)) added[target] = i + 1;
                }
                else
                {
                    removed.Add(target);
                }
            }
        }

        foreach (var pair in added)
        {
            if (!removed.Contains(pair.Key))
            {
                Add(violations, "WARN", "UCG601", root, file, pair.Value, $"事件或委托 `{pair.Key}` 有订阅但未发现对应退订");
            }
        }
    }

    private static string GetAttributeBlock(string[] lines, int index)
    {
        var parts = new List<string>();
        var current = lines[index].Trim();
        if (current.StartsWith("[", StringComparison.Ordinal)) parts.Add(current);
        for (var i = index - 1; i >= 0; i--)
        {
            var text = lines[i].Trim();
            if (text.Length == 0) break;
            var startsWithBracket = text.StartsWith("[", StringComparison.Ordinal);
            var closesBracket = text.EndsWith("]", StringComparison.Ordinal);
            if (!startsWithBracket && !closesBracket) break;
            parts.Add(text);
        }
        return string.Join(" ", parts);
    }


    private static ProjectInfo BuildProjectInfo(IList<string> files)
    {
        var projectInfo = new ProjectInfo();
        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file);
            CollectInterfaceMembers(lines, projectInfo);
        }

        return projectInfo;
    }

    private static void CollectInterfaceMembers(string[] lines, ProjectInfo projectInfo)
    {
        var typeContexts = BuildTypeContexts(lines);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = StripLineCommentForCode(lines[i]);
            var typeMatch = Regex.Match(line, @"^\s*(?:(?:public|private|protected|internal|static|abstract|sealed|partial)\s+)*(class|struct|interface)\s+(?<name>[A-Za-z_]\w*)\s*(?::\s*(?<bases>[^{}]+))?");
            if (typeMatch.Success && typeMatch.Groups[1].Value == "interface")
            {
                AddInterfaceBases(projectInfo, typeMatch.Groups["name"].Value, ExtractInterfaceNames(typeMatch.Groups["bases"].Value));
            }

            var currentType = typeContexts[i];
            if (currentType == null || currentType.Kind != "interface")
                continue;

            var methodMatch = Regex.Match(line, @"^\s*(?:(?:public|abstract|virtual|sealed|static|extern|new)\s+)*(?<type>[A-Za-z_]\w*(?:\s*<[^;=(){}]+>)?(?:\[\])?)\s+(?<name>[A-Za-z_]\w*)\s*\(");
            if (methodMatch.Success)
            {
                AddInterfaceMember(projectInfo, currentType.Name, methodMatch.Groups["name"].Value);
                continue;
            }

            var propertyMatch = Regex.Match(line, @"^\s*(?:(?:public|abstract|virtual|sealed|static|extern|new)\s+)*(?<type>[A-Za-z_]\w*(?:\s*<[^;=(){}]+>)?(?:\[\])?)\s+(?<name>[A-Za-z_]\w*)\s*(?:\{|=>)");
            if (propertyMatch.Success && !line.Contains("("))
            {
                AddInterfaceMember(projectInfo, currentType.Name, propertyMatch.Groups["name"].Value);
            }
        }
    }

    private static TypeContext[] BuildTypeContexts(string[] lines)
    {
        var contexts = new TypeContext[lines.Length];
        var stack = new Stack<(TypeContext Context, int Depth)>();
        TypeContext pending = null;
        var braceDepth = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var code = StripLineCommentForCode(lines[i]);
            contexts[i] = stack.Count > 0 ? stack.Peek().Context : null;

            var typeMatch = Regex.Match(code, @"^\s*(?:(?:public|private|protected|internal|static|abstract|sealed|partial)\s+)*(class|struct|interface)\s+(?<name>[A-Za-z_]\w*)\s*(?::\s*(?<bases>[^{}]+))?");
            if (typeMatch.Success)
            {
                pending = new TypeContext
                {
                    Kind = typeMatch.Groups[1].Value,
                    Name = typeMatch.Groups["name"].Value,
                    Interfaces = ExtractInterfaceNames(typeMatch.Groups["bases"].Value)
                };
            }

            var openCount = CountCharOutsideStrings(code, '{');
            var closeCount = CountCharOutsideStrings(code, '}');

            if (pending != null && openCount > 0)
            {
                stack.Push((pending, braceDepth + 1));
                pending = null;
            }

            braceDepth += openCount - closeCount;
            while (stack.Count > 0 && braceDepth < stack.Peek().Depth)
            {
                stack.Pop();
            }
        }

        return contexts;
    }

    private static void AddInterfaceMember(ProjectInfo projectInfo, string interfaceName, string memberName)
    {
        if (!projectInfo.InterfaceMembers.TryGetValue(interfaceName, out var members))
        {
            members = new HashSet<string>(StringComparer.Ordinal);
            projectInfo.InterfaceMembers[interfaceName] = members;
        }

        members.Add(memberName);
    }

    private static void AddInterfaceBases(ProjectInfo projectInfo, string interfaceName, HashSet<string> baseInterfaces)
    {
        if (!projectInfo.InterfaceBases.TryGetValue(interfaceName, out var interfaces))
        {
            interfaces = new HashSet<string>(StringComparer.Ordinal);
            projectInfo.InterfaceBases[interfaceName] = interfaces;
        }

        interfaces.UnionWith(baseInterfaces);
    }

    private static bool CanUseInheritDoc(string line, TypeContext currentType, ProjectInfo projectInfo, string memberName)
    {
        if (Regex.IsMatch(line, @"\boverride\b"))
            return true;

        if (currentType == null || currentType.Interfaces.Count == 0)
            return false;

        foreach (var interfaceName in currentType.Interfaces)
        {
            var members = GetResolvedInterfaceMembers(interfaceName, projectInfo);
            if (members.Contains(memberName))
                return true;
        }

        return false;
    }

    private static HashSet<string> GetResolvedInterfaceMembers(string interfaceName, ProjectInfo projectInfo)
    {
        if (projectInfo.ResolvedInterfaceMembers.TryGetValue(interfaceName, out var cached))
            return cached;

        var resolved = new HashSet<string>(StringComparer.Ordinal);
        ResolveInterfaceMembers(interfaceName, projectInfo, resolved, new HashSet<string>(StringComparer.Ordinal));
        projectInfo.ResolvedInterfaceMembers[interfaceName] = resolved;
        return resolved;
    }

    private static void ResolveInterfaceMembers(string interfaceName, ProjectInfo projectInfo, HashSet<string> resolved, HashSet<string> visiting)
    {
        if (!visiting.Add(interfaceName))
            return;

        if (projectInfo.InterfaceMembers.TryGetValue(interfaceName, out var members))
        {
            resolved.UnionWith(members);
        }

        if (projectInfo.InterfaceBases.TryGetValue(interfaceName, out var baseInterfaces))
        {
            foreach (var baseInterface in baseInterfaces)
            {
                ResolveInterfaceMembers(baseInterface, projectInfo, resolved, visiting);
            }
        }

        visiting.Remove(interfaceName);
    }

    private static HashSet<string> ExtractInterfaceNames(string bases)
    {
        var interfaces = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(bases))
            return interfaces;

        foreach (var rawPart in bases.Split(','))
        {
            var part = rawPart.Trim();
            if (part.Length == 0)
                continue;

            var whereIndex = part.IndexOf(" where ", StringComparison.Ordinal);
            if (whereIndex >= 0)
            {
                part = part.Substring(0, whereIndex).Trim();
            }

            var match = Regex.Match(part, @"(?:[A-Za-z_]\w*\.)*(?<name>[A-Za-z_]\w*)");
            if (!match.Success)
                continue;

            var name = match.Groups["name"].Value;
            if (name.StartsWith("I", StringComparison.Ordinal) && name.Length > 1 && char.IsUpper(name[1]))
            {
                interfaces.Add(name);
            }
        }

        return interfaces;
    }

    private static bool HasInheritDocBefore(string[] lines, int index)
    {
        for (var i = index - 1; i >= 0 && i >= index - 8; i--)
        {
            var text = lines[i].Trim();
            if (text.Contains("/// <inheritdoc", StringComparison.OrdinalIgnoreCase)) return true;
            if (text.Length == 0) continue;
            if (text.StartsWith("///", StringComparison.Ordinal)) continue;
            if (text.StartsWith("[", StringComparison.Ordinal)) continue;
            break;
        }
        return false;
    }

    private static int CountCharOutsideStrings(string line, char target)
    {
        var count = 0;
        var inString = false;
        var inChar = false;
        var escape = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '\\' && (inString || inChar))
            {
                escape = !escape;
                continue;
            }

            if (c == '"' && !escape && !inChar)
            {
                inString = !inString;
            }
            else if (c == '\'' && !escape && !inString)
            {
                inChar = !inChar;
            }
            else if (!inString && !inChar && c == target)
            {
                count++;
            }

            escape = false;
        }

        return count;
    }

    private static bool HasSummaryBefore(string[] lines, int index)
    {
        for (var i = index - 1; i >= 0 && i >= index - 8; i--)
        {
            var text = lines[i].Trim();
            if (text.Contains("/// <summary>")) return true;
            if (text.Length == 0) continue;
            if (text.StartsWith("///", StringComparison.Ordinal)) continue;
            if (text.StartsWith("[", StringComparison.Ordinal)) continue;
            break;
        }
        return false;
    }

    private static string StripLineCommentForCode(string line)
    {
        var inString = false;
        var escape = false;
        for (var i = 0; i < line.Length - 1; i++)
        {
            var c = line[i];
            if (c == '\\' && inString)
            {
                escape = !escape;
                continue;
            }
            if (c == '"' && !escape)
            {
                inString = !inString;
            }
            escape = false;
            if (!inString && c == '/' && line[i + 1] == '/')
            {
                return line.Substring(0, i);
            }
        }
        return line;
    }

    private static string ExtractComment(string line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("//", StringComparison.Ordinal)) return trimmed;

        var inString = false;
        var escape = false;
        for (var i = 0; i < line.Length - 1; i++)
        {
            var c = line[i];
            if (c == '\\' && inString)
            {
                escape = !escape;
                continue;
            }
            if (c == '"' && !escape)
            {
                inString = !inString;
            }
            escape = false;
            if (!inString && c == '/' && line[i + 1] == '/')
            {
                return line.Substring(i).TrimStart();
            }
        }
        return null;
    }

    private static bool IsPascalCase(string name)
    {
        return Regex.IsMatch(name, @"^[A-Z][A-Za-z0-9]*$");
    }

    private static bool IsBoolType(string type)
    {
        return type == "bool" || type == "Boolean" || type == "System.Boolean";
    }

    private static bool HasBoolPrefix(string name)
    {
        var plain = name;
        if (plain.StartsWith("_", StringComparison.Ordinal)) plain = plain.Substring(1);
        if (plain.StartsWith("s_", StringComparison.Ordinal)) plain = plain.Substring(2);
        return Regex.IsMatch(plain, @"^(is|has|can|should)[A-Z_].*") || Regex.IsMatch(plain, @"^(Is|Has|Can|Should)[A-Z].*");
    }

    private static void Add(List<Violation> violations, string severity, string code, string root, string file, int line, string message)
    {
        violations.Add(new Violation
        {
            Severity = severity,
            Code = code,
            File = Path.GetRelativePath(root, file).Replace('\\', '/'),
            Line = line,
            Message = message
        });
    }

    private static void PrintText(string root, List<string> files, List<Violation> violations, int errorCount, int warnCount)
    {
        Console.WriteLine("Unity Code Guard lint");
        Console.WriteLine($"Root: {root}");
        Console.WriteLine($"Files scanned: {files.Count}");
        Console.WriteLine($"Errors: {errorCount}  Warnings: {warnCount}");
        Console.WriteLine(errorCount == 0 ? "PASS" : "FAIL");

        foreach (var v in violations.OrderBy(v => v.File).ThenBy(v => v.Line).ThenBy(v => v.Code))
        {
            Console.WriteLine($"{v.Severity} {v.Code} {v.File}:{v.Line} {v.Message}");
        }
    }

    private static void PrintJson(string root, List<string> files, List<Violation> violations, int errorCount, int warnCount)
    {
        string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n");
        Console.WriteLine("{");
        Console.WriteLine($"  \"root\": \"{Escape(root)}\",");
        Console.WriteLine($"  \"filesScanned\": {files.Count},");
        Console.WriteLine($"  \"errors\": {errorCount},");
        Console.WriteLine($"  \"warnings\": {warnCount},");
        Console.WriteLine($"  \"status\": \"{(errorCount == 0 ? "PASS" : "FAIL")}\",");
        Console.WriteLine("  \"violations\": [");
        for (var i = 0; i < violations.Count; i++)
        {
            var v = violations[i];
            var comma = i == violations.Count - 1 ? "" : ",";
            Console.WriteLine($"    {{ \"severity\": \"{v.Severity}\", \"code\": \"{v.Code}\", \"file\": \"{Escape(v.File)}\", \"line\": {v.Line}, \"message\": \"{Escape(v.Message)}\" }}{comma}");
        }
        Console.WriteLine("  ]");
        Console.WriteLine("}");
    }
}

return GuardLint.Run(Args);

