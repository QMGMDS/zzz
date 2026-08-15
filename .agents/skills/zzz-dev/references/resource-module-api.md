# 资源模块 API 速览

> 适用场景：按资源键同步实例化预制体、批量创建实例、统一管理实例的创建与释放。
> 边界约定：外部只允许引用 `SPResource.Contract`，通过 `ModuleServiceHub.Get<IInstantiateResource>()` 获取资源服务；不要直接引入 `SPResource.Core` / `SPResource.Wiring`。
> 使用原则：外部通过 `ModuleServiceHub.Get<IInstantiateResource>()` 取服务，同步调用拿结果；实例释放统一走 `IResourceHandle.Release()`。

## 一、核心 API

### 对外公开的命名空间

```csharp
using SPFramework.Service;
using SPResource.Contract;
```

- `Contract`：稳定的能力接口与只读数据结构。
- `Wiring`：模块内接线胶水，负责把 `ResourceLoadService` 注册到 `ModuleServiceHub`，外部禁依赖。

### 能力接口：IInstantiateResource

```csharp
public interface IInstantiateResource
{
    ResourceLoadResult Instantiate(ResourceLoadRequest request);
    IReadOnlyList<ResourceLoadResult> InstantiateBatch(IReadOnlyList<ResourceLoadRequest> requests);
}
```

- 这是**同步**实例化能力，不是异步资源/Addressables 下载。
- `Instantiate`：按请求创建一个预制体实例。
- `InstantiateBatch`：按请求列表批量创建，结果顺序与请求一致；传入 `null` 返回空数组。

### 资源键：ResourceKey

```csharp
public struct ResourceKey : IEquatable<ResourceKey>
{
    public string Value { get; }
    public bool IsValid { get; }
}
```

- `Value` 是资源定位键字符串，需要与目录中的 Key **完全一致**。
- 比较采用 `StringComparison.Ordinal`，区分大小写。
- `IsValid` 表示键非空，`new ResourceKey("Enemy/Melee")` 即有效键。

### 请求：ResourceLoadRequest

```csharp
public readonly struct ResourceLoadRequest
{
    public ResourceKey Key { get; }
    public Transform Parent { get; }
    public Vector3 WorldPosition { get; }
    public Quaternion WorldRotation { get; }
    public bool ShouldActivateAfterCreate { get; }
}
```

构造签名：

```csharp
new ResourceLoadRequest(
    ResourceKey key,
    Transform parent,
    Vector3 worldPosition,
    Quaternion worldRotation,
    bool shouldActivateAfterCreate = true)
```

- `Parent`、`WorldPosition`、`WorldRotation` 对应 `Object.Instantiate(prefab, pos, rot, parent)` 的世界坐标语义。
- `ShouldActivateAfterCreate` 默认 `true`，创建后立即激活。

### 结果：ResourceLoadResult

```csharp
public readonly struct ResourceLoadResult
{
    public ResourceKey Key { get; }
    public bool IsSuccess { get; }
    public GameObject Instance { get; }
    public IResourceHandle Handle { get; }
    public string ErrorMessage { get; }
}
```

- `IsSuccess == false` 时 `Instance` / `Handle` 为 `null`，原因在 `ErrorMessage`。
- 失败不抛异常，调用方靠 `IsSuccess` 走控制流。

### 释放句柄：IResourceHandle

```csharp
public interface IResourceHandle
{
    ResourceKey Key { get; }
    GameObject Instance { get; }
    bool IsReleased { get; }
    void Release();
}
```

- `Release()` 会销毁实例：运行期走 `Destroy`，编辑器走 `DestroyImmediate`。
- 谁持有句柄，谁负责释放；重复调用 `Release()` 无副作用（有 `IsReleased` 保护）。

### 获取服务：IInstantiateResource

- 外部通过 `ModuleServiceHub.Get<IInstantiateResource>()` 获取资源加载服务。
- 返回可能为 `null`；为空时静默返回，不要抛异常。
- `Register/Unregister` 由 `ResourceLoaderWiring` 维护，外部不要手调。

### 内部实现：Core 与 Wiring 胶水

- `ResourceLoadService`：`internal` MonoBehaviour，实现 `IInstantiateResource`，通过目录把键解析为预制体后实例化。
- `ResourceCatalogSO`：`internal` ScriptableObject，保存「资源键 → 预制体」映射。
- `ResourceLoaderWiring`：`internal` 接线胶水，`[DefaultExecutionOrder(-380)]`，在 `Awake` 把 `ResourceLoadService` 注册到 `ModuleServiceHub`，`OnDestroy` 反注册。
- 以上都属于模块内部实现，不是跨模块 API。

### 运行时空源约定

`ModuleServiceHub.Get<IInstantiateResource>()` 返回 `null` 通常表示尚未接线、注册对象已销毁、场景未就绪。外部应静默降级，不要临时查找 `SPResource.Core` 组件绕过服务。

## 二、使用模式

### 编辑期装配

