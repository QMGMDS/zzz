---
name: dev-workflow
description: 开发任务标准工作流（主 Agent 编排）。用户提出任何涉及代码改动的开发需求——新功能、修 Bug、重构、调整 Assets/_Scripts 下的脚本——或说"走工作流 / 按流程来 / dev-workflow"时触发。流程：加载 zzz-dev 项目文档 → 派发只读 Explore 子 agent 实地调研 → 列任务清单 → 派发 general-purpose 子 agent 执行 → 主 Agent 审查并过 unity-code-guard 硬门禁，不通过则打回重做。纯问答、查看、咨询类请求不触发本流程。
---

# dev-workflow — 开发任务标准工作流

主 Agent 在本流程中只做四件事：**加载文档、派发、审查、把关**。不亲自调研、不亲自写业务代码——这保证调研与执行的上下文干净，且执行结果可以整体打回重做。

## 阶段 0：适用判定

- 走本流程：用户需求最终会改动 `Assets/_Scripts` 下的 `.cs` 文件（新功能、修 Bug、重构）。
- 不走本流程：纯问答、架构咨询、只看不改的请求 → 直接回答；单文件一两行的小修正（如错字）可直接做，但写完 C# 后仍须触发 `unity-code-guard` 走门禁。
- 判定模糊时，问一句用户。

## 阶段 1：加载项目文档

通过 Skill 工具加载 `zzz-dev`，按其文档路由表读取与本次需求相关的模块文档。目的：主 Agent 先具备项目结构知识，才能写出高质量的调研派发单。

## 阶段 2：派发调研子 agent（子 agent 1）

1. 按 [templates/research-brief.md](templates/research-brief.md) 填写调研派发单：需求原文、调研范围、要回答的问题。
2. 用 Agent 工具派发，`subagent_type` 用 `Explore`（只读类型，防止调研过程顺手改代码）。
3. 派发后**等待结果，不要自己也去做同样的调研**——重复劳动且结论可能分叉。
4. 收到调研报告后逐题核对派发单：没答全的问题再次派发补调，不要自己下场补。

发现调研结论与 `zzz-dev` 文档描述不符时记录下来，在最终汇报中告知用户（由用户判断文档是否需要更新，见 AGENTS.md）。

## 阶段 3：列任务清单

1. 基于调研报告，用 TodoWrite 列出任务清单。每项任务必须有明确的改动点（文件/模块）和完成标准（可验证的行为）。
2. 向用户简报清单（一段摘要即可，不必等待确认），随后立即进入阶段 4。

## 阶段 4：派发执行子 agent（子 agent 2）

1. 按 [templates/execution-task.md](templates/execution-task.md) 填写执行派发单。
2. 用 Agent 工具派发，`subagent_type` 用 `general-purpose`。
3. 子 agent **不继承本会话上下文**，派发单必须自包含：项目硬约束（URP、Input System、Animancer）、任务清单、调研结论摘要，缺一不可。

## 阶段 5：审查与打回

收到执行结果后，主 Agent 依次审查：

1. **完成度**：逐项核对任务清单，读实际 diff，不以子 agent 的自述为准。
2. **项目约束**：URP API、Input System、Animancer、模块"代码组装"边界、未手改 `.meta` / `.asset`。
3. **硬门禁**：触发 `unity-code-guard` 走机械门禁；出现 ERROR 即不通过。

**不通过 → 打回**：用 SendMessage 向执行子 agent 的 agentId 发送审查意见（该 agent 会被唤醒继续工作）。审查意见必须逐条可执行：`文件:行号` + 问题 + 期望行为。打回后回到本阶段重新审查。

**通过 → 收尾**：更新 TodoWrite 全部完成，按下方汇报格式向用户交付。

同一任务的打回上限为 **3 次**：第 3 次仍不通过时停止打回，把分歧点整理成选项交给用户决策。

## 汇报格式

收尾时只汇报以下内容：

- 调研结论一句话摘要
- 任务清单及各项完成状态
- 审查轮数（含打回原因概要）
- 门禁结果（PASS / 剩余 WARN 基线）
- 与 zzz-dev 文档不符之处（如有）
