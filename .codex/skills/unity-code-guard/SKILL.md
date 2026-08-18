---
name: unity-code-guard
description: Unity C# 编辑硬门禁。用于 AI 编辑、审查、恢复或验证 Unity C# 脚本、Inspector 暴露字段、XML 注释（summary / inheritdoc）、生命周期与事件订阅时触发；用户要求运行代码门禁、硬门禁、lint.csx 或 Unity 代码规则时也触发。激活后先判定改动是否涉及 Assets/_Scripts 下的 .cs，再运行随附机械检查脚本；出现 ERROR 无需用户确认、自行复工并回走门禁；编辑后必须再次运行检查。
---

# unity-code-guard

本 skill 是 Unity C# 编辑硬门禁，不是风格建议。

软复核（架构越界检查）规范待制定；制定前本 skill 只负责硬门禁。

## 强制工作流

1. 入口判定：确认本轮改动是否包含 `Assets/_Scripts` 下的 `.cs` 文件；不包含则不走本门禁
2. 定位 Unity 项目根目录
   - 优先使用同时包含 `Assets/` 与 `ProjectSettings/` 的目录
   - 找不到 Unity 项目根目录时，使用当前仓库根目录
3. 编辑或恢复工作前，先运行机械门禁
   - 命令：`& <本 skill 目录>/tools/lint/run-guard.ps1 <项目根目录>`
   - 只检查指定文件时，在命令末尾追加 `--files` 和文件路径
   - 默认只扫描 `<项目根目录>/Assets/_Scripts` 下的 `.cs` 文件，其他目录不扫描
4. 读取检查输出后再决定下一步
   - `PASS`：允许继续任务
   - `ERROR`：无需用户确认，停止正常工作，自行修复任务范围内的违规，修复后从第 1 步回走一遍门禁
   - `WARN`：允许继续，但必须识别风险，并且不得新增同类风险
5. 编辑 C# 文件后，必须再次运行同一机械门禁
6. 门禁仍报告 `ERROR` 时，不得交付任务
7. 项目原本已经存在的无关错误，将其记录为基线，不得制造新错误

## 规则加载方式

规则正文按主题拆分在 `references/rules/` 下，通过 `references/rules.md` 的路由表按需加载，不要求全量阅读：

- **错误驱动**：lint 报出某类 UCG 代码后，查路由表加载对应主题文档，按规则条目修复
- **预防驱动**：即将编辑敏感写法（Inspector 字段、事件订阅、公开成员等）前，预加载对应主题文档

## 资源

- 规则索引与路由表：`references/rules.md`
- 主题规则文档：`references/rules/00-scope.md` 至 `references/rules/06-lifecycle-events.md`
- 门禁执行协议：`references/gate-protocol.md`
- 机械检查脚本：`tools/lint/lint.csx`

## 失败处理

- 不得用人工阅读替代机械门禁
- 不得因为修改很小就跳过门禁
- 不得通过削弱规则来压制门禁结果
- 门禁无法运行时，必须报告硬门禁不可用，并说明原因
- 需要规则例外时，必须先询问用户

## 交付说明

完成任务时，只报告以下内容：

- 预检查是通过、失败，还是存在基线问题
- 复检是否通过
- 是否存在需要用户知道的剩余警告
