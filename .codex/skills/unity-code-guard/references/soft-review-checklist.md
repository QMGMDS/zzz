# 软复核清单

软复核只用于机械检查之后，不替代 `lint.csx`

## 命名

- 名称是否能直接表达单一职责
- 同一概念是否始终使用同一术语
- 布尔名称是否能直接读出真假判断
- 是否存在无意义缩写、临时别名或多套命名并存

## 注释

- 注释是否只保留必要信息
- 继承或重写成员是否优先使用 `/// <inheritdoc />`，避免重复 summary
- XML 注释是否与当前代码一致
- 是否存在过期、矛盾或误导性注释
- 是否存在解释显而易见逻辑的注释

## Inspector

- 序列化字段是否只承担配置职责
- Tooltip 是否明确说明含义与单位
- 字段是否按 Inspector 的阅读顺序排列
- ScriptableObject 是否只保存静态配置

## 生命周期与事件

- 事件订阅是否存在对应退订
- 退订是否位于可靠的生命周期节点
- `OnEnable` 与 `OnDisable` 是否成对表达启停关系
- `Awake` 与 `OnDestroy` 是否成对表达资源生命周期

## 代码风格一致性

- `using` 是否按项目约定排序：`System` -> `UnityEngine / UnityEditor` -> `第三方库` -> `项目命名空间`
- 同一文件内的 `using` 分组是否保持一致
- 不同分组之间是否保留空行
- 是否存在与项目约定不一致的局部格式写法

```csharp
using System;
// 不同分组之间保留空行
using UnityEngine;

using DG.Tweening;

using SPCamera.Contract;
using SPTeam.Contract;
```

## 软复核结论

- 以上条目通过：允许继续
- 存在轻微不一致：记录并优先修正
- 存在会扩大风险的问题：停止并回到机械检查或用户确认
