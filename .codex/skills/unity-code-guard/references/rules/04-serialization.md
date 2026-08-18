# Inspector 与序列化

## 定位

约束 Inspector 暴露字段与序列化相关写法：特性要求、字段职责与 ScriptableObject 的使用边界。命名要求（`_` 小驼峰）见 `02-naming.md`，此处只管特性与职责约束。

## 规则条目

- **SER-01** [机械 UCG302] 所有需要在 Inspector 中配置的字段必须使用 `[SerializeField] private`。
- **SER-02** [机械 UCG303] 每个序列化字段必须带 `[Tooltip]`。
- **SER-03** [人工] `[Tooltip]` 必须说明字段含义与单位，不能只写同义词。
- **SER-04** [人工] 相关字段应使用 `[Header]` 分组。
- **SER-05** [人工] 数值字段应在合理时使用 `[Range]`。
- **SER-06** [人工] 一个字段只能有一个责任：要么设计期配置，要么运行时维护，不得同时承担。
- **SER-07** [人工] 运行时状态不得放入 ScriptableObject；ScriptableObject 只保存静态配置，不保存会被多实例共享改写的状态。
- **SER-08** [人工] 修改序列化字段名、类型、顺序之前，必须确认不会破坏已有资产、Prefab、场景与存档。

## 正例与反例

```csharp
// 正例
[Header("战斗")]
[SerializeField, Tooltip("最大血量")] private int _maxHp = 100;
[SerializeField, Range(0, 10), Tooltip("移动速度 单位 m/s")] private float _moveSpeed = 5f;
```

```csharp
// 反例
public int maxHp;                                     // 违反 SER-01：裸 public 字段（UCG302）
[SerializeField] private int _maxHp;                  // 违反 SER-02：缺 [Tooltip]（UCG303）
[SerializeField, Tooltip("血量")] private int _hp;    // 违反 SER-03：Tooltip 只写同义词，未说明含义与单位
[SerializeField, Tooltip("当前剩余血量")] private int _currentHp;  // 违反 SER-06：运行时状态进设计期配置
```

## 例外情况

- 纯内部调试字段（如 `FrameInputDebugger` 类调试输出）可豁免 SER-04/SER-05，但 SER-01~03 仍须满足。
- 其余无例外，确需例外时按 `00-scope.md` 的 SCP-06 处理。

## 与 lint 的映射表

| UCG | 级别 | 规则条目 | 修复指引 |
|---|---|---|---|
| UCG302 | ERROR | SER-01 | 改为 `[SerializeField] private`，或确认无需暴露时改为普通私有字段 |
| UCG303 | ERROR | SER-02 | 补 `[Tooltip]`，按 SER-03 写明含义与单位 |

注：SER-04~08（Header/Range/字段职责/SO 边界/序列化兼容）尚无机械检查，靠 AI 编辑时自查；后续可按 `rules.md` 索引中的维护规则逐步落地进 lint.csx。
