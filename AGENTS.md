# AGENTS.md

请使用中文写提案和回答，这个文件为 AI Coding Agent 提供指导，用于处理此项目中的代码。

## 项目进度

本项目当前处于整体代码重构阶段，一切以用户输出为主，以审视角度查阅代码。

## 项目环境

- Unity Editor：2022.3.62f3 (revision 96770f904ca7)
- 渲染管线：URP（com.unity.render-pipelines.universal 14.0.12）。写渲染/材质/后处理相关代码前先确认走 URP API，不要用 Built-in 管线写法。
- 关键包：
  - Input System 1.14.2（用新输入系统，不要用旧 Input Manager 的 `Input.GetAxis` 等）。
  - Cinemachine 2.10.7。
  - TextMeshPro 3.0.7。
  - Timeline 1.7.7。
- 动画框架：Animancer 8.0.0（com.kybernetik.animancer，UPM 包形式安装于 Packages/com.kybernetik.animancer）。用 Animancer 按需播放动画的 API，不要按旧 Mecanim Animator + Animator Controller 的状态机方式写。
- 脚本根目录：Assets/_Scripts，下分 Camera、Character、Effects、Event、Input、Team、UI 等子目录。

## 编码前先读

开始写或改 C# 脚本前，先阅读 `$unity-coding-rules-mini` 技能。在命名、注释、Inspector 字段、生命周期与事件订阅四个护栏领域严格遵守其约定；护栏之外（架构、异步方案、性能等）按最佳判断自由发挥。

## 解决方案文件说明

本项目根文件夹曾多次更名，旧名 "Combat System"。
- `.sln` / `.csproj` 由 Unity 自动生成，且已在 .gitignore 中忽略，不要纳入版本控制。
- 如 IDE（VSCode / Rider 等）读取解决方案异常，删除根目录下所有 `.sln` 和 `.csproj` 及 `obj/`，切回 Unity 重新编译即可生成本次更新的解决方案 `UnityProject.sln`。
- `.vscode/settings.json` 中 `dotnet.defaultSolution` 指向 `UnityProject.sln`，保持该项与 Unity 重新生成的文件名一致。

## UnitySkills 服务器

- 地址：localhost:8090。
- 用于通过 REST API 自动化 Unity Editor 操作：创建/修改脚本、场景、预制体、组件、资源等；修改脚本后可触发编译验证（见 `unity-compile` skill）。
- 改动场景、预制体、脚本等持久化内容后，优先用 UnitySkills 确认编译与引用状态，不要只改不验。