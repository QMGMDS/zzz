# 命名规则

## 定位

约束 `Assets/_Scripts` 下所有 `.cs` 代码的命名：用语、大小写、前缀后缀与 Unity 字段命名。注释内容见 `03-comments.md`，序列化特性要求见 `04-serialization.md`。

## 规则条目

### 通用

- **NAM-01** [人工] 命名统一使用英文。
- **NAM-02** [人工] 命名必须能直接表达职责，不得使用无意义缩写；缩写只能用于项目内已统一且无需解释的标准缩写。
- **NAM-03** [人工] 同一概念在同一项目内必须使用同一命名风格，不得同时存在两套可替换命名方式。
- **NAM-04** [机械 UCG308 / UCG503] 布尔名称必须表达真假判断：字段用 `is/has/can/should` 前缀，属性用 `Is/Has/Can/Should` 前缀。
- **NAM-05** [人工] 不得把多个职责塞进一个名称，不得为了显得简短而牺牲可读性。

### 标识符大小写

- **NAM-06** [机械 UCG102 / UCG402 / UCG502] 命名空间、类、结构体、接口、属性、方法、事件必须大驼峰。
- **NAM-07** [机械 UCG307] 私有实例字段必须 `_` + 小驼峰。
- **NAM-08** [机械 UCG306] 私有静态字段必须 `s_` + 小驼峰。
- **NAM-09** [机械 UCG305] 常量与 `static readonly` 必须大驼峰。
- **NAM-10** [人工] 局部变量、方法参数使用小驼峰。
- **NAM-11** [机械 UCG103] 接口必须 `I` 前缀。
- **NAM-12** [机械 UCG104] 抽象基类必须 `Base` 后缀。

| 标识符 | 约定 | 示例 |
|---|---|---|
| 命名空间、类、结构体、接口、属性、方法、事件 | 大驼峰 | `TeamController`、`IDamageable`、`OnDead` |
| 私有实例字段 | `_` + 小驼峰 | `private int _currentHp;` |
| 私有静态字段 | `s_` + 小驼峰 | `private static int s_totalCount;` |
| 常量 | 大驼峰 | `private const int MaxLevel = 100;` |
| `static readonly` | 大驼峰 | `private static readonly int StepMs = 16;` |
| 局部变量、方法参数 | 小驼峰 | `int remainHp = 10;` |
| 接口 | `I` 前缀 | `IDamageable` |
| 抽象基类 | `Base` 后缀 | `EnemyBase` |

### Unity 字段

- **NAM-13** [机械 UCG302] Inspector 暴露字段必须使用 `[SerializeField] private`，禁止裸 `public` 字段直接暴露给 Inspector。
- **NAM-14** [机械 UCG304] 序列化字段统一使用 `_` + 小驼峰。
- **NAM-15** [人工] 组件引用字段名称必须能直接看出用途或类型。

## 正例与反例

```csharp
// 正例
[SerializeField, Tooltip("最大血量")] private int _maxHp = 100;
[SerializeField, Tooltip("移动速度 单位 m/s")] private float _moveSpeed = 5f;
[SerializeField, Tooltip("动画控制器引用")] private Animator _anim;
private static int s_totalCount;
private const int MaxLevel = 100;
```

```csharp
// 反例
public int maxHp;                      // 违反 NAM-13：裸 public 字段（UCG302）
[SerializeField] private int maxHp;    // 违反 NAM-14：缺 _ 前缀（UCG304）
private int hp;                        // 违反 NAM-07：缺 _ 前缀（UCG307）
private static int totalCount;         // 违反 NAM-08：缺 s_ 前缀（UCG306）
private bool flag;                     // 违反 NAM-04：布尔缺语义前缀（UCG308）
```

## 例外情况

无。确需例外时按 `00-scope.md` 的 SCP-06 处理。

## 与 lint 的映射表

| UCG | 级别 | 规则条目 | 修复指引 |
|---|---|---|---|
| UCG102 | ERROR | NAM-06 | 类型名改为大驼峰 |
| UCG103 | ERROR | NAM-11 | 接口名补 `I` 前缀 |
| UCG104 | ERROR | NAM-12 | 抽象类名补 `Base` 后缀 |
| UCG305 | ERROR | NAM-09 | 常量或 `static readonly` 改大驼峰 |
| UCG306 | ERROR | NAM-08 | 私有静态字段补 `s_` 前缀 |
| UCG307 | ERROR | NAM-07 | 私有实例字段补 `_` 前缀 |
| UCG308 | ERROR | NAM-04 | 布尔字段名补 `is/has/can/should` 语义前缀 |
| UCG402 | ERROR | NAM-06 | 方法名改为大驼峰 |
| UCG502 | ERROR | NAM-06 | 属性名改为大驼峰 |
| UCG503 | ERROR | NAM-04 | 布尔属性名补 `Is/Has/Can/Should` 前缀 |

注意：修复命名类违规前，先按 `01-edit-boundary.md` 的 EDG-05 确认改名不破坏序列化与引用关系。
