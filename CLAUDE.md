# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

复刻绝区零（Zenless Zone Zero）战斗系统的 Unity 3D 项目。使用 URP 渲染管线、New Input System、Cinemachine 摄像机系统、Behavior Designer 行为树、Animancer 动画状态机

- **Unity**: 2022.3.62f3
- **渲染管线**: URP 14.0.12
- **输入系统**: New Input System 1.14.2
- **摄像机**: Cinemachine 2.10.7
- **AI**: Behavior Designer (第三方插件)
- **状态机**: Animancer (第三方插件)
- **解决方案文件**: `Combat System.sln`

## Commands

本项目为标准 Unity 项目，无额外构建/测试 CLI。

- 在 Unity Editor 中打开项目：通过 Unity Hub 打开项目根目录
- 使用 VS Code 打开：`code "Combat System.code-workspace"`（位于 `WorkSpace/`）
- 运行测试：通过 Unity Test Runner 窗口（Window → General → Test Runner）
- 查看事件通道调试：Play Mode 下 Window → Event Debugger

## Code Conventions

- **私有字段**: `_camelCase`（下划线前缀），如 `_currentState`
- **其它所有**: PascalCase（局部变量、类名、方法、属性、常量、枚举、事件）
- **接口**: `I` 前缀，泛型 `T` 前缀，异步方法 `Async` 后缀
- **命名空间**: 从 `_Scripts` 目录开始算，如 `Core.Input`、`GamePlay.Combat`
- **Inspector 字段**: 必须带 `[Tooltip]`
- **类**: 写 `<summary>` 说明职责
- **公有/保护方法**: 三行 XML 注释（`<summary>` + `<param>` 每个参数 + `<returns>`）
- **接口方法/属性**: 单行 XML 注释

## Architecture

代码位于 `Assets/_Scripts/`，分为两大命名空间：

### Core（框架层，不依赖 GamePlay）

**Input Pipeline（`Core.Input`）— 分层管道架构**

数据流：`IInputSource` → `InputCollector` → `MainProcessorPipeline` → `IntentionBlackboard` → 下游读取

1. **`IInputSource`** — 输入源接口，所有输入来源（玩家/AI/调试）通过 `FetchRawInput(ref RawInputData)` 采样原始硬件数据
2. **`PlayerInputReader`** — MonoBehaviour 实现 `IInputSource`，通过 `InputActionReference` 从 New Input System 读取 WASD/攻击/闪避
3. **`InputCollector`** — 纯 C# 类，每帧驱动 IInputSource 采样 → 后处理（Move 防抖 + Attack/Evade 按键 BufferTimer 缓存）→ 对外暴露 `Current`/`LastFrameData`。提供 `ConsumeAttackPressed()` / `ConsumeEvadePressed()` 显式核销接口
4. **`MainProcessorPipeline`** — 遍历 `IIntentProcessor` 列表，将处理后的输入翻译为离散意图写入 `IntentionBlackboard`
5. **`IntentionBlackboard`** — POCO 黑板，暴露 `MoveDirection`、`WantToAttack`、`WantToEvade` 属性。每帧由 Pipeline 覆盖写入，不跨帧残留
6. **`IIntentProcessor`** — 意图处理器接口（Move/Attack/Evade 各一个实现），Attack 和 Evade 处理器写入黑板后调用 Consume 防止同帧多重消费
7. **`InputPostProcessConfig`** — 静态配置常量（防抖窗口 0.05s、Buffer 窗口 0.2s）

关键数据结构的流转：
- `RawInputData`（struct，栈上）→ `ProcessedInputData`（struct，防抖+Buffer后）→ `FrameInputData`（struct，单帧完整快照，含 FrameIndex 和时间）
- `InputData`（class，堆上）持有 `CurrentFrameData` 和 `LastFrameData` 双缓冲

**Event System（`Core.Event`）— ScriptableObject 解耦事件**

- **`EventChannelSO`** — 抽象基类，继承 `ScriptableObject`，OnEnable/OnDisable 时自动向 `EventChannelRegistry` 注册/注销
- **`VoidEventChannelSO`** / **`FloatEventChannelSO`** / **`TransformEventChannelSO`** — 具体通道类型，通过 C# event 实现订阅/触发
- **`EventChannelRegistry`** — 静态 `HashSet<EventChannelSO>` 注册表，供 `EventDebugWindow` 编辑器窗口遍历显示
- 典型用法：在 `Assets/Data/EventSO/` 创建 .asset 实例（如 `CameraShake_Channel`），生产者 `Raise()`，消费者在 OnEnable 中 `Subscribe()`

**Object Pool（`Core.Pool`）**

- **`IPoolable`** — 对象需实现的接口（`PoolName` + `OnSpawn()` + `OnDespawn()`）
- **`Pool<T>`** — 泛型池，Queue 实现，支持预热（prewarm）
- **`PoolManager`** — 懒汉单例，string-keyed `Dictionary<string, IPool>`，`Get<T>(poolName)` / `Recycle(obj)`
- **`PoolRegistrar`** — 场景中挂载的 MonoBehaviour，Awake 时创建层级容器并注册池。目前仅有 `FX_Slash` 池

