# AGENTS.md

请使用中文写提案和回答，这个文件为 AI Coding Agent 提供指导，用于处理此项目中的代码。

## 项目环境

- Unity Editor：2022.3 LTS
- 渲染管线：URP。写渲染/材质/后处理相关代码前先确认走 URP API，不要用 Built-in 管线写法。
- 关键约束：用 Input System（不要用旧 Input Manager 的 `Input.GetAxis` 等）；动画用 Animancer 的按需播放 API（不要 Mecanim Animator + Animator Controller 状态机）。
- 其余包：Cinemachine、TextMeshPro、Timeline。
- 脚本根目录：`Assets/_Scripts`。

## 注意

- 仅允许修改 `_Scripts/` 目录下的 `.cs` 文件；`.meta` 和 `.asset` 文件应由 Unity 自动维护，或在用户明确创建/修改时再处理。
