# CLAUDE.md

## 项目概述

复刻绝区零（Zenless Zone Zero）战斗系统的 Unity 3D 项目。使用 URP 渲染管线、New Input System、Cinemachine 摄像机系统、Behavior Designer 行为树、Animancer 动画状态机

- **Unity**: 2022.3.62f3
- **渲染管线**: URP 14.0.12
- **输入系统**: New Input System 1.14.2
- **摄像机**: Cinemachine 2.10.7
- **AI**: Behavior Designer (第三方插件)
- **状态机**: Animancer (第三方插件)
- **解决方案文件**: `Combat System.sln`

## 项目进度

本项目当前正在开发玩家角色控制器，一切代码服务于玩家角色控制器。

重构中.......
一切代码都可能是干扰项，一切以用户指令为准。

## 编码规范

### 命名规范

- **私有字段**: `_camelCase`（下划线前缀），如 `_currentState`
- **其它所有**: PascalCase（局部变量、类名、方法、属性、常量、枚举、事件）
- **接口**: `I` 前缀，泛型 `T` 前缀，异步方法 `Async` 后缀

### 注释规范

- **类**: 写 `<summary>` 说明职责
- **公有/保护方法**: 三行 XML 注释（`<summary>` + `<param>` 每个参数 + `<returns>`）
- **接口方法/属性**: 单行 XML 注释

### 其他

- **Inspector 字段**: 必须带 `[Tooltip]`

## 项目架构



## 注意事项

- 向用户输出的内容必须严格按照 `docs\AI Personalization.md` 文件执行。
- 除非用户说明，否则不要直接恢复已删除文件。
- Plan 模式下，你拥有工作区所有文件的访问权，无任何编写权。
- Build 模式下，你拥有工作区所有文件的访问权，只拥有 _Scripts 文件夹下代码的编写权。
