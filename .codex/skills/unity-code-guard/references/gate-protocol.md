# 硬门禁执行流程

## 前置条件

编译这步直接调 UnitySkills（localhost:8090）下的 Skill。链路成立的前提：
- Unity Editor 处于打开状态。
- UnitySkills 服务器在 localhost:8090 在线。

不满足时跳转到下文链路的"服务不可用"分支——提示用户，**不要**把它当编译失败去改代码。

## 链路

```
AI 改完脚本
  └─> 触发编译（直接调 UnitySkills，见下文「触发编译」）
        ├─ 服务不可用 → 提示用户检查 Unity Editor / UnitySkills 服务器，不自动改代码
        ├─ 编译失败（代码错）→ console 错误回灌 → 修 → 回编译
        └─ 编译通过
              └─> 硬门禁：dotnet script tools/lint/lint.csx -- <scriptsRoot> --baseline tools/lint/.lint-baseline
                    ├─ new violations 非空 → 清单回灌 → 按条目修 → 回编译 + 回门禁
                    └─ new violations 为空 → 可交付（附"硬门禁已过"）
                          └─[用户说"跑一次软复核"] → 见 soft-review-checklist.md
```
硬门禁不过必须返工到过，不靠"自觉记约定"。

## 触发编译（直接调 UnitySkills）

UnitySkills 编译相关有三个 Skill，职责不同：

| Skill | 模块 | 是否触发编译 | 用途 |
|---|---|---|---|
| `asset_refresh` | asset | 是 | 调 `AssetDatabase.Refresh()`，触发脚本重编译 |
| `debug_check_compilation` | debug | 否 | 查询当前是否正在编译（`isCompiling` / `isUpdating`）|
| `debug_get_errors` | debug | 否 | 读取控制台当前错误与异常 |

只有 `asset_refresh` 主动触发编译；`debug_check_compilation` / `debug_get_errors` 只读取状态。

标准工作流：

```
1. POST /skill/asset_refresh          ← 触发 AssetDatabase.Refresh
2. 等待 5-8 秒（若返回含 retryAfterSeconds / mayDisconnect 则按其等待后重试）
3. POST /skill/debug_check_compilation ← 查询是否仍在编译
4. POST /skill/debug_get_errors        ← 拉取编译/控制台错误（含 message）
5. errors 为空 → 编译通过；非空 → 按条目修 → 回 asset_refresh
```

陷阱：`asset_refresh` 执行后 Unity 开始编译，REST 服务器可能短暂断开（`mayDisconnect: true`）。需按返回的 `retryAfterSeconds` 等待后重试。

`debug_get_errors` 返回结构示例：

```json
{
  "entries": [
    { "type": "Error", "message": "...", "stackTrace": "..." }
  ]
}
```

`entries` 为空即视为当前无错误。注意错误条目来自控制台快照，编译刷新期间可能短暂滞后，必要时隔几秒再拉一次。

`debug_check_compilation` 是有效 Skill 名，别拼成 `check_compilation`。

## 两层机制（不要混淆）

| 层 | 执行器 | 强制力 | 能力面 |
|---|---|---|---|
| 提示层/预防层 | `.editorconfig` + IDE 分析器（`UnityProject/.editorconfig`） | `warning` 波浪线，**不阻止 csc 编译**，AI 可无视 | 仅标识符前缀/大小写 |
| 硬门禁/出口关卡 | `lint.csx`（基于 Roslyn AST） | exit code：new violations 非空 → 必须返工到过；为空 → 才可交付 | 8 条规则全管（类型筛选、Update 热路径、`[Tooltip]`、`——`、XML 存在等）|

一句话：`.editorconfig` 是编辑器红波浪线（边写边提示，可无视），`lint.csx` 才是出口闸机（真正决定"能不能交付"）。

## lint.csx 的 8 条规则

| 编号 | 规则 | 对照 rules.md 章 |
|---|---|---|
| R1 PublicMutableField | public 字段非 const/readonly 报；`[NonSerialized] public` 豁免 | §3/§5 |
| R2 SerializeFieldPrivacy | `[SerializeField]` 必须 private | §3 |
| R3 TooltipRequired | `[SerializeField]` 必带 `[Tooltip]` | §3 |
| R4 NoEmDash | 注释/字符串/插值含 `——` 报 | §2 |
| R5 UpdateHotPath | Update 热路径禁 Find/GetComponent/GetChild/字符串拼接 | §4/§5 |
| R6 AsyncSuffix | 仅 Task/UniTask 要求 Async 后缀；协程 IEnumerator 豁免；override/On*/Handle* 豁免 | §1.2 |
| R7 BoolPrefix | 私有 bool 字段业务名（先剥 `_`）须 is/has/can/should 开头 | §1.1 |
| R8 ClassSummary/PublicMethodXml | 类缺 `<summary>`/public 方法缺 XML 报；override 豁免 | §2 |

## 日常用法

```powershell
$csx = "D:\AAA_APPData\Unity\Unity Project\My-ZZZ\tools\lint\lint.csx"
$scriptsRoot = "D:\AAA_APPData\Unity\Unity Project\My-ZZZ\UnityProject\Assets\_Scripts"
$baseline = "D:\AAA_APPData\Unity\Unity Project\My-ZZZ\tools\lint\.lint-baseline"

# 1) 改完代码跑门禁（AI 改完 → 编译过 → 跑这个）；需在无 .csproj 的目录运行
Push-Location $env:TEMP
dotnet script $csx -- $scriptsRoot --baseline $baseline
Pop-Location
# 0 new violations + HARD-GATE: PASS = 可交付；非空 = 必须返工

# 2) 修复存量后刷新基线（重点是"刷新"，不是"塞新违规"）
Push-Location $env:TEMP
dotnet script $csx -- $scriptsRoot --gen-baseline $baseline
Pop-Location
```

## 基线快照（重构期机制）

- 存量违规记入 `tools/lint/.lint-baseline`，门禁只拦截"新增"违规，存量通过重构逐步清零。
- 修复一条存量后重跑 `--gen-baseline` 刷新；**禁止为"过门禁"把新违规塞进基线**——那是放水，不是修代码。

## 执行注意

- `dotnet script` 会在含 `.csproj` 的目录里误判项目上下文，需在无 `.csproj` 的目录运行（用绝对路径调用即可）。
- 编译触发依赖外部服务 UnitySkills（localhost:8090），不属本 skill。服务离线时见链路"服务不可用"分支。