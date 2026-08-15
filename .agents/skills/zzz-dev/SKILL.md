---
name: zzz-dev
description: 让 AI 快速上手 My-ZZZ 项目各模块的 API。关键词：输入、角色、摄像机、队伍、事件、UI 等模块的代码读写时加载。
---

# zzz 开发指导

本项目各模块遵循"代码组装"架构：模块自身代码独立，模块间协调通过胶水连接。每个模块有明确的对外边界（Contract），外部只允许引用其 Contract；跨模块顺序业务流程归编排层（SPFlow）。本 skill 提供 AI 专用的精炼参考文档。

## 通用规则

1. **先查再写**：涉及某模块时先读对应参考文档，确认现有 API，避免冗余方法。
2. **守边界**：只引用目标模块的 `*.Contract` 命名空间（契约、事件与数据类型），不直接引用其 Core/Debug/Wiring。
3. **先判通信语义**：跨模块交流前先判断是“能力/状态借用”还是“事实广播”，避免把事件总线当命令通道。
4. **编排层**：不属于任何模块的跨模块顺序业务流程放编排层 `SPFlow`（如 `TeamAssemblyFlow`）；归属判断参考 framework-core.md 的启用判据。

## 文档路由

按需加载对应模块的参考文档：

| 模块             | 参考文档                                                         | 何时加载                                       |
| ---------------- | ---------------------------------------------------------------- | ---------------------------------------------- |
| 框架核心（访问级别 + 模块通讯 + 编排层） | [references/framework-core.md](references/framework-core.md) | 设计/审查跨模块引用、`public`/`internal` 语义、事件总线、模块服务接线、编排层（Flow）启用判据 |
| 输入模块 (Input) | [references/input-module-api.md](references/input-module-api.md) | 读写输入相关代码、下游需要获取帧输入、接线调试 |
| 摄像机模块 (Camera) | [references/camera-module-api.md](references/camera-module-api.md) | 输入方向转相机系世界移动方向、平滑跟随目标、坐标转换接线 |
| 角色模块 (Character) | [references/character-module-api.md](references/character-module-api.md) | 读写角色控制器代码、搭状态节点/状态机配置、接意图供给源（玩家/AI）、排查动画/运动/时序问题 |
| 资源模块 (Resource) | [references/resource-module-api.md](references/resource-module-api.md) | 按资源键同步实例化/批量创建预制体、统一管理实例释放、配置资源目录与模块服务接线 |
| 队伍模块 (Team) | [references/team-module-api.md](references/team-module-api.md) | 读写队伍/切换相关代码、接入队伍服务、订阅队伍事件、排查切换时序与相机跟随 |

> 尚未列出的模块暂无独立参考文档，以 framework-core.md 为准。



