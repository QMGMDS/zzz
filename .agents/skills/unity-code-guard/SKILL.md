---
name: unity-code-guard
description: Unity C# 编辑硬门禁。用于 AI 编辑、审查、恢复或验证 Unity C# 脚本、Inspector 暴露字段、XML 注释（summary / inheritdoc）、生命周期与事件订阅时触发；用户要求运行代码门禁、硬门禁、lint.csx、软复核或 Unity 代码规则时也触发。激活后必须先运行随附机械检查脚本，再根据结果决定继续、修复或停止；编辑后必须再次运行检查。
---

# unity-code-guard

本 skill 是 Unity C# 编辑硬门禁，不是风格建议。

## 强制工作流

1. 定位 Unity 项目根目录
   - 优先使用同时包含 `Assets/` 与 `ProjectSettings/` 的目录
   - 如果找不到 Unity 项目根目录，则使用当前仓库根目录
2. 编辑或恢复工作前，先运行机械门禁
   - 命令：`& <本 skill 目录>/tools/lint/run-guard.ps1 <项目根目录>`
   - 如果只需要检查指定文件，在命令末尾追加 `--files` 和文件路径
   - 默认只扫描 `<项目根目录>/Assets/_Scripts` 下的 `.cs` 文件，其他目录不扫描
3. 读取检查输出后再决定下一步
   - `PASS`：允许继续任务
   - `ERROR`：停止正常工作，先修复任务范围内的违规，再重新运行检查
   - `WARN`：允许继续，但必须识别风险，并且不得新增同类风险
4. 编辑 C# 文件后，必须再次运行同一机械门禁
5. 门禁仍报告 `ERROR` 时，不得交付任务
6. 如果项目原本已经存在无关错误，将其记录为基线，不得制造新错误

## 资源

- 权威规则：`references/rules.md`
- 门禁协议：`references/gate-protocol.md`
- 软复核清单：`references/soft-review-checklist.md`
- 机械检查脚本：`tools/lint/lint.csx`

## 失败处理

- 不得用人工阅读替代机械门禁
- 不得因为修改很小就跳过门禁
- 不得通过削弱规则来压制门禁结果
- 如果门禁无法运行，必须报告硬门禁不可用，并说明原因
- 如果需要规则例外，必须先询问用户

## 交付说明

完成任务时，只报告以下内容：

- 预检查是通过、失败，还是存在基线问题
- 复检是否通过
- 是否存在需要用户知道的剩余警告
