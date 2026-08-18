# 代码架构

## 一、文件结构一览

```text
Assets/
└── _Scripts/                        # 项目开发代码统一放在该目录下
    ├── Framework/                   # 通讯层（SPFramework 程序集）
    │   ├── Service/                 # 接口服务：ModuleServiceHub / InstanceServiceHub
    │   └── Event/                   # 事件广播：EventBus / EventKey
    │
    ├── Module/                      # 模块层
    │   ├── AI/                       # 敌人 AI 模块（Module.SPAI 程序集）
    │   ├── Camera/                  # 摄像机模块（Module.SPCamera 程序集）
    │   ├── Character/               # 角色模块（Module.SPCharacter 程序集）
    │   ├── Effects/                 # 空占位目录，尚无模块代码
    │   ├── Input/                   # 输入模块（Module.SPInput 程序集）
    │   ├── Resource/                # 资源加载模块（Module.SPResource 程序集）
    │   ├── Team/                    # 队伍模块（Module.SPTeam 程序集）
    │   └── UI/                      # 空占位目录，尚无模块代码
    │
    ├── Flow/                        # 编排层（SPFlow 程序集）
    │
    └── Tools/                       # 与业务无关的通用工具（SPTools 程序集）
```

本项目共分三层：

- **通讯层**：提供模块间的交流手段。模块与外部交流必须通过该层指定的两种方式——**接口服务** 和 **事件广播**，禁止绕过该层直接引用其他模块的实现。
- **模块层**：模块是本项目中最大的代码单位。每个模块对应一个独立程序集（`Module.SPXXX`），以程序集边界加访问级别约定共同构成模块间的硬性隔离。
- **编排层**：不含任何具体实现逻辑，仅负责模块的编排创建和纯协调性管理。

## 二、访问级别语义

本项目对 C# 访问级别有严格的语义约定，读写代码时必须按下表理解：

| 写法                                           | 本项目语义                                          |
| ---------------------------------------------- | --------------------------------------------------- |
| `public` 类型                                  | 项目级共享 API，改动需考虑跨模块影响                |
| `internal` 类型                                | 模块内部实现，外部不得引用                          |
| `internal` 类型中的 `public` 成员              | 仅模块内部公开，不是项目 API                        |
| `internal` 类型中的 `private`/`protected` 成员 | 类型自身实现或继承扩展点                            |
| `internal` 类型中的 `internal` 成员            | 禁止，重复语义，改为 `public`/`private`/`protected` |
| `public` 类型中的非 `public` 成员              | 内部实现或接线钩子，不作为跨模块 API                |

要点：

- `internal` 的作用域是程序集，即整个模块；因此 `internal` 类型内部的成员无需、也不允许再标注 `internal`。
- 全部 `public` 类型的集合即项目的对外 API 面，只应出现在通讯层与各模块的 Contract/ 中。
- 豁免：`Tools/` 下与业务无关、需挂载到场景的通用工具组件（如 `RuntimeFrameRateController` 这类 `public` MonoBehaviour）允许保持 `public`，其 `public` 是 Unity 组件挂载所需，不属于跨模块 API 面。

## 三、模块层

```text
Module/                            # 单个模块的目录结构（SPXXX 为模块名，如 SPCamera）
├── Module.SPXXX.asmdef            # 模块程序集定义：一个模块有且仅有一个程序集
├── Contract/                      # 对外契约：服务接口、事件标识、事件负载与共享数据类型，命名空间为 SPXXX.Contract
├── Core/                          # 核心实现：模块的全部业务逻辑，命名空间为 SPXXX.Core
└── Wiring/                        # 接线胶水：实现契约、注册服务、收发事件并转发给 Core，命名空间为 SPXXX.Wiring
```

`Contract/`、`Core/`、`Wiring/` 内允许按需再建子目录（如 `Contract/Event/`、`Contract/Data/`、`Core/Config/`、`Core/Editor/`、`Core/Expansion/`），命名空间仍归属所属层（如 `SPXXX.Contract`、`SPXXX.Core`），上述访问级别规则不变。

硬性规则：

