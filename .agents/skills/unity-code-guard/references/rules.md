# Unity AI 编辑硬门禁

> 该文件用于 AI 在 Unity 项目中执行代码、场景、Prefab、配置与资源编辑时的硬性约束
> 任何不满足本文件的修改，默认视为无效修改

## 0 适用范围

- 本文件只约束 AI 的编辑行为，不约束人类手工的临时排查
- 本文件的优先级高于个人习惯、局部示例和局部风格
- 未被明确允许的行为，一律视为禁止
- 本文件中的规则以可执行、可判断、可验证为准，禁止模糊表述

## 1 编辑边界

- 只修改本次任务明确要求的文件与内容
- 不得顺手修复无关问题
- 不得顺手重构无关代码
- 不得顺手格式化整个项目
- 不得新增无关文件、无关依赖、无关资源
- 不得删除、移动、重命名与任务无关的文件、对象或组件
- 不得在未说明原因时改动 public API、序列化字段、事件名、Unity 回调名、资源路径、Prefab 绑定、场景引用
- 涉及上述内容时，必须保证兼容；不能保证兼容时，先停止并说明
- 未确认影响前，不得修改 `ProjectSettings`、`Packages/manifest.json`、`Packages/packages-lock.json`
- 不得保留临时代码、调试输出、注释掉的旧实现、一次性修补代码
- 不得用“顺手”“优化”“整理”作为修改理由

## 2 命名规则

### 2.1 通用规则

- 命名统一使用英文
- 命名必须能直接表达职责，不得使用无意义缩写
- 同一概念在同一项目内必须使用同一命名风格
- 布尔名称必须表达真假判断，优先使用 `is`、`has`、`can`、`should`
- 不得把多个职责塞进一个名称
- 不得为了显得简短而牺牲可读性
- 不得同时存在两套可替换命名方式

### 2.2 标识符大小写

| 标识符 | 约定 | 示例 |
|--------|------|------|
| 命名空间、类、结构体、接口、属性、方法、事件 | 大驼峰 | `TeamController`、`IDamageable`、`OnDead` |
| 私有实例字段 | `_` + 小驼峰 | `private int _currentHp;` |
| 私有静态字段 | `s_` + 小驼峰 | `private static int s_totalCount;` |
| 常量 | 大驼峰 | `private const int MaxLevel = 100;` |
| `static readonly` | 大驼峰 | `private static readonly int StepMs = 16;` |
| 局部变量、方法参数 | 小驼峰 | `int remainHp = 10;` |
| 接口 | `I` 前缀 | `IDamageable` |
| 抽象基类 | `Base` 后缀 | `EnemyBase` |

### 2.3 Unity 字段命名

- 需要在 Inspector 暴露的字段，必须使用 `[SerializeField] private`
- 禁止使用裸 `public` 字段直接暴露给 Inspector
- 所有序列化字段统一使用 `_` + 小驼峰
- 组件引用字段名称必须能直接看出用途或类型
- 缩写只能用于项目内已统一且无需解释的标准缩写

```csharp
[SerializeField, Tooltip("最大血量")] private int _maxHp = 100;
[SerializeField, Tooltip("移动速度 单位 m/s")] private float _moveSpeed = 5f;
[SerializeField, Tooltip("动画控制器引用")] private Animator _anim;
```

## 3 注释规则

- 注释只写必要信息，禁止解释显而易见的代码
- 注释不得引用脚本名、文件名、类名作为重复说明
- 注释中不使用全角破折号 `——`，统一使用 ` - `
- 注释中不使用句号 `。`
- 注释必须简短、直接、可验证
- 注释必须与代码同步，代码变化后注释失效时应立即更新或删除
- 禁止写“临时”“待优化”“后续再改”之类无执行价值注释

### 3.1 类注释

- 类必须提供 `<summary>`
- 类注释只描述职责，不描述实现细节

