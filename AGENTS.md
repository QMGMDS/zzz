# AGENTS.md

This file provides guidance to OpenCode when working with code in this repository.

## 项目概述

复刻《绝区零》（Zenless Zone Zero）战斗系统的 Unity 3D 项目。
项目使用 URP 渲染管线、New Input System、Cinemachine、Behavior Designer 与 Animancer。

- **Unity**: 2022.3.62f3
- **渲染管线**: URP 14.0.12
- **输入系统**: New Input System 1.14.2（资产路径：`Assets/Settings/Input/Input System.inputactions`）
- **摄像机**: Cinemachine 2.10.7
- **AI**: Behavior Designer（第三方插件，编辑器内使用，代码仓库未包含源码）
- **动画状态机**: Animancer（第三方插件）
- **解决方案文件**: `Combat System.sln`

## 当前工作范围

项目当前只在做**玩家角色控制器**，所有代码都应为它服务。

## 项目架构

### 角色控制器方案

#### 代码目录

```
Assets/_Scripts/Player
├── Brain/             — 数据中枢模块
├── Input/             — 玩家输入模块
├── StateLogic/        — 角色状态模块-状态逻辑层
├── Animation/         — 角色状态模块-动画表现层
└── Motion/            — 角色状态模块-物理移动层
```
- 所有角色控制器相关代码位于 `SPPlayer` 命名空间。

#### 角色控制器的数据流

`PlayerController`（MonoBehaviour）是入口，使用 `[DefaultExecutionOrder(-300)]` 保证在大多数系统之前执行，每帧按固定管线推进：

```
Update:
  InputCollector.Update()  ── 采样 + 后处理（防抖 + BufferTimer）
    ↓
  InputMainProcessor.UpdateInputProcessors()  ── 意图翻译 → 黑板
    ↓
  StateMachine.CurrentState.LogicUpdate()  ── 全局拦截 → 状态逻辑
    ↓
  AnimationDriver.Update()  ── 下达动画指令

  [Animator 自动更新骨骼，产出本帧根位移]

OnAnimatorMove:
  PlayerMotor.ApplyMove()  ── 根运动缩放 + 旋转 + 重力

LateUpdate:
  AnimationDriver.SyncAnimProgress()  ── 动画进度回写黑板
    ↓
  PlayerBrain.ResetInputBrain()  ── 清除意图标记
```

关键子系统职责：

- **`PlayerController`**：装配子系统、严格时序分发，不写具体游戏逻辑。
- **`PlayerBrain`**：所有模块共享的数据黑板。
- **`InputSource`**：通过 `InputActionReference` 采样原始设备输入。
- **`InputCollector`**：维护当前帧/上一帧输入快照，提供 Move 防抖与 Attack/Evade 缓存窗口。
- **`InputMainProcessor`**：`IInputProcessor` 子处理器的工厂与驱动器，将处理后的输入翻译为意图写入黑板。

## 编码规范

### 命名

- **私有成员**：`_camelCase`（下划线前缀），例如 `_currentState`。
- **局部变量**：`camelCase`，例如 `temp`。
- **其它所有**：`PascalCase`（类名、方法名、常量名、属性、枚举、事件）。

- **接口**：`I` 前缀；泛型：`T` 前缀。

### 注释

注释要求简明扼要，不引用脚本名

- **类**：提供 `<summary>`，说明职责。
- **公有/保护方法**：三行 XML 注释（`<summary>` + 每个参数的 `<param>` + `<returns>`）。
- **接口方法/属性**：单行 XML 注释。

- **继承过来的 接口方法/父类方法**：`/// <inheritdoc />` 指明该方法是从父类或者接口继承而来的即可。

### 其它

- Inspector 字段必须带 `[Tooltip]`。

## 交互约定

- 向用户输出的内容必须是中文。
- 除非用户说明，否则不要直接恢复已删除文件。
- 你只允许在 `_Script/` 文件夹下对代码文件和文件夹进行添加或修改，`.meta` 由 Unity 自行编译即可，AI 无需手动改动。
