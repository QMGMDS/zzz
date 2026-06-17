# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

复刻《绝区零》（Zenless Zone Zero）战斗系统的 Unity 3D 项目。项目使用 URP 渲染管线、New Input System、Cinemachine、Behavior Designer 与 Animancer。

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

### 代码目录

- `Assets/_Scripts/Player` — 玩家角色控制器核心代码。
- `Assets/_Scripts/UI` — UI 相关脚本（目前暂无核心逻辑）。
- `Assets/Settings/Input` — New Input System 的 `.inputactions` 资产。
- `Assets/Scenes/SampleScene.unity` — 主要场景。

### 玩家角色控制器的数据流

`PlayerController`（MonoBehaviour）是入口，使用 `[DefaultExecutionOrder(-300)]` 保证在大多数系统之前执行，每帧按固定管线推进：

```
InputSource（采样原始输入）
    ↓
InputCollector（后处理：防抖 + BufferTimer）
    ↓
InputMainProcessor（驱动 IInputProcessor 子处理器翻译意图）
    ↓
PlayerBrain（运行时数据黑板）
    ↓
StateMachine.CurrentState.LogicUpdate()
    ↓
BaseState.CheckInterrupts() → MainInterceptor（全局拦截器管线）
    ↓
BaseState.UpdateStateLogic()
    ↓
AnimationDriver.Update()（监听黑板状态变化 -> 播放动画）
    ↓
BaseState.PhysicsUpdate()（OnAnimatorMove 中执行，应用动画根运动）
    ↓
PlayerBrain.ResetInputBrain()（LateUpdate 清除输入意图）
```

关键子系统职责：

- **`PlayerController`**：装配子系统、严格时序分发，不写具体游戏逻辑。
- **`PlayerBrain`**：所有模块共享的数据黑板。输入意图（`WantToAttack`、`WantToEvade`）、当前逻辑状态 `CurrentPlayerState`、动画进度 `CurrentNormalizedTime` / `AnimationCompleted` 均在此。
- **`InputSource`**：通过 `InputActionReference` 采样原始设备输入。
- **`InputCollector`**：维护当前帧/上一帧输入快照，提供 Move 防抖与 Attack/Evade 缓存窗口，并支持 `ConsumeAttackPressed()` / `ConsumeEvadePressed()` 显式核销。
- **`InputMainProcessor`**：`IInputProcessor` 子处理器的工厂与驱动器，将处理后的输入翻译为意图写入黑板。
- **`StateMachine` / `BaseState`**：纯状态生命周期管理。`BaseState.LogicUpdate()` 先执行全局拦截，再执行状态逻辑。`BaseState` 不写动画播放代码。
- **`MainInterceptor` / `StateInterceptorSO`**：全局可配置的状态转移拦截管线，按数组顺序决定优先级。每个拦截器可维护豁免清单。
- **`AnimationDriver` / `StateToAnimationAdapter` / `AnimationSource`**：动画表现层。`AnimationDriver` 监听黑板中的 `CurrentPlayerState`，通过 SO 配置映射到 `AnimationStateConfig`，再经 `AnimationSource` 调用 Animancer 播放，并把归一化时间与完成标记回写黑板。

### ScriptableObject 配置约定

- 动画配置：`PlayerAnimationConfigSO`，位于 `Assets/_Scripts/Player/Animation/Config/`。
- 拦截器：`PlayerInterceptorConfigSO`，位于 `Assets\_Scripts\Player\StateLogic\Interceptor\Config`。

## 编码规范

### 命名

- **私有字段**：`_camelCase`（下划线前缀），例如 `_currentState`。
- **其它所有**：`PascalCase`（局部变量、类名、方法、属性、常量、枚举、事件）。
- **接口**：`I` 前缀；泛型：`T` 前缀；异步方法：`Async` 后缀。

### 注释

注释要求简明扼要，不引用脚本名

- **类**：提供 `<summary>`，说明职责。
- **公有/保护方法**：三行 XML 注释（`<summary>` + 每个参数的 `<param>` + `<returns>`）。
- **接口方法/属性**：单行 XML 注释。

- **继承过来的 接口方法/父类方法**：`/// <inheritdoc />` 指明该方法是从父类或者接口继承而来的即可。

### 其它

- Inspector 字段必须带 `[Tooltip]`。
- 新增状态枚举时，先在 `PlayerStateType` 中添加值。

## 交互约定

- 向用户输出的内容必须是中文。
- 除非用户说明，否则不要直接恢复已删除文件。
- 你只允许在 `_Script/` 文件夹下对 .cs 文件进行编写或文件夹的添加修改，`.meta` 由 Unity 自行编译，AI 无需改动。
