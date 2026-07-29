# AGENTS.md

请使用中文写提案和回答 这个文件为 OpenCode 提供指导，用于处理此代码库中的代码。

## 编码规范

### 命名

- **私有成员**：`_camelCase`（下划线前缀），例如 `_currentState`。
- **局部变量**：`camelCase`，例如 `temp`。
- **其它所有**：`PascalCase`（类名、方法名、常量名、属性、枚举、事件）。

- **接口**：`I` 前缀；泛型：`T` 前缀。

### 注释

注释要求简明扼要，不引用脚本名。
不使用 "——"，使用 " - "。例如 : "玩家角色控制器 - Root MonoBehaviour 驱动源。"

- **类**：提供 `<summary>`，说明职责。
- **公有/保护方法**：三行 XML 注释（`<summary>` + 每个参数的 `<param>` + `<returns>`）。
- **接口方法/属性**：单行 XML 注释。

- **继承过来的 接口方法/父类方法**：`/// <inheritdoc />` 指明该方法是从父类或者接口继承而来的即可。

### 其它

- Inspector 字段必须带 `[Tooltip]`。
- **按契约式编程，只在入口校验并抛异常，函数内部禁止重复判空，保持逻辑纯净。**
    参数自身为 null           -> ArgumentNullException
    参数值或参数内容不合法     -> ArgumentException
    Inspector/对象内部状态错误 -> InvalidOperationException
    索引或数值超出允许范围     -> ArgumentOutOfRangeException

## 交互约定

- 向用户输出的内容必须是简体中文。
- 除非用户说明，否则不要直接恢复已删除文件。
- 当前 UnitySkills 服务器（localhost:8090）已启动。