1. 通过 `Create > SPResource > Resource Catalog` 创建目录资产，填充「资源键 → 预制体」条目。
2. 场景中放一个挂 `ResourceLoadService` 的对象，指定目录；再放一个挂 `ResourceLoaderWiring` 的对象，指定 Service。

### 标准调用：单个实例化

```csharp
using UnityEngine;

using SPFramework.Service;
using SPResource.Contract;

public sealed class ResourceConsumerExample : MonoBehaviour
{
    [SerializeField] private string _prefabKey = "Enemy/Melee";

    private IResourceHandle _handle;

    private void Start()
    {
        IInstantiateResource provider = ModuleServiceHub.Get<IInstantiateResource>();
        if (provider == null) return;

        var request = new ResourceLoadRequest(
            new ResourceKey(_prefabKey),
            parent: transform,
            worldPosition: transform.position,
            worldRotation: Quaternion.identity);

        ResourceLoadResult result = provider.Instantiate(request);
        if (!result.IsSuccess)
        {
            // 模块已播报失败原因，这里只做本侧兜底
            return;
        }

        _handle = result.Handle; // 持有句柄，负责释放
    }

    private void OnDestroy()
    {
        _handle?.Release();
    }
}
```

### 标准调用：批量实例化

```csharp
var requests = new[]
{
    new ResourceLoadRequest(new ResourceKey("Enemy/Melee"), null, Vector3.zero, Quaternion.identity),
    new ResourceLoadRequest(new ResourceKey("Enemy/Ranged"), null, Vector3.zero, Quaternion.identity),
};

IReadOnlyList<ResourceLoadResult> results = provider.InstantiateBatch(requests);

foreach (ResourceLoadResult result in results)
{
    if (result.IsSuccess)
    {
        _handles.Add(result.Handle);
    }
}
```

- 结果顺序与请求顺序一致。
- 逐条失败不影响其它条目，失败项 `IsSuccess == false`。

### 释放实例

```csharp
_handle?.Release();
_handle = null;
```

- 实例销毁统一走句柄，不要直接 `Destroy(result.Instance)`。
- 句柄只在本次实例化内有效，释放后不要再访问 `Instance`。

### 推荐的空源保护

```csharp
IInstantiateResource provider = ModuleServiceHub.Get<IInstantiateResource>();
if (provider == null) return;
```

- 空源不是异常流程，直接跳过即可。

### 场景接线顺序

```text
ResourceLoaderWiring.Awake 先把 ResourceLoadService 注册到 ModuleServiceHub
        ↓
下游模块每次按需 Get<IInstantiateResource>()
        ↓
只调用 IInstantiateResource 接口，不直接碰 Core
```

- 接线发生在 `Awake`（执行顺序 -380），调用方建议在 `Start` 或之后获取服务，或始终判空。

## 三、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| `using SPResource.Core` | `using SPResource.Contract` + `using SPFramework.Service` | Core 是实现层，外部禁引 |
| 直接引用 `ResourceLoadService` | `ModuleServiceHub.Get<IInstantiateResource>()` | Service 不是项目级 API |
| `provider.Instantiate(...)` 前不判空 | `if (provider == null) return;` | 服务可能未注册或已销毁 |
| 手调 `ModuleServiceHub.Register/Unregister` | 不调用 | 注册/反注册由 ResourceLoaderWiring 维护 |
| 失败后再补一条 `Debug.LogWarning(result.ErrorMessage)` | 只处理控制流，模块已播报失败原因 | 避免重复刷日志、职责越界 |
| 直接 `Destroy(result.Instance)` | `result.Handle.Release()` | 释放生命周期由句柄统一管理 |
| 不保存 `result.Handle` | 持有句柄并在生命周期结束时 `Release` | 不释放会导致实例残留 |
| 在 `Awake` 里假定服务已就绪 | 在 `Start` 或之后取，并判空 | 接线胶水在 Awake 执行顺序 -380 注册 |
| 键大小写或字符串与目录不一致 | `new ResourceKey("Enemy/Melee")` 精确匹配 | 目录按 Ordinal 区分大小写匹配 |

## 四、交叉引用

| 相关文档 | 用途 |
|---|---|
| [framework-core.md](framework-core.md) | 访问级别语义、Contract / 模块服务、事件总线 |
| [input-module-api.md](input-module-api.md) | 模块服务空源约定的同类参考 |
| [camera-module-api.md](camera-module-api.md) | 标准「接口 + 模块服务」调用范式参考 |

> 建议顺序：先看 `framework-core.md`，再按需看各模块文档。

### 边界提醒

- `SPResource.Contract`：外部可依赖的接口契约。
- `SPResource.Wiring`：模块内 `internal` 接线胶水，外部禁用。
- `SPResource.Core`：资源加载内部实现，外部禁引。
- `ResourceLoaderWiring`：`internal` 接线胶水，外部禁用。
- `SPFramework.Service`：通过 `ModuleServiceHub` 获取资源服务。