1. Contract/ 下的类型一律 `public`；Core/ 与 Wiring/ 下的类型一律 `internal`。
2. 借助程序集隔离，其他模块在编译期只能看到本模块 Contract/ 中的 `public` 类型——Contract 即模块的唯一对外可见面。
3. 跨模块交流代码只能写在 Wiring/ 中，且必须作为 `internal` 类型依附于某一个具体模块。Wiring/ 只允许引用其他模块的 Contract 命名空间，禁止引用其 Core 与 Wiring（二者因 `internal` 在编译期本就不可见）。
4. Core/ 不感知其他模块的存在，不直接参与跨模块交流；一切对外能力均由 Wiring/ 实现 Contract 接口、再把调用转发给 Core。
5. 若一段跨模块协调逻辑不适合依附于任何单一模块，应上升至编排层，而不是随意挑选一个模块收容。

## 四、编排层

编排层（Flow/）承载"不属于任何单一模块"的跨模块业务流程，例如按固定顺序装配一支队伍、驱动一次牵涉多个模块的演出。

规则：

1. **只编排，不实现**：流程中需要的具体能力一律通过通讯层向模块借取（接口服务）或等待模块通知（事件广播），编排层自身不实现业务逻辑。
2. 编排层可以引用各模块的 Contract 命名空间；与 Wiring 同理，禁止触碰任何模块的内部实现。
3. 归属判据：一段协调逻辑若能明确归属于某个模块（由该模块主导，其他模块仅提供能力），应放在该模块的 Wiring/；只有当它同时牵涉多个模块且无天然归属时，才放入编排层。

## 五、通讯层

选用判据（先判语义，再选通道）：

- **接口服务 = 能力借用**：借用方明确认识被借用方，主动拿取接口来使用（请求-响应、状态查询）。
- **事件广播 = 事实通知**：发布方与订阅方互不认识。发布方仅发布已发生的离散事实，订阅方按需订阅；禁止把事件总线当作命令通道（即用事件驱使对方"去做某事"）。

### 接口服务

接口服务分两级，契约接口须继承对应的标记接口：

| 级别   | 标记接口          | 服务中心            | 注册键             | 适用场景                             |
| ------ | ----------------- | ------------------- | ------------------ | ------------------------------------ |
| 模块级 | `IModuleService`  | `ModuleServiceHub`  | 契约类型           | 全项目唯一的能力，如摄像机坐标转换   |
| 实例级 | `IInstanceService`| `InstanceServiceHub`| 契约类型 + 实例 id | 同类多实例的能力，按 id 寻址         |

生命周期纪律：注册与注销必须成对出现，两级服务统一在 `OnEnable` 中注册、在 `OnDisable` 中注销，注销时调用对应服务中心的 `Unregister`（模块级按契约类型寻址，实例级按契约类型 + 实例 id 寻址）。

服务中心的防御语义（写 Wiring 时需知晓）：

- 模块级 `Register`：同一契约已被其他存活实例注册时，输出 `LogWarning` 并以新实例覆盖。
- 模块级 `Unregister`：仅当当前注册的正是传入实例时才移除；契约未注册或实例不匹配时输出 `LogWarning` 并忽略本次注销。
- 实例级 `Register`：返回 `bool`；同一契约 + id 已被其他存活实例占用时输出 `LogError`、保留现有实例并返回 `false`（同实例重复注册或顶替已销毁实例视为成功，返回 `true`）。
- 实例级 `Unregister`：同样校验实例身份，仅身份匹配时移除并返回 `true`。

允许"依赖未接线则不注册"的条件注册惯例：Wiring 的 Core 依赖（如 `_main`、`_collector`）未在 Inspector 接好线时，`OnEnable` 可跳过注册、`OnDisable` 对应跳过注销，服务方法内部做空源降级；此时该契约对外表现为"服务未注册"，消费方按 `TryGet` 失败路径自行降级即可。

使用示例（摄像机模块，模块级服务）：

1）在 Contract/ 中定义契约（`public`，继承 `IModuleService`）：

```csharp
// Module/Camera/Contract/IConvertCameraTransform.cs
namespace SPCamera.Contract
{
    /// <summary>
    /// 转换行为 - 平面方向与摄像机坐标系相关联，产出世界 XZ 方向
    /// </summary>
    public interface IConvertCameraTransform : IModuleService
    {
        /// <summary>
        /// 将平面方向关联摄像机，产出世界 XZ 方向
        /// </summary>
        /// <param name="inputDirection">输入模块产出的平面方向</param>
        /// <returns>世界 XZ 方向</returns>
        Vector2 ConvertCameraTransform(Vector2 inputDirection);
    }
}
```

2）在 Wiring/ 中实现契约并注册（`internal`，调用转发给 Core 主入口；以下为节选，实际代码另含 `[DefaultExecutionOrder]` 执行顺序标注与 `_main` 未接线时的空源降级）：