```csharp
/// <summary>
/// 队伍控制器 - 管理角色切换与状态分发
/// </summary>
public class TeamController : MonoBehaviour
```

### 3.2 方法与属性注释

- 新声明的公有或受保护成员必须提供 XML summary
- 继承自接口或父类的重写成员可以直接使用 `/// <inheritdoc />`
- 若重写成员有额外约束、副作用或局部差异，可在 `inheritdoc` 之外补充简短说明
- 只有在逻辑复杂且命名无法充分表达时，私有方法才允许加注释
- 参数和返回值只在有歧义或有约束时说明

```csharp
/// <summary>
/// 对目标造成伤害
/// </summary>
/// <param name="amount">伤害量 必须大于 0</param>
/// <returns>实际扣减量</returns>
public int TakeDamage(int amount) { }
```

```csharp
/// <inheritdoc />
public override CharacterIntentionFrame CurrentFrame { get; }
```

### 3.3 属性与常量注释

- 常量与新声明的公开属性必须提供 `<summary>`
- 继承或重写属性可以直接使用 `/// <inheritdoc />`
- 公开属性注释只说明语义，不解释推导过程

```csharp
/// <summary>最大血量上限</summary>
public int MaxHp => _maxHp;
```

```csharp
/// <inheritdoc />
public override CharacterIntentionFrame CurrentFrame { get; }
```

### 3.4 字段注释

- 私有字段默认不写行内注释
- 可序列化字段优先使用 `[Tooltip]`
- 只有当字段意图无法从命名直接判断时，才允许补充最短必要注释

## 4 Inspector 与序列化约束

- 所有需要在 Inspector 中配置的字段必须使用 `[SerializeField] private`
- 每个序列化字段必须带 `[Tooltip]`
- `[Tooltip]` 必须说明字段含义与单位，不能只写同义词
- 相关字段应使用 `[Header]` 分组
- 数值字段应在合理时使用 `[Range]`
- 一个字段只能有一个责任，要么设计期配置，要么运行时维护，不得同时承担
- 运行时状态不得放入 ScriptableObject
- ScriptableObject 只保存静态配置，不保存会被多实例共享改写的状态
- 修改序列化字段名、类型、顺序之前，必须确认不会破坏已有资产、Prefab、场景与存档

```csharp
[Header("战斗")]
[SerializeField, Tooltip("最大血量")] private int _maxHp = 100;
[SerializeField, Range(0, 10), Tooltip("移动速度 单位 m/s")] private float _moveSpeed = 5f;
```

## 5 禁止写法

- 禁止 public 字段直接暴露给 Inspector
- 禁止用注释代替代码
- 禁止注释掉旧逻辑当备份
- 禁止保留 `TODO`、`FIXME`、`HACK`、`TEMP` 作为交付结果
- 禁止空行堆砌、无意义重排、纯格式噪音
- 禁止制造歧义的缩写、临时别名和多套命名并存
- 禁止通过“看起来更干净”而改变行为
- 禁止在未明确要求时修改场景层级、Prefab 结构、资源命名、动画状态机、事件绑定
- 禁止新增无关依赖、无关脚本、无关资源来绕过规则

## 6 生命周期与事件订阅约束

- 事件订阅必须有对应退订
- `OnEnable` 中订阅的事件必须在 `OnDisable` 中退订
- `Awake` 或 `Start` 中建立的长期订阅必须在 `OnDestroy` 中退订
- 禁止只订阅不退订
- 禁止依赖对象销毁隐式清理事件订阅
- 禁止在生命周期方法中混放无关职责
- 生命周期方法只放与该生命周期阶段直接相关的逻辑

## 7 不确定时的处理

- 任何一项规则无法满足时，先停止，不要猜
- 先向用户确认，不要擅自折中
- 如果需要例外，必须显式说明例外原因与影响
- 如果修改会影响序列化、引用关系或运行时行为，必须先说明风险再执行
