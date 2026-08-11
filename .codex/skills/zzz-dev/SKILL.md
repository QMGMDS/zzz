---
name: zzz-dev
description: 让 AI 快速上手 My-ZZZ 项目各模块的 API。关键词：输入、角色、摄像机、队伍、事件、UI 等模块的代码读写时加载。
---

# zzz 开发指导

本项目各模块遵循"代码组装"架构：模块自身代码独立，模块间协调通过胶水连接。每个模块有明确的对外边界（Contract + Wiring），外部只允许引用这两层。本 skill 提供 AI 专用的精炼参考文档。

## 通用规则

1. **先查再写**：涉及某模块时先读对应参考文档，确认现有 API，避免冗余方法。
2. **守边界**：只引用目标模块的 Contract + Wiring 命名空间，不直接引用其 Core/Debug。
3. **先判通信语义**：跨模块交流前先判断是“能力/状态借用”还是“事实广播”，避免把事件总线当命令通道。

## 文档路由

按需加载对应模块的参考文档：

| 模块             | 参考文档                                                         | 何时加载                                       |
| ---------------- | ---------------------------------------------------------------- | ---------------------------------------------- |
| 模块边界与通信规则 | [references/project-module-boundaries.md](references/project-module-boundaries.md) | 设计/审查跨模块引用、public/internal、事件总线、接口 + SO 信箱、协调层边界 |
| 输入模块 (Input) | [references/input-module-api.md](references/input-module-api.md) | 读写输入相关代码、下游需要获取帧输入、接线调试 |
| 摄像机模块 (Camera) | [references/camera-module-api.md](references/camera-module-api.md) | 输入方向转相机系世界移动方向、平滑跟随目标、坐标转换接线 |
| 角色模块 (Character) | [references/character-module-api.md](references/character-module-api.md) | 读写角色控制器代码、搭状态节点/状态机配置、接意图供给源（玩家/AI）、排查动画/运动/时序问题 |

> 后续模块（队伍、事件、UI 等）的参考文档将在各自重构完成后追加至此表。