```csharp
// Module/Camera/Wiring/CameraTransformWiring.cs
namespace SPCamera.Wiring
{
    /// <summary>
    /// 相机坐标转换接线胶水 - 实现坐标转换契约并注册到模块服务中心，调用转发给摄像机主入口
    /// </summary>
    internal sealed class CameraTransformWiring : MonoBehaviour, IConvertCameraTransform
    {
        [SerializeField] private SPCameraMain _main; // Core 主入口

        private void OnEnable()
            => ModuleServiceHub.Register<IConvertCameraTransform>(this);

        private void OnDisable()
            => ModuleServiceHub.Unregister<IConvertCameraTransform>(this);

        /// <inheritdoc />
        public Vector2 ConvertCameraTransform(Vector2 inputDirection)
            => _main.ConvertCameraTransform(inputDirection);
    }
}
```

3）消费方按需获取（仅引用 `SPCamera.Contract`，服务缺失时自行降级）：

```csharp
// Module/Character/Wiring/PlayerInputIntentionWiring.cs（节选）
Vector2 moveDirection = ModuleServiceHub.TryGet<IConvertCameraTransform>(out IConvertCameraTransform converter)
    ? converter.ConvertCameraTransform(inputDirection)
    : inputDirection;                                  // 服务未注册时降级为原始输入方向
```

要点：消费方拿取的只是 Contract 接口，对实现细节零感知；两级服务中心统一以 `TryGet` 获取服务，服务未注册或已销毁时返回 `false` 且 `out` 结果为 `null`，消费方需自行决定降级策略，不可默认服务必然可用。

### 事件广播

事件由两部分契约组成，全部定义在发布方模块的 Contract/ 中：

- **事件标识**：`EventKey<TPayload>`（引用类型的事件标识 class，非值类型），集中声明在 `XXXEvents` 静态类里，命名约定为 `"模块名.事件名"`；
- **事件负载**：`readonly struct`，以只读属性描述一条已发生的事实。

订阅时 `EventBus.Subscribe` 返回 `IDisposable` 句柄，订阅方必须持有并在失效时 `Dispose()`；同一处理函数重复订阅同一事件会抛异常。

使用示例（规范示意：假设摄像机模块需要广播"跟随目标已变化"这一事实；示例中的 `CameraEvents` 为规范示意，真实范例见 `Module/Character/Contract/Event/CharacterEvents.cs` 与 `Module/Team/Contract/Event/TeamEvents.cs`）：

1）在 Contract/ 中声明事件与负载（`public`）：

```csharp
// Module/Camera/Contract/CameraEvents.cs
namespace SPCamera.Contract
{
    /// <summary>
    /// 摄像机事件 - 由摄像机模块发布的事实广播
    /// </summary>
    public static class CameraEvents
    {
        /// <summary>跟随目标变化事件</summary>
        public static readonly EventKey<CameraFollowTargetChangedEvent> FollowTargetChanged =
            new EventKey<CameraFollowTargetChangedEvent>("Camera.FollowTargetChanged");
    }

    /// <summary>
    /// 跟随目标变化事件负载
    /// </summary>
    public readonly struct CameraFollowTargetChangedEvent
    {
        /// <summary>
        /// 创建跟随目标变化事件
        /// </summary>
        /// <param name="target">新的跟随目标</param>
        public CameraFollowTargetChangedEvent(Transform target) => Target = target;

        /// <summary>新的跟随目标</summary>
        public Transform Target { get; }
    }
}
```

2）发布方（摄像机模块）在事实发生后发布：

```csharp
EventBus.Publish(CameraEvents.FollowTargetChanged, new CameraFollowTargetChangedEvent(target));
```

3）订阅方（任意模块的 Wiring/）订阅并成对注销：

```csharp
private IDisposable _followChangedSubscription;

private void OnEnable()
    => _followChangedSubscription = EventBus.Subscribe(CameraEvents.FollowTargetChanged, OnFollowTargetChanged);

private void OnDisable()
{
    _followChangedSubscription?.Dispose();
    _followChangedSubscription = null;
}

private void OnFollowTargetChanged(CameraFollowTargetChangedEvent payload)
{
    // 按需响应事实
}
```

要点：发布方与订阅方唯一的共同依赖是 Contract 中的事件标识与负载类型，彼此互不知晓；订阅句柄必须成对 `Dispose`，避免泄漏与悬空回调。
