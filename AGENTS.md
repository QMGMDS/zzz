# AGENTS.md

请使用中文写提案和回答，这个文件为 AI Coding Agent 提供指导，用于处理此项目中的代码。

## 项目进度

本项目当前处于整体代码重构阶段，一切以用户输出为主，以审视角度查阅代码。

## 项目环境

- Unity Editor：2022.3 LTS
- 渲染管线：URP。写渲染/材质/后处理相关代码前先确认走 URP API，不要用 Built-in 管线写法。
- 关键约束：用 Input System（不要用旧 Input Manager 的 `Input.GetAxis` 等）；动画用 Animancer 的按需播放 API（不要 Mecanim Animator + Animator Controller 状态机）。
- 其余包：Cinemachine、TextMeshPro、Timeline。
- 脚本根目录：Assets/_Scripts，下分 Camera、Character、Effects、Event、Input、Team、UI 等子目录。

## 代码审查

写完 C# 脚本后必须触发 `$unity-code-guard` 技能走门禁；用户说"跑一次软复核"时也由该技能处理。

## 注意

- 仅允许修改 `_Script/` 目录下的 `.cs` 文件；`.meta` 和 `.asset` 文件应由 Unity 自动维护，或在用户明确创建/修改时再处理。