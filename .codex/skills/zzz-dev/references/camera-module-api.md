# 摄像机模块核心 API

> **适用场景**：从输入模块方向计算摄像机坐标系下的世界移动方向、挂接相机跟随/坐标转换、排查方向不对问题
> **模块边界**：外部只允许引 `SPCamera.Contract` + `SPCamera.Wiring`，禁止引 `SPCamera.Core`

## 模块总览

```
SmoothCameraTarget（跟随目标）   CameraMoveAxisConverter（坐标转换）
       │                                    │
       │                   CameraCoordinateConverterWiring ──→ CoordinateConverterProviderSO
       │                                    │                   运行时信箱
       │                                    │
       ▼                                    ▼
  Transform 平滑跟随                  InputTranslator pull 得到世界 XZ 目标方向
```

摄像机模块提供两项独立能力：
1. **坐标转换**：将输入平面方向（x=右，y=前）转换为世界 XZ 目标方向（x=X，y=Z），消费方为角色模块 `InputTranslator`
2. **平滑跟随**：挂载于 CameraLook 物体，持续平滑跟随角色 Transform

两项能力解耦，互不依赖。

---

## 一、Contract 层（外部可用）

### 坐标转换接口：ICoordinateConverter

```csharp
namespace SPCamera.Contract
{
    public interface ICoordinateConverter
    {
        /// <summary>
        /// 将输入平面方向转换为世界 XZ 目标方向。
        /// </summary>
        /// <param name="inputDirection">输入模块产出的平面方向（x=右，y=前，归一化单位向量）</param>
        /// <returns>世界 XZ 目标方向，XY 分量分别对应世界 XZ 轴</returns>
        Vector2 ConvertToWorldMoveDirection(Vector2 inputDirection);
    }
}
```

### 命名空间引用

```csharp
using SPCamera.Contract;   // ICoordinateConverter
using SPCamera.Wiring;     // CoordinateConverterProviderSO
```

---

## 二、Wiring 层（外部可用）

### 槽位 SO：CoordinateConverterProviderSO

```csharp
namespace SPCamera.Wiring
{
    [CreateAssetMenu(menuName = "SPCamera/Coordinate Converter Provider",
                     fileName = "CoordinateConverterProvider")]
    public class CoordinateConverterProviderSO : ScriptableObject
    {
        public ICoordinateConverter Provider { get; }   // 未注入时为 null
        internal void Bind(ICoordinateConverter provider);
        internal void Clear();
    }
}
```

`Provider` 为 null 时不抛异常，下游静默降级：`InputTranslator` 在坐标转换器为空时直通输入方向（不做相机系转换）。

---

## 三、使用模式

### 标准下游消费（以角色模块 InputTranslator 为例）

```csharp
using SPCamera.Contract;
using SPCamera.Wiring;
using UnityEngine;

public class ConsumerExample : MonoBehaviour
{
    [SerializeField] private CoordinateConverterProviderSO _coordinateConverter;

    private void Update()
    {
        ICoordinateConverter converter = _coordinateConverter?.Provider;
        Vector2 inputDir = new Vector2(0f, 1f); // e.g. 输入向前

        // 空源降级：converter 为 null 时直通
        Vector2 worldDir = converter != null
            ? converter.ConvertToWorldMoveDirection(inputDir)
            : inputDir;

        // worldDir.x → 世界 X, worldDir.y → 世界 Z
    }
}
```

### 空源保护（重要）

```csharp
// Provider 可能为 null：接线胶水未放 / 未注入 / Converter 已销毁
// InputTranslator 的降级策略：converter 为 null 时直通输入方向
var converter = _coordinateConverter?.Provider;
Vector2 worldDir = converter != null
    ? converter.ConvertToWorldMoveDirection(input.MoveDirection)
    : input.MoveDirection;
```

### 场景接线步骤

1. 相机根物体（持 yaw 者）挂 `CameraMoveAxisConverter`（参考系默认自身）
2. 同一物体挂 `CameraCoordinateConverterWiring`，填入 Converter 引用和 ProviderSO 槽位
3. 创建 `CoordinateConverterProvider` SO 资产
4. 同一份 SO 填入所有消费者的 `_coordinateConverter` 槽位（如 `InputTranslator`）

---

## 四、常见错误

| 错误写法 | 正确写法 | 原因 |
|---------|---------|------|
| `Camera.main.transform.forward` | `transform.forward`（挂 yaw 物体上） | 不语义耦合 Camera.main，坐标转换只认 yaw，不知 main |
| 零向量不做保护直接 `normalized` | `sqrMagnitude <= epsilon` 时直接 return Vector2.zero | 零向量归一化出 NaN |
| 在输出方向做 `-forward` 翻转 | 只做纯坐标转换 | 角色面朝方向是角色侧的事，相机系不关心 |
| `using SPCamera.Core` | `using SPCamera.Contract` + `using SPCamera.Wiring` | Core 是实现层，外部禁引 |
| 把 yaw 混入 SmoothCameraTarget 的位置平滑 | 两者解耦 — yaw 由父级/根物体持，Target 只做位置跟随 | 职责分离 |
| 在角色侧用 `Camera.main` 自己做转换 | 经 ProviderSO → ICoordinateConverter 统一转换 | 避免散落重复逻辑 |

---

## 五、内部结构（外部禁引，仅供了解）

| 层 | 命名空间 | 文件 | 职责 |
|----|---------|------|------|
| Contract | `SPCamera.Contract` | `ICoordinateConverter.cs` | 坐标转换接口 |
| Core | `SPCamera` | `CameraMoveAxisConverter.cs` | 转换实现 — 取参考系 yaw，投影 XZ 平面做方向旋转变换 |
| Core | `SPCamera` | `SmoothCameraTarget.cs` | 平滑跟随 — 用 SmoothDamp 跟随目标 Transform |
| Wiring | `SPCamera.Wiring` | `CoordinateConverterProviderSO.cs` | 槽位 SO（运行时信箱） |
| Wiring | `SPCamera.Wiring` | `CameraCoordinateConverterWiring.cs` | 接线胶水，Awake Bind / OnDestroy Clear |

执行时序：输入模块采集 → `CameraCoordinateConverterWiring [-380]` 注入 → `InputTranslator` Pull（角色 `Update`）。

### 坐标转换实现细节

```
输入：Vector2 inputDirection（x=右，y=前，已归一化）
算法：forward = Reference.forward 投影到 XZ（y=0），right = Reference.right 投影到 XZ（y=0）
      world = forward * inputDirection.y + right * inputDirection.x
输出：Vector2(x=world.x, y=world.z)，归一化单位方向
```

- `_reference` 槽位留空时默认使用自身 `transform`，可显式指定任意持 yaw 的节点
- 输入为零向量（sqrMagnitude ≤ 1e-6）时直接返回零，不进行任何计算
- 投影后方向为零（纯垂直向下/向上）时返回零，不抛异常

### SmoothCameraTarget 实现细节

```
每帧 Update：_targetCharacter.position → 保持 y 不变 → Vector3.SmoothDamp
参数：_smoothTime（默认 0.3s）、_maxSpeed（默认 40f/s）
执行顺序：[DefaultExecutionOrder(-50)]
```

---

## 六、交叉引用

| 相关文档 | 内容 |
|---------|------|
| [input-module-api.md](input-module-api.md) | `InputTranslator` 上游 — 帧输入数据形状（MoveDirection 语义） |
| [character-module-api.md](character-module-api.md) | `InputTranslator` 所在模块 — 角色意图翻译 + 消费相机系方向 |
