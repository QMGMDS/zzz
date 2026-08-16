---
name: zzz-dev
description: 让 AI 快速上手 My-ZZZ 项目各模块的 API。关键词：项目架构、各模块的代码读写、跨模块能力借用与事件订阅时加载。
---

# zzz 开发指导

本项目各模块遵循"代码组装"架构：模块自身代码独立。本 skill 提供 AI 专用的精炼参考文档。

## 通用规则

1. **必须加载**：[framework-core.md](references/framework-core.md) 作为项目开发最核心的文档，快速了解项目架构。
2. **先查再写**：涉及某具体模块时先读对应参考文档，确认现有 API，避免冗余方法。

## 文档路由

按需加载对应模块的参考文档：

| 模块 | 参考文档 | 何时加载 |
| ---- | -------- | -------- |
| 摄像机模块 (Camera) | [references/camera-module-api.md](references/camera-module-api.md) | 平面方向关联摄像机转世界 XZ 方向、设置摄像机跟随目标 |
| 角色模块 (Character) | [references/character-module-api.md](references/character-module-api.md) | 驱动单个角色上场/退场切换会话、锁定玩家操作、订阅角色切换事实 |
| 输入模块 (Input) | [references/input-module-api.md](references/input-module-api.md) | 读取当前帧玩家输入（移动方向、按键按下/长按状态） |
| 资源加载模块 (Resource) | [references/resource-module-api.md](references/resource-module-api.md) | 按资源键同步实例化预制体、销毁模块产出的实例 |
| 队伍模块 (Team) | [references/team-module-api.md](references/team-module-api.md) | 查询队伍切换状态、请求切换上场角色、移交装配结果、订阅队伍切换事实 |
