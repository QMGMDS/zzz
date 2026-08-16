# 注释与 XML 文档

## 定位

约束注释的用语、格式与 XML 文档注释（`<summary>`、`inheritdoc`）的适用范围。序列化字段的 `[Tooltip]` 要求见 `04-serialization.md`。

## 规则条目

### 行内注释

- **CMT-01** [人工] 注释只写必要信息，禁止解释显而易见的代码。
- **CMT-02** [人工] 注释不得引用脚本名、文件名、类名作为重复说明。
- **CMT-03** [机械 UCG201] 注释中不使用全角破折号 `——`，统一使用 ` - `。
- **CMT-04** [机械 UCG202] 注释中不使用句号 `。`。
- **CMT-05** [人工] 注释必须简短、直接、可验证。
- **CMT-06** [人工] 注释必须与代码同步，代码变化后注释失效时应立即更新或删除。
- **CMT-07** [机械 UCG203] 禁止写"临时"、"待优化"、"后续再改"及 `TODO/FIXME/HACK/TEMP` 之类无执行价值注释。

### 类注释

- **CMT-08** [机械 UCG101] 类、结构体、接口必须提供 `<summary>`，只描述职责，不描述实现细节。

### 方法与属性注释

- **CMT-09** [机械 UCG401] 新声明的公有或受保护方法必须提供 XML `<summary>`；实现接口成员或重写父类成员时可使用 `/// <inheritdoc />`。
- **CMT-10** [机械 UCG401 反向] 非接口实现、非重写的方法不得使用 `/// <inheritdoc />`。
- **CMT-11** [机械 UCG501] 新声明的公有或受保护属性必须提供 XML `<summary>`；实现接口成员或重写父类成员时可使用 `/// <inheritdoc />`。
- **CMT-12** [机械 UCG501 反向] 非接口实现、非重写的属性不得使用 `/// <inheritdoc />`。
- **CMT-13** [人工] 重写成员有额外约束、副作用或局部差异时，可在 `inheritdoc` 之外补充简短说明。
- **CMT-14** [人工] 只有在逻辑复杂且命名无法充分表达时，私有方法才允许加注释。
- **CMT-15** [人工] 参数和返回值只在有歧义或有约束时说明。

### 常量与字段注释

- **CMT-16** [人工] 常量必须提供 `<summary>`；公开属性注释只说明语义，不解释推导过程。
- **CMT-17** [人工] 私有字段默认不写行内注释；只有当字段意图无法从命名直接判断时，才允许补充最短必要注释。
- **CMT-18** [人工] 可序列化字段优先使用 `[Tooltip]`，不用行内注释。

## 正例与反例

```csharp
/// <summary>
/// 队伍控制器 - 管理角色切换与状态分发
/// </summary>
public class TeamController : MonoBehaviour
{
    /// <summary>最大血量上限</summary>
    public int MaxHp => _maxHp;

    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    /// <param name="amount">伤害量 必须大于 0</param>
    /// <returns>实际扣减量</returns>
    public int TakeDamage(int amount) { }

    /// <inheritdoc />
    public override CharacterIntentionFrame CurrentFrame { get; }
}
```

```csharp
// 反例
public class TeamController {}           // 违反 CMT-08：缺类 summary（UCG101）
/// <summary>这是 TeamController 的方法</summary>  // 违反 CMT-02：引用类名重复说明
public void DoWork() {}                  // 违反 CMT-09：公有方法缺 summary（UCG401）
/// <inheritdoc />                       // 违反 CMT-10：非继承成员误用 inheritdoc（UCG401）
public void DoWork() {}
// TODO 后续再改 —— 这里逻辑很复杂。    // 违反 CMT-07、CMT-03、CMT-04（UCG203/201/202）
```

## 例外情况

- `*.Designer.cs`、`*.g.cs`、`*.generated.cs` 等生成代码豁免（与 `.editorconfig` 的生成代码豁免一致）。
- 其余无例外，确需例外时按 `00-scope.md` 的 SCP-06 处理。

## 与 lint 的映射表

| UCG | 级别 | 规则条目 | 修复指引 |
|---|---|---|---|
| UCG101 | ERROR | CMT-08 | 补类/结构体/接口的 `<summary>`，只述职责 |
| UCG201 | ERROR | CMT-03 | `——` 替换为 ` - ` |
| UCG202 | ERROR | CMT-04 | 删除注释中的 `。` |
| UCG203 | ERROR | CMT-07 | 删除 TODO/FIXME/HACK/TEMP 标记或落地为正式注释 |
| UCG401 | ERROR | CMT-09 / CMT-10 | 公有/受保护方法补 summary；非继承成员移除 inheritdoc |
| UCG501 | ERROR | CMT-11 / CMT-12 | 公有/受保护属性补 summary；非继承属性移除 inheritdoc |

注：inheritdoc 资格由 lint 全量扫描 `_Scripts` 下的接口信息判定；若 lint 判定与实际不符（如动态生成代码），按 SCP-05 停止并询问用户。
