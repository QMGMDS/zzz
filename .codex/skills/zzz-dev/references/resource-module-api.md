# 资源加载模块 API

## 一、模块概述

资源加载模块（`Module.SPResource` 程序集）对外提供一项能力：根据资源键同步实例化预制体。能力以**模块级服务**形式注册到 `ModuleServiceHub`，契约定义在 `SPResource.Contract` 命名空间：

| 契约接口 | 能力 |
| --- | --- |
| `IInstantiateResource` | 根据资源键同步实例化预制体，产出实例与释放委托 |

边界约定：外部只允许引用 `SPResource.Contract`，通过 `ModuleServiceHub.TryGet` 获取服务；`SPResource.Core` 与 `SPResource.Wiring` 中的类型均为 `internal`，编译期对外不可见。

失败语义：实例化不抛异常、不写日志，成功与否与失败原因全部由 `ResourceInstantiateResult` 承载。

### IInstantiateResource

```csharp
public interface IInstantiateResource : IModuleService
{
    ResourceInstantiateResult Instantiate(ResourceKey key, Transform parent = null, bool activate = true);
    ResourceInstantiateResult Instantiate(ResourceKey key, Vector3 worldPosition, Quaternion worldRotation, Transform parent = null, bool activate = true);
}
```

- 第一个重载：保持预制体自身姿态创建实例（预制体的局部位置与旋转原样保留）；`parent` 留空时实例位于场景根。
- 第二个重载：以指定世界位姿创建实例。
- `activate` 传 `false` 时实例创建后即处于未激活状态，激活时机由调用方控制。

### ResourceKey

```csharp
[Serializable]
public struct ResourceKey : IEquatable<ResourceKey>
{
    public string Value { get; }
    public bool IsValid { get; }
    public ResourceKey(string value);
}
```

- 从字符串构造，如 `new ResourceKey("Prop.Chest")`。
- 键与资源目录条目按字符串精确匹配，区分大小写。
- 空键不会抛异常，实例化返回 `InvalidKey` 失败结果。

### ResourceInstantiateResult

```csharp
public readonly struct ResourceInstantiateResult
{
    public GameObject Instance { get; }
    public ResourceInstantiateError Error { get; }
    public Action Release { get; }
    public bool IsSuccess { get; }
}
```

- 成功时：`Instance` 为产出的实例，`Error` 为 `None`，`Release` 为释放委托。
- 失败时：`Instance` 与 `Release` 均为 `null`，读 `Error` 获得失败原因。
- `Release` 用于销毁该实例，重复调用安全；实例若已被其他途径销毁，`Release` 静默跳过。
- 结果只能由模块签发，构造入口对外不可见，消费方只读。

### ResourceInstantiateError

```csharp
public enum ResourceInstantiateError
{
    None,                // 无错误 - 实例化成功
    InvalidKey,          // 资源键为空
    KeyNotFound,         // 资源目录中不存在该资源键
    InstantiateFailed,   // 实例化过程发生异常
    ServiceUnavailable,  // 资源主入口未接线或已销毁
}
```

## 二、API 调用示例

获取服务统一使用 `ModuleServiceHub.TryGet`。服务未注册或已销毁时返回 `false` 且 `out` 结果为 `null`，调用方必须自行降级，不可默认服务必然可用。

### 实例化并持有释放委托

```csharp
using SPFramework.Service;
using SPResource.Contract;

if (ModuleServiceHub.TryGet<IInstantiateResource>(out IInstantiateResource resource))
{
    ResourceInstantiateResult result = resource.Instantiate(new ResourceKey("Prop.Chest"), parent);
    if (result.IsSuccess)
    {
        GameObject instance = result.Instance;
        Action release = result.Release; // 持有以备销毁
    }
}
// 服务缺失时跳过本次实例化
```

### 以指定世界位姿实例化，并保持未激活

```csharp
using SPFramework.Service;
using SPResource.Contract;

if (ModuleServiceHub.TryGet<IInstantiateResource>(out IInstantiateResource resource))
{
    ResourceInstantiateResult result = resource.Instantiate(
        new ResourceKey("Prop.Chest"), spawnPosition, spawnRotation, parent, activate: false);
    // 之后由调用方自行决定何时 SetActive(true)
}
```

### 按失败原因分支处理

```csharp
if (!result.IsSuccess)
{
    if (result.Error == ResourceInstantiateError.KeyNotFound)
    {
        // 目录中不存在该键，按需降级
    }
    // 其余原因（InvalidKey / InstantiateFailed / ServiceUnavailable）同理可按需区分
}
```

### 销毁实例

```csharp
release?.Invoke(); // 重复调用安全，实例已销毁时静默跳过
```

## 三、反例

| 反例 | 正确做法 |
| --- | --- |
| 引用 `SPResource.Core` / `SPResource.Wiring` 中的类型 | 只引用 `SPResource.Contract`，经 `ModuleServiceHub` 获取接口 |
| 绕过模块自行持有预制体引用调用 `Instantiate`，或用 `Resources.Load` 等自搭加载路径 | 借用 `IInstantiateResource` 按资源键实例化 |
| 不判空直接调用服务，默认其必然可用 | 用 `TryGet` 获取，失败时按上文示例降级 |
| 长期缓存服务接口并假设其永久有效 | 服务随模块内部的注册/注销生命周期变动，每次按需 `TryGet` |
| 不判 `IsSuccess` 就使用 `Instance` 或 `Release`（失败时二者均为 `null`） | 先判 `IsSuccess`，需要区分原因时按 `Error` 枚举分支 |
| 用 `Object.Destroy` 直接销毁模块产出的实例 | 调用结果的 `Release` 委托 |
| 试图自行构造 `ResourceInstantiateResult` | 结果只能由模块签发，消费方只读 |
