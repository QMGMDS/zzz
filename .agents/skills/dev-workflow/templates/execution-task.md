# 执行派发单（子 agent 2 · general-purpose）

> 用途：主 Agent 填写本单后，通过 Agent 工具以 `subagent_type: general-purpose` 派发。
> 填写原则：自包含——项目硬约束、任务清单、调研结论必须全部写进来，子 agent 看不到主会话上下文。占位符一律删除。

## 需求原文

{{用户需求的原文}}

## 项目硬约束（不可违反）

- Unity 2022.3 LTS + URP：渲染/材质/后处理只走 URP API，禁用 Built-in 管线写法
- 输入只用 Input System，禁用旧 Input Manager（`Input.GetAxis` 等）
- 动画只用 Animancer 按需播放 API，不用 Mecanim Animator 状态机
- 只允许修改 `Assets/_Scripts` 下的 `.cs` 文件；`.meta` / `.asset` 由 Unity 自动维护，不手改
- 架构遵循模块"代码组装"原则：模块代码独立，通过契约组装

## 任务清单

1. {{任务}} — 完成标准：{{可验证的行为}}
2. {{……}}

## 调研结论摘要

{{子 agent 1 报告的要点：改动点 文件:行号、相关 API、波及的调用方、可复用的既有实现}}

## 完成要求

- 逐项完成任务后，通过 Skill 工具触发 `unity-code-guard` 走硬门禁；出现 ERROR 必须自行修复后重跑，直到 PASS（剩余 WARN 记录为基线）
- 不得超出任务清单范围改动无关文件
- 返回：改动文件清单、每项任务的完成情况、门禁结果与剩余 WARN、任何未决问题
