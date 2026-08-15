# 框架核心：访问级别与模块通讯

> 模块关系与通讯总则。涉及跨模块引用、命名空间、事件总线、模块服务接线时加载。

## 1. 跨模块引用位置约束（硬性约定）

跨模块交流统一由 **Wiring 胶水层** 承载；`Core` 层禁止引用任何外部模块命名空间（含其 `Contract`）以及框架 `SPFramework.Service` / `SPFramework.Event`。

| 层 | 允许引用的命名空间 | 职责 |
|---|---|---|
| `Contract` | 自身模块 + 框架事件/服务类型（`EventKey`、`IModuleService`、`IInstanceService`） | 定义对外 API 与事件，不含实现 |
| `Core` | 自身模块 + `UnityEngine` / 第三方库 | 纯实现与业务逻辑，零外部模块引用 |
| `Wiring` | 自身模块（`Contract` + `Core`）+ 外部模块 `Contract` + 框架 `SPFramework.Service` / `SPFramework.Event` | 服务注册/注销、事件订阅/转发、外部动作执行 |

Core 需要外部能力（资源实例化、其他模块服务、外部事实）时的做法：

1. Core 内定义**内部端口接口**，签名只使用自身/`UnityEngine` 类型，不得出现外部 Contract 类型。
2. Wiring 实现该端口（适配器），在实现内引用外部 `Contract` 并调用 Hub / `EventBus`。
3. Wiring 在 `Awake` 注入端口，Core 在 `Start` 使用（遵守"服务调用必须在 Start 或之后"）。
4. 订阅外部模块事件属于跨模块交流，订阅与转发必须位于 Wiring；处理逻辑留在 Core。

先例：Character 模块的 `ICCWiringExtension` —— Core 定义意图写入端口，`PlayerInputIntentionWiring` 在 Wiring 层读取输入并提交。

## 2. 访问级别语义

| 写法 | 本项目语义 |
|---|---|
| `public` 类型 | 项目级共享 API，改动需考虑跨模块影响 |
| `internal` 类型 | 模块内部实现，外部不得引用 |
| `internal` 类型中的 `public` 成员 | 仅模块内部公开，不是项目 API |
| `internal` 类型中的 `private`/`protected` 成员 | 类型自身实现或继承扩展点 |
| `internal` 类型中的 `internal` 成员 | 禁止，重复语义，改为 `public`/`private`/`protected` |
| `public` 类型中的非 `public` 成员 | 内部实现或接线钩子，不作为跨模块 API |

- `internal` 成员只用于 `public` 类型中限制接线/绑定入口，例如模块服务的注册入口。
- 不要为了跨模块调用把 `internal` 改成 `public`，应先设计正式入口。

## 3. 两种模块通讯方式

### 能力/状态借用：Contract 接口 + 服务

- 用于：读取当前状态/连续数据、调用目标模块的明确能力。
- A 只依赖 B 的 Contract，不依赖 B 的 Core。
- 没有返回值不代表是事件：`SetCameraFollowTarget(target)` 仍是能力调用。

服务契约用标记接口区分作用域：

| 契约形态 | 标记接口 | 注册/获取入口 | 键 |
|---|---|---|---|
| 模块级单例 | `IModuleService` | `ModuleServiceHub` | 契约类型 |
| 实例级 | `IInstanceService` | `InstanceServiceHub` | 契约类型 + 实例 id |
| 普通契约 | 无 | 不作为服务注册 | - |

#### 模块级服务（ModuleServiceHub）

时序约束：

1. 服务注册必须在 `Awake`（早于 `Start`）。
2. 服务调用必须在 `Start` 或之后，禁止在 `Awake` 取服务。
3. 满足以上两条时，`Get<I...>()` 在稳定运行窗口内可直接使用，不判空。

```csharp
// 供方：模块内 Wiring 在 Awake 注册
[DefaultExecutionOrder(-380)]
internal sealed class ExampleWiring : MonoBehaviour
{
    [SerializeField] private ExampleService _service; // 模块内 Core 实现

    private void Awake()
    {
        ModuleServiceHub.Register<IExampleContract>(_service);
    }

    private void OnDestroy()
    {
        ModuleServiceHub.Unregister<IExampleContract>();
    }
}

// 需方：取用服务属跨模块交流，须在 Wiring 层；在 Start 或之后取用，不判空
internal sealed class ExampleConsumerWiring : MonoBehaviour
{
    private void Start()
    {
        IExampleContract service = ModuleServiceHub.Get<IExampleContract>();
        service.DoWork();
    }
}
```

