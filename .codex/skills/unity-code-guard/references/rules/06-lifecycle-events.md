# 生命周期与事件订阅

## 定位

约束 Unity 生命周期方法与事件订阅的配对关系：订阅必有退订，生命周期方法职责单一。

## 规则条目

- **LCE-01** [机械 UCG601] 事件订阅必须有对应退订，禁止只订阅不退订。
- **LCE-02** [人工] `OnEnable` 中订阅的事件必须在 `OnDisable` 中退订。
- **LCE-03** [人工] `Awake` 或 `Start` 中建立的长期订阅必须在 `OnDestroy` 中退订。
- **LCE-04** [人工] 禁止依赖对象销毁隐式清理事件订阅。
- **LCE-05** [人工] 禁止在生命周期方法中混放无关职责；生命周期方法只放与该生命周期阶段直接相关的逻辑。

## 正例与反例

```csharp
// 正例
private void OnEnable()
{
    EventBus.Subscribe<TeamSwitchEvent>(OnTeamSwitch);
}

private void OnDisable()
{
    EventBus.Unsubscribe<TeamSwitchEvent>(OnTeamSwitch);
}
```

```csharp
// 反例
private void Start()
{
    EventBus.Subscribe<TeamSwitchEvent>(OnTeamSwitch);  // 违反 LCE-01/03：无对应退订（UCG601）
}

private void OnDestroy()
{
    EventBus.Subscribe<DeadEvent>(OnDead);              // 违反 LCE-05：销毁期建立订阅，职责错位
}
```

## 例外情况

- 静态事件中订阅方生命周期与订阅目标一致、且由框架统一管理的场景，须显式说明后方可豁免 LCE-03（按 SCP-06 确认）。
- 其余无例外。

## 与 lint 的映射表

| UCG | 级别 | 规则条目 | 修复指引 |
|---|---|---|---|
| UCG601 | WARN | LCE-01 | 在对应生命周期方法补 `-=` 退订；WARN 允许继续，但不得新增同类风险 |

注：UCG601 为全文件级 `+=`/`-=` 配对扫描，无法区分订阅位置与退订方法是否正确配对（LCE-02/03 的精确配对靠 AI 自查）。
