# 摄像机模块 API 速览

> 适用场景：下游模块按需做相机系方向转换，或请求摄像机切换跟随目标。
> 边界约定：外部只允许引用 `SPCamera.Contract`，通过 `ModuleServiceHub.Get<...>()` 获取摄像机服务；不要直接引入 `SPCamera.Core` / `SPCamera.Wiring`。
> 使用原则：外部通过 `ModuleServiceHub.Get<...>()` 取接口，判空后调用；不要自己碰内部实现。

## 一、核心 API

### 对外公开的命名空间

```csharp
using SPFramework.Service;
using SPCamera.Contract;
```

- `Contract`：稳定的接口契约。
- `Wiring`：模块内接线胶水，负责把 `Core` 实现注册到 `ModuleServiceHub`，外部不引用。

### 接口：IConvertCameraTransform

```csharp
public interface IConvertCameraTransform
{
    Vector2 ConvertCameraTransform(Vector2 inputDirection);
}
```

- 输入值是“角色输入方向”，不是世界方向。
- `inputDirection.x` 表示输入右方向。
- `inputDirection.y` 表示输入前方向。
- 返回值是世界 XZ 平面方向。
- 外部不重复实现相机朝向换算。

### 获取服务：IConvertCameraTransform

- 外部通过 `ModuleServiceHub.Get<IConvertCameraTransform>()` 获取坐标转换服务。
- 返回可能为 `null`；为空时应静默降级，不要抛异常。
- 返回 `null` 通常表示尚未接线、对象已销毁，或场景未就绪。

### 接口：ISetCameraFollowTarget

```csharp
public interface ISetCameraFollowTarget
{
    void SetCameraFollowTarget(Transform target);
}
```

- 外部只提交希望跟随的 `Transform`。
- 摄像机模块内部决定实际跟随方式。
- 外部不要直接改摄像机位置、旋转或挂点。

### 获取服务：ISetCameraFollowTarget

- 外部通过 `ModuleServiceHub.Get<ISetCameraFollowTarget>()` 获取跟随目标设置服务。
- 返回可能为 `null`；为空时通常直接跳过这次请求。
- 外部不直接调用摄像机内部跟随器实现。

### 内部实现：Wiring 胶水

- `CameraTransformWiring`、`CameraFollowTargetWiring` 之类的类型属于模块内部接线。
- 它们负责把 `Core` 实现注册到 `ModuleServiceHub`。
- 这些类型是 `internal`，外部不可依赖。

### 运行时空源约定

`ModuleServiceHub.Get<...>()` 不保证任何时刻都有值。

常见原因：

- 场景中缺少摄像机接线对象
- 接线对象尚未完成 `Awake`
- 对应服务尚未注册
- 摄像机对象已销毁

推荐处理：

- 坐标转换服务为空：直接返回输入方向
- 跟随目标服务为空：直接跳过请求
- 不要临时查找 `SPCamera.Core` 组件绕过服务
- 不要 fallback 到 `Camera.main` 自己复制实现

## 二、使用模式

### 标准调用：坐标转换

```csharp
private Vector2 ToWorldMoveDirection(Vector2 inputDirection)
{
    IConvertCameraTransform converter = ModuleServiceHub.Get<IConvertCameraTransform>();

    return converter == null
        ? inputDirection
        : converter.ConvertCameraTransform(inputDirection);
}
```

- 适合角色模块把输入方向转成世界移动方向。
- 适合 UI / 调试模块显示相机系输入方向。
- 下游只消费结果，不关心摄像机内部算法。

### 标准调用：切换跟随目标

```csharp
public void SwitchFollowTarget(Transform target)
{
    ISetCameraFollowTarget setter = ModuleServiceHub.Get<ISetCameraFollowTarget>();

    if (setter == null) return;
    if (target == null) return;

    setter.SetCameraFollowTarget(target);
}
```

- 适合队伍模块切换当前上场角色。
- 适合角色生成后设置初始跟随目标。
- 适合过场或玩法逻辑请求摄像机跟随另一个目标。

### 推荐的空源保护

```csharp
IConvertCameraTransform provider = ModuleServiceHub.Get<IConvertCameraTransform>();
if (provider == null) return;
```

```csharp
ISetCameraFollowTarget setter = ModuleServiceHub.Get<ISetCameraFollowTarget>();
if (setter == null) return;
```

- 服务为空时应静默处理。
- 不要把空源当成异常流程。

### 场景接线顺序

```text
CameraWiring 先把 Core 实现注册到 ModuleServiceHub
        ↓
下游模块每次按需 Get<...>()
        ↓
只调用 Contract 接口，不直接碰 Core
```

## 三、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| `using SPCamera.Core` | `using SPCamera.Contract` + `using SPFramework.Service` | Core 是实现层，外部禁引 |
| 直接引用 `CameraFollower` | `ModuleServiceHub.Get<ISetCameraFollowTarget>()` | 跟随器不是项目级 API |
| 直接调用跟随器 | `ModuleServiceHub.Get<ISetCameraFollowTarget>()?.SetCameraFollowTarget(target);` | 不跨模块调用 Core 实现 |
| 用 `Camera.main` 转方向 | `ModuleServiceHub.Get<IConvertCameraTransform>()?.ConvertCameraTransform(inputDirection)` | 方向规则应由摄像机模块统一处理 |
| 不判空服务 | `ModuleServiceHub.Get<IConvertCameraTransform>().ConvertCameraTransform(...)` | 服务可能未注册或已销毁 |
| 依赖 Wiring 胶水 | `[SerializeField] private CameraTransformWiring _cameraWiring;` | `internal` 胶水不是外部 API |
| 直接改摄像机位置 | `cameraTransform.position = ...` | 外部不接管摄像机实现细节 |
| 复制方向算法 | 自己算 `Camera.main.transform.forward` | 会破坏统一规则，容易与模块逻辑不一致 |

## 四、交叉引用

| 相关文档 | 用途 |
|---|---|
| [framework-core.md](framework-core.md) | 访问级别语义、模块通讯方式、核心原则 |
| [input-module-api.md](input-module-api.md) | 输入方向 `MoveDirection` 的来源与语义 |
| [character-module-api.md](character-module-api.md) | 角色模块如何消费摄像机方向转换结果 |

> 建议顺序：先看 `framework-core.md`，再看输入、摄像机、角色三份模块文档。

### 边界提醒

- `SPCamera.Contract`：外部可依赖的接口契约。
- `SPCamera.Wiring`：模块内 `internal` 接线胶水，外部禁用。
- `SPCamera.Core`：摄像机内部实现，外部禁引。
- `internal` Wiring 胶水：模块内部接线细节，外部禁用。
- `SPFramework.Service`：通过 `ModuleServiceHub` 获取摄像机服务。
