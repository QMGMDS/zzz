# 事件总线 API 速览

> 适用场景：跨模块广播低频、离散、已发生的事实事件
> 边界约定：事件基础设施位于 `SPEvent`，各模块事件 Key 和 Payload 应放在本模块 `Contract/Events` 下
> 核心原则：发布者只声明事实，订阅者自行响应，事件总线只负责分发

## 一、核心 API

### 对外公开的命名空间

```csharp
using SPEvent;
```

### 事件标识：EventKey<TPayload>

```csharp
public sealed class EventKey<TPayload>
```

- `EventKey<TPayload>` 是事件标识，不是委托
- `TPayload` 是该事件携带的消息体类型
- `Name` 只用于调试、日志和错误提示
- 不建议使用多个泛型参数，多字段事件统一封装成一个 Payload 类型

### 全局事件总线：EventBus.Global

```csharp
EventBus.Global.Subscribe(eventKey, handler);
EventBus.Global.Publish(eventKey, payload);
```

- 项目约定事件总线全局唯一
- `Subscribe` 返回 `IDisposable`，调用 `Dispose()` 即退订
- 没有订阅者时，`Publish` 会直接返回，发布方主逻辑不应依赖订阅者存在
- 回调抛异常会被记录，不应在事件回调里依赖异常控制主流程

## 二、事件定义方式

事件定义放在事件所属模块的契约层，例如：

```text
Assets/_Scripts/<Module>/Contract/Events/
```

推荐结构：

```csharp
using SPEvent;

namespace SPExample.Contract.Events
{
    public static class ExampleEvents
    {
        public static readonly EventKey<ExampleChangedEvent> ExampleChanged =
            new EventKey<ExampleChangedEvent>("Example.State.ExampleChanged");
    }

    public readonly struct ExampleChangedEvent
    {
        public readonly int EntityId;
        public readonly float OldValue;
        public readonly float NewValue;

        public ExampleChangedEvent(int entityId, float oldValue, float newValue)
        {
            EntityId = entityId;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
```

Payload 只描述事实上下文：谁、前后变化、原因、位置、时间、结果等。不要塞入“要别人执行什么”的命令意图。

## 三、发布方式

由状态变化源头发布事件，并且应在状态提交后发布：

```csharp
_currentValue = newValue;

EventBus.Global.Publish(
    ExampleEvents.ExampleChanged,
    new ExampleChangedEvent(entityId, oldValue, newValue));
```

发布方不关心谁订阅、订阅者做了什么、是否有人处理。

## 四、订阅方式

### MonoBehaviour 生命周期

```csharp
private IDisposable _exampleChangedSubscription;

private void OnEnable()
{
    _exampleChangedSubscription = EventBus.Global.Subscribe(
        ExampleEvents.ExampleChanged,
        OnExampleChanged);
}

private void OnDisable()
{
    _exampleChangedSubscription?.Dispose();
    _exampleChangedSubscription = null;
}

private void OnExampleChanged(ExampleChangedEvent eventData)
{
    // 响应已发生的事实
}
```

### 纯 C# 类生命周期

```csharp
private IDisposable _exampleChangedSubscription;

public void Initialize()
{
    _exampleChangedSubscription = EventBus.Global.Subscribe(
        ExampleEvents.ExampleChanged,
        OnExampleChanged);
}

public void Dispose()
{
    _exampleChangedSubscription?.Dispose();
    _exampleChangedSubscription = null;
}
```

- `OnEnable` 订阅必须在 `OnDisable` 退订
- `Initialize` 订阅必须在 `Dispose` 退订
- 不要重复订阅同一个事件和同一个 handler

## 五、什么时候使用

| 情况 | 是否使用事件总线 |
|---|---|
| 只声明某件事已经发生 | 使用 |
| 无订阅者时发布方仍可正常运行 | 使用 |
| 低频、离散、完成语义或状态变化 | 使用 |
| 需要读取当前状态或连续数据 | 不使用，走 Contract 接口 + Provider SO |
| 需要目标模块执行明确能力 | 不使用，走 Contract 接口 + Provider SO |
| 需要返回值、成功失败或顺序编排 | 不使用，走协调层 |
| 每帧输入、移动方向、持续状态 | 不使用 |

## 六、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| 用事件名表达命令，如 `RequestSwitchCharacter` | 用事实名，如 `ActiveCharacterChanged` | 事件总线广播事实，不下命令 |
| Payload 里放目标设置、播放表现、生成对象参数 | Payload 只放事实上下文 | 避免把事件变成隐式任务书 |
| 订阅后不退订 | 保存 `IDisposable` 并在生命周期结束时 `Dispose` | 避免悬挂回调和重复订阅 |
| 发布前状态尚未提交 | 先提交状态，再 `Publish` | 订阅者读取状态时应看到稳定结果 |
| 用事件传递每帧输入 | 下游 Pull `IProvideFrameInput.CurrentProcessed` | 高频连续数据不适合事件总线 |
| 订阅者反查发布者补数据 | Payload 补足必要事实上下文 | 避免重新耦合发布方和订阅方 |

## 七、交叉引用

| 相关文档 | 用途 |
|---|---|
| [project-module-boundaries.md](project-module-boundaries.md) | 判断事件总线、接口 + SO 信箱、协调层的使用边界 |
| [input-module-api.md](input-module-api.md) | 每帧输入为什么不走事件总线 |
| [character-module-api.md](character-module-api.md) | 角色模块对外契约和内部 Core 边界 |
