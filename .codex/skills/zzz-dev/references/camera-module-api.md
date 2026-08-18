# 摄像机模块 API

## 一、模块概述

摄像机模块（`Module.SPCamera` 程序集）对外提供两项能力，均以**模块级服务**形式注册到 `ModuleServiceHub`，契约定义在 `SPCamera.Contract` 命名空间：

| 契约接口 | 能力 |
| --- | --- |
| `IConvertCameraTransform` | 将平面方向关联摄像机参考系，产出世界 XZ 方向 |
| `ISetCameraFollowTarget` | 设置摄像机跟随目标，摄像机自行平滑移动到新目标 |

边界约定：外部只允许引用 `SPCamera.Contract`，通过 `ModuleServiceHub.TryGet` 获取服务；`SPCamera.Core` 与 `SPCamera.Wiring` 中的类型均为 `internal`，编译期对外不可见。

### IConvertCameraTransform

```csharp
public interface IConvertCameraTransform : IModuleService
{
    Vector2 ConvertCameraTransform(Vector2 inputDirection);
}
```

- `inputDirection` 是平面方向：`x` 分量表示右方向，`y` 分量表示前方向。
- 接口不做防御性处理，调用方需保证输入合法。
- 返回值是关联摄像机参考系后的**世界 XZ 方向**（已归一化），不是摄像机物体坐标系下的方向。

### ISetCameraFollowTarget

```csharp
public interface ISetCameraFollowTarget : IModuleService
{
    void SetCameraFollowTarget(Transform target);
}
```

- 调用方只提交希望跟随的 `Transform`，跟随方式（平滑时间、最大速度等）由摄像机模块内部决定。
- 切换目标时摄像机会立即吸附到新目标的 XZ 位置，随后逐帧平滑跟随；跟随仅发生在世界 XZ 平面，摄像机高度保持不变。

## 二、API 调用示例

获取服务统一使用 `ModuleServiceHub.TryGet`。服务未注册或已销毁时返回 `false` 且 `out` 结果为 `null`，调用方必须自行降级，不可默认服务必然可用。

### 平面方向转世界 XZ 方向

```csharp
using SPFramework.Service;
using SPCamera.Contract;

Vector2 worldDirection = ModuleServiceHub.TryGet<IConvertCameraTransform>(out IConvertCameraTransform converter)
    ? converter.ConvertCameraTransform(inputDirection)
    : inputDirection; // 服务缺失时降级为原始平面方向
```

### 设置摄像机跟随目标

```csharp
using SPFramework.Service;
using SPCamera.Contract;

if (ModuleServiceHub.TryGet<ISetCameraFollowTarget>(out ISetCameraFollowTarget follow))
    follow.SetCameraFollowTarget(target); // 服务缺失时跳过本次请求
```

## 三、反例

| 反例 | 正确做法 |
| --- | --- |
| 引用 `SPCamera.Core` / `SPCamera.Wiring` 中的类型 | 只引用 `SPCamera.Contract`，经 `ModuleServiceHub` 获取接口 |
| 通过 `Camera.main` 或自行查找场景摄像机组件，自己实现方向换算或跟随逻辑 | 借用 `IConvertCameraTransform` / `ISetCameraFollowTarget` |
| 不判空直接调用服务，默认其必然可用 | 用 `TryGet` 获取，失败时按上文示例降级 |
| 直接修改摄像机的 `Transform`（位置、旋转、挂点）来实现跟随或切换目标 | 调用 `ISetCameraFollowTarget.SetCameraFollowTarget`，运动方式由模块内部决定 |
| 长期缓存服务接口并假设其永久有效 | 服务随模块内部的注册/注销生命周期变动，每次按需 `TryGet` |