### GamePlay（游戏逻辑层，依赖 Core）

**Attribute（`GamePlay.Attribute`）**

- **`AttributeType`** enum：MaxHealth、Attack、Defense、MoveSpeed
- **`CharacterAttributeSO`** — ScriptableObject，定义角色初始属性值
- **`CharacterAttributes`** — 运行时容器，从 SO 加载，实现 `IAttributeProvider` 只读接口
- **`IAttributeProvider`** — 唯一属性读取接口，供战斗系统等外部模块通过 `GetAttribute(AttributeType)` 查询

**Combat System（`GamePlay.Combat`）**

核心概念：**攻击由 Animator 归一化时间驱动**，基于 `AttackComboConfigSO` 配置判定窗口和特效触发时刻。

- **`AttackComboConfigSO`** — ScriptableObject，包含 `AttackSegmentConfig[]`。每段配置包含 `AnimationHash`、`HitWindow[]`（`Start/EndNormalizedTime` + `ShakeForce`）、`EffectSpawnInfo[]`（`NormalizedTime` + `LocalPosition/Rotation`）
- **`AttackHitbox`** — MonoBehaviour，Trigger 碰撞体，激活周期内通过 `HashSet<IDamageable>` 去重（同一目标只命中一次），排除自身（root 比较）
- **`HitboxService`** — **静态服务类**，每帧被调用，根据归一化时间维护碰撞体启停、从 `IAttributeProvider` 读取攻击力、通过 `FloatEventChannelSO` 触发震屏
- **`EffectService`** — **静态服务类**，每帧被调用，根据归一化时间从 PoolManager 取出 `SlashEffect` 放置在挂点指定位置
- **`IDamageable`** — 受击接口（`Transform` + `TakeDamage(DamageInfo)`），由 EnemyController 实现
- **`DamageInfo`** — struct（Amount、HitPoint、Source）

**Player（`GamePlay.Player`）**

- **`PlayerController`** — 玩家入口，`RequireComponent CharacterController + Animator`。Awake 中完成依赖注入（`InputCollector` ← `PlayerInputReader`，`MainProcessorPipeline` ← Collector + Blackboard），Update 中驱动采集→翻译管线。对外暴露 `IntentionBlackboard` 供下游系统读取。

**Enemy（`GamePlay.Enemy`）**

- **`EnemyController`** — 实现 `IDamageable`，通过 `CharacterAttributeSO` 初始化属性，`isHitRequested` 标记供行为树使用。受击时设置 Animator Hit Trigger
- **`BT/IsHitRequested`** — Behavior Designer Conditional 任务，检测 `isHitRequested`
- **`BT/PlayHitAnimation`** — Behavior Designer Action 任务，播放 Hit_Front 动画并等待 `normalizedTime >= 0.9f` 后清除标记

**Cameras（`GamePlay.CustomCameras`）**

- **`CameraLockEnemy`** — 锁敌系统，通过 `VoidEventChannelSO` 接收切换事件，SphereCast 搜索最近 Enemy 标签目标，冻结 CinemachinePOV 水平轴并平滑追踪。通过 `TransformEventChannelSO` 广播目标变化
- **`CameraShakeHandler`** — 订阅 `FloatEventChannelSO`，收到震屏力度时通过 `CinemachineImpulseSource.GenerateImpulseWithForce()` 触发
- **`CameraOrbitDistance`** — 根据 POV 垂直角度在两段之间 lerp 摄像机距离，实现环视时自动缩放

**Effects（`GamePlay.Effects`）**

- **`SlashEffect`** — 实现 `IPoolable`，OnSpawn 时播放 ParticleSystem 并启动协程，粒子播完后自动 `PoolManager.Instance.Recycle(this)`

### Key Design Patterns

- **输入缓冲 + Consume 模式**：按键按下后 buffer timer 开始衰减，处理器检测到后立即 Consume 归零，防止同一按键被多帧消耗
- **归一化时间驱动**：HitboxService 和 EffectService 均基于 Animator 归一化时间（0~1）驱动，配置与动画帧精确绑定
- **静态服务类**：HitboxService 和 EffectService 是无状态的纯逻辑静态方法，由调用方维护 `ref` 状态（currentWindowIndex、hitboxEnabled、currentSpawnIndex）
- **SO 事件通道**：跨系统通信（如攻击→震屏）通过 ScriptableObject 事件通道解耦，无需组件引用
- **对象池**：频繁创建/销毁的特效（SlashEffect）通过 PoolManager 管理生命周期
- **只读接口隔离**：`IAttributeProvider` 只暴露查询，不暴露修改；`IPoolable` 只定义生命周期钩子

## Notes

- 向用户输出的内容必须是中文，且每次输出都需要称用户为 **"SP酱"**，用户将据此判断 AI 是否正确运行。
- 除非我说明，否则不要直接上手 Editor 侧操作。
