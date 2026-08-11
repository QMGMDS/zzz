# 项目模块边界与通信规则

> **适用场景**：设计或审查跨模块引用、`public` / `internal` 暴露、Provider SO、事件总线与协调层。本文是模块关系总则。

## 0. 核心原则

本项目以模块作为最大代码单位。未拆 asmdef 不代表没有边界；AI 不能只按 C# 编译器可见性判断能否引用。

模块关系分两层：**静态边界**决定“能不能引用”，**动态通信**决定“该怎么交流”。跨模块只能走目标模块正式入口，不直接引用 `Core` / `Debug` / `Editor` / `internal Wiring`。

一句话：**要用别人的东西，走接口和 SO 信箱；只宣布自己做完了事，走事件总线。**

## 1. 模块判定与正式入口

模块通常以 `Assets/_Scripts/<Module>` 目录和主命名空间划分，例如输入 `SPInput`、摄像机 `SPCamera`、角色 `SPCharacter`。

跨模块默认只允许引用：

- `*.Contract`：稳定接口、只读数据结构、事件契约。
- `*.Wiring` 中 `public` 的 Provider SO / 槽位资产 / 对外接线入口。

默认禁止引用：`*.Core`、`*.Debug`、`*.Editor` / `*.Core.Editor`、`*.Wiring` 中 `internal` 胶水。

## 2. 访问级别语义

| 写法 | 本项目语义 |
|---|---|
| `public` 类型 | 项目级共享 API，修改前要考虑跨模块影响。 |
| `internal` 类型 | 模块内部实现；同程序集可访问也不代表外部可引用。 |
| `internal` 类型中的 `public` 成员 | 仅模块内部公开，不是项目 API。 |
| `internal` 类型中的 `private` / `protected` 成员 | 类型自身实现或继承扩展点。 |
| `internal` 类型中的 `internal` 成员 | 禁止使用；这是重复语义，应改成 `public` / `private` / `protected` 表达真实使用面。 |
| `public` 类型中的非 `public` 成员 | 内部实现或接线钩子，不作为跨模块 API。 |

`internal` 类型已经完成模块内部边界表达，成员层不要再写 `internal`。若成员要给该类型的模块内协作者调用，写 `public`；若只给本类型使用，写 `private`；若是继承扩展点，写 `protected`。`internal` 成员主要用于 `public` 类型中限制接线或绑定入口，例如 Provider SO 的 `Bind`。

不要为了跨模块调用直接把 `internal` 类型改成 `public`；应先设计正式入口。

## 3. 通信方式一：能力/状态借用

当 A 需要使用 B 的数据、状态、计算结果或明确能力时，走 **Contract 接口 + Provider SO 信箱**。A 只依赖 B 的契约，不依赖 B 的 Core。

典型场景：读取当前帧输入、读取当前状态、转换相机系移动方向、设置摄像机跟随目标。Provider SO 是运行时信箱，不是业务逻辑容器；`Provider` 可能为空，调用方必须判空。

注意：**没有返回值不代表是事件**。`SetCameraFollowTarget(target)` 仍是明确能力调用。

## 4. 通信方式二：事实广播

当 A 只声明“某件事已经发生”，且不关心谁处理、是否处理、处理结果时，走 **事件总线**。事件是事实，不是命令；没有订阅者时，发布方主逻辑仍应成立。

事件适合低频、离散、完成语义或状态变化，如 `ActiveCharacterChanged`、`CharacterDead`、`SkillCastCompleted`。避免命令式事件名，如 `NotifyCameraToShake`、`RequestSwitchCharacter`、`SetCameraFollowTargetRequested`。

事件可以带参数，但参数只能补全事实：谁、前后变化、位置、时间、原因、结果、上下文；不能夹带设置目标、播放表现、生成对象、切换状态等命令意图。

## 5. 协调层边界

事件总线不负责流程编排。若业务流程要求多个模块按顺序协作，或主流程依赖多个模块的执行结果，应使用协调层 / 编排层。协调层可以监听事实事件，再调用 Provider 完成确定动作。

## 6. 快速决策表

| 情况 | 用法 |
|---|---|
| 当前状态、连续数据、可查询数据 | 接口 + SO 信箱 |
| 目标模块执行明确能力 | 接口 + SO 信箱 |
| 只声明事实已发生 | 事件总线 |
| 无订阅者时发布方仍成立 | 事件总线 |
| 多模块按序协作或汇总结果 | 协调层 |
| 参数像任务书、命令、目标设置 | 不要用普通事件 |
| 每帧输入、连续状态 | 不要做成事件 |

## 7. AI 审查清单

1. 是否跨模块引用 `Core` / `Debug` / `Editor` / `internal Wiring`？
2. 是否把 `internal` 类型或其 `public` 成员当项目 API？`internal` 类型内是否误写了 `internal` 成员？
3. 是否该新增 `Contract` / Provider SO，而不是暴露 Core？
4. 真实意图是“借能力/状态”，还是“广播事实”？
5. 是否需要返回值、成功失败或确定副作用？需要则不用普通事件。
6. 事件名是否像事实，而不是命令？
7. 事件参数是在描述事实，还是夹带执行要求？
8. 多模块流程是否需要协调层？
