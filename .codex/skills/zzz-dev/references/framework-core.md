# 框架核心：访问级别与模块通讯

> 模块关系与通讯总则。涉及跨模块引用、命名空间、事件总线、模块服务接线时加载。

## 1. 访问级别语义

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

## 2. 两种模块通讯方式

### 能力/状态借用：Contract 接口 + 模块服务

- 用于：读取当前状态/连续数据、调用目标模块的明确能力。
- A 只依赖 B 的 Contract，不依赖 B 的 Core。
- 没有返回值不代表是事件：`SetCameraFollowTarget(target)` 仍是能力调用。

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

// 需方：在 Start 或之后取用，不判空
public sealed class ExampleConsumer : MonoBehaviour
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
- 事件名用事实（`ActiveCharacterChanged`），不用命令（`RequestSwitchCharacter`）；Payload 只放事实上下文。
- `EventBus.Clear()` 清空全部订阅，用于测试与运行复位。

## 3. 核心原则

1. 模块是最大代码单位；未拆 asmdef 不代表没有边界。
2. 跨模块只引用 `*.Contract` 与 `SPFramework.Service` 的 `ModuleServiceHub`；不引用 `Core`/`Debug`/`Editor`/`Wiring`。
3. 要用别人的东西，走接口 + 模块服务；只宣布自己做完了事，走事件总线。
4. 事件是事实不是命令；参数只补全事实（谁、前后变化、位置、时间、结果）。
