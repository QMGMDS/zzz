# 编码护栏规则（权威定义）

本文件是护栏四域的**权威定义源**。硬门禁 `tools/lint/lint.csx` 的 R1~R8 规则与此处章号对齐；软复核也会对照此处自检。

> 护栏不是天花板：四域内强制，四域外放手发挥（详见 SKILL.md 顶部）。

## 1 命名约定

### 1.1 标识符大小写约定

| 标识符 | 约定 | 示例 |
|--------|------|------|
| 命名空间、类、结构、接口、属性、方法、事件 | 大驼峰 | `TeamController`、`IDamageable`、`OnDead` |
| 私有实例字段 | `_` + 小驼峰 | `private int _currentHp;` |
| 私有静态字段 | `s_` + 小驼峰 | `private static int s_totalCount;` |
| 常量 / `static readonly` | 大驼峰 | `private const int MaxLevel = 100;` |
| 局部变量、方法参数 | 小驼峰 | `int remainHp = 10;` |
| 布尔 | `is`/`has`/`can`/`should` 前缀 | `bool _isDead;` |
| 接口 | `I` 前缀 | `IDamageable` |
| 抽象类 | `Base` 后缀或仅父类职责词 | `EnemyBase` |

### 1.2 字段与方法示例

```csharp
private int _currentHp;                    // 私有字段：_小驼峰
private static int s_totalCount;          // 私有静态：s_小驼峰
private const int MaxLevel = 100;           // const：大驼峰
private static readonly int StepMs = 16;    // static readonly：大驼峰

public int CurrentHp => _currentHp;         // 公开属性：大驼峰
public bool IsDead => _currentHp <= 0;     // 布尔属性：is 前缀
public event Action<int> OnHpChanged;      // 事件：大驼峰

// 异步方法：Async 后缀（仅 Task/UniTask 系；协程保留 XxxRoutine/XxxCoroutine 习惯，见下注）
public async UniTask LoadDataAsync() { }
// 事件回调：On 前缀
private void OnHpChanged(int hp) { }
```

> 异步方案本身不受本 skill 约束，由 AI 自选；仅约束"异步方法名带 `Async` 后缀"这条命名一致性。
>
> 协程例外：返回 `IEnumerator` 的协程免 `Async` 后缀，保留 `XxxRoutine`/`XxxCoroutine` 命名习惯（语义比 Async 更明确）；仅 `Task`/`UniTask` 系异步方法要求 `Async` 后缀。此约定与硬门禁 `lint.csx` 的 R6 规则对齐。

### 1.3 Unity 序列化字段命名

- Inspector 字段统一 `_` + 小驼峰 + `[SerializeField] private`，**不要**用 `public` 字段裸露给 Inspector。

```csharp
[SerializeField, Tooltip("最大血量")] private int _maxHp = 100;
[SerializeField, Tooltip("移动速度 (m/s)")] private float _moveSpeed = 5f;
```

- 组件引用后缀用类型简称：`_rigid`、`_anim`、`_renderer`、`_collider`。
- 类名尽量说明职责，避开 `Manager`/`Script`/`Handler` 滥用。

## 2 注释规范

注释要求简明扼要，**不引用脚本名**。

- 不使用 `——`，使用 ` - `。例：`玩家角色控制器 - Root MonoBehaviour 驱动源。`

- **类**：提供 `<summary>`，说明职责。

```csharp
/// <summary>
/// 队伍控制器 - 控制玩家角色子物体
/// </summary>
public class TeamController : MonoBehaviour
```

- **公有/保护方法**：三行 XML 注释，必要时介绍参数和返回值。

```csharp
/// <summary>
/// 对目标造成伤害
/// </summary>
/// <param name="amount">伤害量，必须 > 0</param>
/// <returns>实际扣减量（考虑护盾后的值）</returns>
public int TakeDamage(int amount) { }
```

- **常量/公开属性**：单行 XML 注释。
  ```csharp
  /// <summary>最大血量上限</summary>
  public int MaxHp => _maxHp;
  ```

- **私有字段**：用 `[Tooltip]` 代替行内注释；逻辑确实非显而易见时才加 `//`。

## 3 Inspector 字段约定

- **私有优先**：所有 Inspector 字段用 `[SerializeField] private`，不要 `public` 字段。
- **必带 Tooltip**：每个序列化字段必须带 `[Tooltip]`，说明含义和单位。
- **单一所有者**：一个字段要么设计师编辑要么运行时维护，两者不可同时写。
- 相关字段用 `[Header]` 分组，数值用 `[Range]` 约束，避免 Inspector 一片扁平。

```csharp
[Header("战斗")]
[SerializeField, Tooltip("最大血量")] private int _maxHp = 100;
[SerializeField, Range(0, 10), Tooltip("移动速度 (m/s)")] private float _moveSpeed = 5f;
```

- **ScriptableObject 边界**：静态配置（武器数值、关卡描述）可放 SO，运行时只读；**运行时状态（当前血量、选中目标）禁止放进 SO**——SO 是资产，会被多实例共享串改。

## 4 生命周期与事件订阅

### 4.1 MonoBehaviour 生命周期选择

| 想要 | 用 | 备注 |
|------|----|------|
| 组件/引用缓存 | `Awake` | 自身内部就绪 |
| 跨对象引用/初始化 | `Start` | 依赖他人在 `Awake` 设置完的引用 |
| 每帧逻辑 | `Update` | 必要才用 |
| 一次性延迟/间隔 | 协程 或 AI 选择的异步方案 | 不要在 `Update` 累 `Time.deltaTime` 做长计时 |
| 销毁清理 | `OnDestroy` | 退订事件、取消 CTS |

> `Awake` 做"自身内部就绪"，`Start` 做"与他人连接"。

### 4.2 事件订阅与释放

- 成对出现：`OnEnable` 订阅 → `OnDisable` 退订；`AddListener` 必须有对应 `RemoveListener`。
- 回调方法用 `Handle`/`On` 前缀并对应事件名：`HandleHpChanged`、`OnLevelLoaded`。
- 用 `CancellationTokenSource` 时，`OnDestroy` 必须 `cts.Cancel(); cts.Dispose();`。

```csharp
private void OnEnable() => _broker.OnHpChanged += HandleHpChanged;
private void OnDisable() => _broker.OnHpChanged -= HandleHpChanged;
private void OnDestroy() { _cts?.Cancel(); _cts?.Dispose(); }
```

### 4.3 协程

- `StartCoroutine` 需要能在销毁/切场景时停掉（随 MonoBehaviour 销毁自动停，但显式停更稳）。
- 别 `yield return null` 死循环做计时，用 `yield return new WaitForSeconds(...)`。

## 5 护栏内禁止写法

以下属于四个护栏领域，违反即需纠正：

- `public` 可变字段裸露（用属性或事件替代）。
- `Find("Name")` / `FindObjectOfType<T>()` 在 `Update` 里用；只在 `Awake`/`Start` 缓存一次。
- `Update` 里 `GetComponent` / `transform.GetChild` / 字符串拼接。
- 订阅事件不退订，造成"死后回调"空引用。
- 一个字段同时被 Inspector 编辑和运行时脚本写回。
- 把运行时可变状态塞进 ScriptableObject。
