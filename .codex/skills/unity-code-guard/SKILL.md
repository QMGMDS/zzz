---
name: unity-code-guard
description: My-ZZZ 项目的整套代码审查机制（规则权威定义 + 硬门禁 + 软复核）。护栏内由本 skill 管，护栏外按最佳判断自由发挥、不要为迁就编码者而降级。改完 C# 脚本后必走本 skill 的门禁链路；用户说"跑一次软复核"时也由本 skill 处理。
---

# unity-code-guard — 代码审查机制总路由

本 skill 是 My-ZZZ 的**整套代码审查机制**：规则权威定义（护栏四域）+ 硬门禁（lint.csx 后置校验）+ 软复核（语义条目自检）。

护栏范围仅四域：命名、注释、Inspector 字段、生命周期与事件订阅。护栏之外（架构、程序集、异步方案、性能、对象池、热更新等）**不设本 skill 限制**——AI 按最佳判断自由选用更优写法，不要为"迁就编码者当前水平"而降级或回避高级写法。

## 触发点

- 写/改 C# 脚本后 → 走硬门禁链路（必走，不靠自觉）。
- 用户说"跑一次软复核" → 走软复核分支。

## 路由表（按需读 references/，不要一次全载）

| 要做什么 | 读哪个 |
|---|---|
| 查规则的权威定义（命名/注释/Inspector/生命周期） | `references/rules.md` |
| 跑硬门禁 / 读懂门禁链路与基线机制 | `references/gate-protocol.md` |
| 软复核自检清单 | `references/soft-review-checklist.md` |

## 边界说明

硬门禁的物理执行器是 `tools/lint/lint.csx`（基于 Roslyn AST 的 dotnet script，在仓库根级 `tools/lint/`，不在 skill 内）。本 skill 只描述**如何使用**它、门禁怎么走、违规怎么处理；不复制其实现。编译触发通过 UnitySkills REST（localhost:8090）直连。
