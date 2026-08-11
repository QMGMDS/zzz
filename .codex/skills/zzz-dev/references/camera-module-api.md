# 摄像机模块 API 速览

> 适用场景：下游模块按需做相机系方向转换，或请求摄像机切换跟随目标。
> 边界约定：外部只允许引用 `SPCamera.Contract` + `SPCamera.Wiring`，不要直接引入 `SPCamera.Core`。
> 使用原则：外部只拿 `Provider SO`，再通过 `Provider` 取接口；不要自己碰内部实现。

## 一、核心 API

### 对外公开的命名空间

```csharp
using SPCamera.Contract;
using SPCamera.Wiring;
```

- `Contract`：稳定的接口契约。
- `Wiring`：槽位 SO 和接线入口，负责把内部实现暴露给外部。

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

### 入口资产：CameraTransformProviderSO

- 外部通过 `CameraTransformProviderSO.Provider` 获取 `IConvertCameraTransform`。
- `Provider` 可能为空；为空时应静默降级，不要抛异常。
- `Provider` 为空通常表示尚未接线、对象已销毁，或场景未就绪。

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

### 入口资产：CameraFollowTargetProviderSO

- 外部通过 `CameraFollowTargetProviderSO.Provider` 获取 `ISetCameraFollowTarget`。
- `Provider` 可能为空；为空时通常直接跳过这次请求。
- 外部不直接调用摄像机内部跟随器实现。

### 内部实现：Wiring 胶水

- `CameraTransformWiring`、`CameraFollowTargetWiring` 之类的类型属于模块内部接线。
- 它们负责把 `Core` 实现注入到 `Provider SO`。
- 这些类型即使出现在 `SPCamera.Wiring` 命名空间，也不代表外部可以依赖。

### 运行时空源约定

`Provider SO` 是运行时信箱，不保证任何时刻都有值。

常见原因：

- 场景中缺少摄像机接线对象
- 接线对象尚未完成 `Awake`
- 接线对象没有配置对应 Provider SO
- 外部模块引用了错误的 Provider SO 资产
- 摄像机对象已销毁

推荐处理：

- 坐标转换 Provider 为空：直接返回输入方向
- 跟随目标 Provider 为空：直接跳过请求
- 不要临时查找 `SPCamera.Core` 组件绕过 Provider
- 不要 fallback 到 `Camera.main` 自己复制实现

## 二、使用模式

### 标准调用：坐标转换

```csharp
[SerializeField] private CameraTransformProviderSO _cameraTransform;

private Vector2 ToWorldMoveDirection(Vector2 inputDirection)
{
    IConvertCameraTransform converter = _cameraTransform == null
        ? null
        : _cameraTransform.Provider;

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
[SerializeField] private CameraFollowTargetProviderSO _cameraFollowTarget;

public void SwitchFollowTarget(Transform target)
{
    ISetCameraFollowTarget setter = _cameraFollowTarget == null
        ? null
        : _cameraFollowTarget.Provider;

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
var provider = _cameraTransform?.Provider;
if (provider == null) return;
```

```csharp
var setter = _cameraFollowTarget?.Provider;
if (setter == null) return;
```

- `Provider` 为空时应静默处理。
- 不要把空源当成异常流程。

### 场景接线顺序

```text
CameraWiring 先把 Core 实现注入 Provider SO
        ↓
下游模块每次按需 Pull Provider
        ↓
只调用 Contract 接口，不直接碰 Core
```

## 三、常见错误

| 错误写法 | 正确写法 | 原因 |
|---|---|---|
| `using SPCamera.Core` | `using SPCamera.Contract` + `using SPCamera.Wiring` | Core 是实现层，外部禁引 |
| 直接引用 `CameraFollower` | 通过 `CameraFollowTargetProviderSO.Provider` 获取接口 | 跟随器不是项目级 API |
| 直接调用跟随器 | `_cameraFollowTarget?.Provider?.SetCameraFollowTarget(target);` | 不跨模块调用 Core 实现 |
| 用 `Camera.main` 转方向 | `_cameraTransform?.Provider?.ConvertCameraTransform(inputDirection)` | 方向规则应由摄像机模块统一处理 |
| 不判空 Provider | `_cameraTransform.Provider.ConvertCameraTransform(...)` | Provider 可能未注入或已销毁 |
| 依赖 Wiring 胶水 | `[SerializeField] private CameraTransformWiring _cameraWiring;` | `internal` 胶水不是外部 API |
| 直接改摄像机位置 | `cameraTransform.position = ...` | 外部不接管摄像机实现细节 |
| 复制方向算法 | 自己算 `Camera.main.transform.forward` | 会破坏统一规则，容易与模块逻辑不一致 |

## 四、交叉引用

| 相关文档 | 用途 |
|---|---|
| [project-module-boundaries.md](project-module-boundaries.md) | 项目级模块边界、`public` / `internal` 约定、跨模块引用规则 |
| [input-module-api.md](input-module-api.md) | 输入方向 `MoveDirection` 的来源与语义 |
| [character-module-api.md](character-module-api.md) | 角色模块如何消费摄像机方向转换结果 |

> 建议顺序：先看 `project-module-boundaries.md`，再看输入、摄像机、角色三份模块文档。

### 边界提醒

- `SPCamera.Contract`：外部可依赖的接口契约。
- `SPCamera.Wiring`：外部只依赖 `public` Provider SO。
- `SPCamera.Core`：摄像机内部实现，外部禁引。
- `internal` Wiring 胶水：模块内部接线细节，外部禁用。