必选 vs 可选：

- 必选服务（如输入 `IProvideFrameInput`）：`Get` 直接用，不判空。
- 可选服务（如 `IConvertCameraTransform`）：`TryGet` 或判空做语义回退。

#### 实例级服务（InstanceServiceHub）

- 用于：同一契约存在多个实例，需要按实例路由（如按角色 id 请求切换）。
- 契约接口继承 `IInstanceService`，注册/获取都带实例 id。
- 实例 id 使用稳定字符串，当前单队伍下全局唯一即可。

生命周期与语义：

1. MonoBehaviour 实例服务在 `OnEnable` 注册、`OnDisable` 注销，由 `SetActive` 同步触发。
2. 注册/获取/注销必须使用同一个契约接口类型作泛型参数，键才一致；用实现类注册会与契约查询不匹配。
3. `Register` 遇到重复 id 报错并拒绝覆盖；`Unregister(id, instance)` 只注销该实例名下的条目，避免误删他人注册。
4. `Get<T>(id)` 未注册返回 `null`，`TryGet<T>(id, out service)` 返回 `false`，由调用方做“不可用/不可切换”回退，Hub 不抛异常。
5. 取用时自动清理已销毁的 Unity 对象，避免脏引用。

```csharp
// 契约：实例级服务接口
public interface IExampleInstanceContract : IInstanceService
{
    void DoWork();
}

// 供方：实例服务在激活时注册、失活时注销
internal sealed class ExampleInstanceService : MonoBehaviour, IExampleInstanceContract
{
    [SerializeField, Tooltip("实例 id")] private string _instanceId;

    private void OnEnable()
    {
        InstanceServiceHub.Register<IExampleInstanceContract>(_instanceId, this);
    }

    private void OnDisable()
    {
        InstanceServiceHub.Unregister<IExampleInstanceContract>(_instanceId, this);
    }
}

// 需方：取用服务属跨模块交流，须在 Wiring 层；在 Start 或之后按 id 取用
internal sealed class ExampleConsumerWiring : MonoBehaviour
{
    [SerializeField, Tooltip("目标实例 id")] private string _targetId;

    private void Start()
    {
        if (InstanceServiceHub.TryGet<IExampleInstanceContract>(_targetId, out IExampleInstanceContract service))
            service.DoWork();
    }
}
```

### 事实广播：事件总线

- 用于：声明“某件事已发生”，低频、离散、完成语义或状态变化。
- 发布者只声明事实，订阅者自行响应，总线只负责分发；无订阅者时发布方仍成立。

用法（命名空间 `SPFramework.Event`）：

```csharp
// 事件定义放目标模块 Contract/Events
public static class ExampleEvents
{
    public static readonly EventKey<ExampleChangedEvent> ExampleChanged =
        new EventKey<ExampleChangedEvent>("Example.State.ExampleChanged");
}

public readonly struct ExampleChangedEvent { /* 只放事实上下文 */ }

// 发布：状态提交后
EventBus.Publish(ExampleEvents.ExampleChanged, new ExampleChangedEvent(...));

// 订阅：返回 IDisposable，生命周期结束退订
IDisposable subscription = EventBus.Subscribe(ExampleEvents.ExampleChanged, OnChanged);
```

- `OnEnable` 订阅必须在 `OnDisable` 退订；纯 C# 类在 `Dispose` 退订。
- 订阅外部模块事件属跨模块交流，须位于 Wiring 层；发布自身 Contract 事件可在 Core。
- 事件名用事实（`ActiveCharacterChanged`），不用命令（`RequestSwitchCharacter`）；Payload 只放事实上下文。
- `EventBus.Clear()` 清空全部订阅，用于测试与运行复位。

## 4. 核心原则

1. 模块是最大代码单位；未拆 asmdef 不代表没有边界。
2. 跨模块引用只发生在 Wiring 层：`Core` 禁止引用外部模块命名空间（含其 `Contract`）与框架 `SPFramework.Service`/`SPFramework.Event`；Wiring 跨模块只引用 `*.Contract` 与 `SPFramework.Service`/`SPFramework.Event`；任何层都不引用其他模块的 `Core`/`Debug`/`Editor`/`Wiring`。
3. 要用别人的东西，走接口 + 服务（模块级或实例级）；只宣布自己做完了事，走事件总线。
4. 事件是事实不是命令；参数只补全事实（谁、前后变化、位置、时间、结果）。
