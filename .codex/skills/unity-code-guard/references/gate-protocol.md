# 硬门禁执行协议

## 目标

让 AI 在继续工作前先获得机械检查结果，再根据结果决定是否继续、修复或停止

## 执行顺序

1. 运行预检查
2. 读取输出
3. 判断是否继续
4. 执行任务内修改
5. 运行复检
6. 只有复检无 `ERROR` 才交付

## 命令

```powershell
& <skill-root>\tools\lint\run-guard.ps1 <project-root>
```

只检查指定文件：

```powershell
& <skill-root>\tools\lint\run-guard.ps1 <project-root> --files Assets\_Scripts\Module\UI\PlayerHUD_Controller.cs Assets\_Scripts\Tools\RuntimeInitShim.cs
```

把警告也当失败：

```powershell
& <skill-root>\tools\lint\run-guard.ps1 <project-root> --fail-on-warn
```

## 扫描范围

- 默认只扫描 `<project-root>/Assets/_Scripts` 下的 `.cs` 文件，其他目录不扫描
- 使用 `--files` 时只检查指定文件，接口信息仍从 `_Scripts` 全量收集
- 第三方插件与资源目录不在扫描范围内

## 输出处理

- `PASS`：继续工作
- `ERROR`：必须修复或停止
- `WARN`：允许继续，但不得新增同类风险

## 基线规则

- 如果预检查已有错误，先判断是否在本次任务范围内
- 任务范围内的错误必须修复
- 任务范围外的错误记为基线，不得扩大
- 复检结果不得比预检查更差

## 禁止行为

- 禁止未运行机械检查就继续修改 C# 文件
- 禁止只读规则文件不跑脚本
- 禁止把脚本失败解释为通过
- 禁止为了通过检查而删除有效代码、降级命名或移除必要注释
