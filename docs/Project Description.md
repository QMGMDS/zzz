# Project Description — Combat System

> 尝试复刻《绝区零》战斗系统 | Unity 2022.3.62f3 | URP 14.0.12 | New Input System 1.14.2

---

## 核心设计原则

- **面向接口解耦**：`IState` / `IStateContext` / `IStateMachine` / `IInputable` 全部基于接口通信，MonoBehaviour 仅作入口，不依赖具体实现。
- **C# 状态机驱动动画**：不用 Animator 的 Transition/Parameter 做逻辑判断（能走纯动画过渡的除外），核心逻辑由状态机 C# 代码决策，最后通过 `CrossFadeInFixedTime` 发指令给 Animator。
- **Root Motion 移动**：`CharacterController.Move(animator.deltaPosition)` 驱动角色位移，不手动计算移动向量。

---

## 代码分层架构

```
Assets\_Scripts\
├── Core\Input\                   ← 输入抽象层
├── GamePlay\
│   ├── Common\                   ← 动画参数哈希常量
│   ├── CustomCameras\            ← 摄像机扩展行为
│   ├── Player\                   ← 玩家入口 + 调试 UI
│   ├── State\                    ← 具体状态类（Idle / Walk / Run / Evade / Attack）
│   └── StateMachine\             ← 状态机基类 + 移动状态机
```

> 命名空间 = 文件路径去掉 `_Scripts` 前缀，如 `Core.Input`、`GamePlay.StateMachine`

---

## 一、功能 ↔ 代码索引

### 1. 玩家输入

| 涉及内容 | 代码路径 |
|----------|----------|
| 输入接口定义（Move/Evade/Attack 事件） | `Assets\_Scripts\Core\Input\IInputable.cs` |
| 输入控制器实现（封装 New Input System） | `Assets\_Scripts\Core\Input\InputController.cs` |
| InputAction 自动生成代码 | `Assets\Settings\Input\Input System.cs` |
| InputAction 配置文件（可编辑） | `Assets\Settings\Input\Input System.inputactions` |

**按键绑定**：
| 操作 | 按键 |
|------|------|
| Move | WASD（2D Vector） |
| CameraLook | 鼠标指针 Delta（暂未使用） |
| Evade | 鼠标右键 / 左 Shift |
| Attack | 鼠标左键 |

### 2. 状态机系统

| 涉及内容 | 代码路径 |
|----------|----------|
| 状态接口（Enter/Exit/Update/LateUpdate/PhysicsUpdate） | `Assets\_Scripts\GamePlay\State\IState.cs` |
| 状态上下文接口（提供 Animator/Transform/输入等依赖） | `Assets\_Scripts\GamePlay\State\IStateContext.cs` |
| 状态机接口（ChangeState / ReenterState / CurrentStateType） | `Assets\_Scripts\GamePlay\StateMachine\IStateMachine.cs` |
| 状态机基类（状态注册/切换/生命周期驱动） | `Assets\_Scripts\GamePlay\StateMachine\StateMachine.cs` |
| 移动状态机（Evade 输入 + CD + 攻击输入处理） | `Assets\_Scripts\GamePlay\StateMachine\MovementStateMachine.cs` |

### 3. 具体状态实现

| 状态 | 代码路径 | 功能摘要 |
|------|----------|----------|
| Idle | `Assets\_Scripts\GamePlay\State\IdleState.cs` | 待机，检测输入 → Walk；LateUpdate 锁定旋转 |
| Walk | `Assets\_Scripts\GamePlay\State\WalkState.cs` | 三阶段（Entering / Walking / Stopping），含长短按判定、RunEnd 过渡 |
| Run | `Assets\_Scripts\GamePlay\State\RunState.cs` | 仅由 Evade + 方向触发进入；松手 → RunEnd → Idle；LateUpdate 相机朝向旋转 |
| EvadeFront | `Assets\_Scripts\GamePlay\State\EvadeFrontState.cs` | 前闪避，CD 结束有方向 → Run，无方向等动画播完 → Idle |
| EvadeBack | `Assets\_Scripts\GamePlay\State\EvadeBackState.cs` | 后撤步，CD 结束有方向 → Walk，无方向等动画播完 → Idle |
| NormalAttack | `Assets\_Scripts\GamePlay\State\NormalAttackState.cs` | 四段连击，Playing→ComboWindow→Ending 三阶段，窗口内按攻击进下一段 |

### 4. 玩家控制器（MonoBehaviour 入口）

| 涉及内容 | 代码路径 |
|----------|----------|
| PlayerController（实现 IStateContext、持有 MovementStateMachine） | `Assets\_Scripts\GamePlay\Player\PlayerController.cs` |
| 调试 UI（C# 状态 / Animator 状态 / 输入信息） | `Assets\_Scripts\GamePlay\Player\PlayerDebugDisplay.cs` |

**PlayerController 关键结构**：
- `Awake`：创建 `MovementStateMachine`，初始化为 `IdleState`
- `OnEnable/OnDisable`：订阅/取消 `InputController` 事件
- `Update` / `LateUpdate`：驱动状态机
- `OnAnimatorMove`：应用 Root Motion 位移到 `CharacterController`
- 实现 `IStateContext` 的全部属性，为各状态提供 Animator、Transform、Camera、输入、CD 参数等

### 5. 动画系统

| 涉及内容 | 路径 |
|----------|------|
| 动画参数哈希常量 | `Assets\_Scripts\GamePlay\Common\AnimationHashes.cs` |
| Animator Controller | `Assets\Animators\Anbi.controller` |
| 角色模型 | `Assets\Art\Character\安比\Source Model\安比.fbx` |
| 基础动画 (Idle/Walk/Run/Evade) | `Assets\Art\Character\安比\Animation Clips\Base Clips\` |
| Walk/Run 起步动画 | `Assets\Art\Character\安比\Animation Clips\WalkStart Clips\` |
| 攻击动画（普攻1-4 / 分支 / 受击 / 招架 | `Assets\Art\Character\安比\Animation Clips\Attack Clips\` |
| 角色材质 | `Assets\Art\Character\安比\Materials\`（髮/体/武器/颜 各 1 个） |
| 角色贴图 | `Assets\Art\Character\安比\Textures\`（髮/体/武器/颜 各 1 个 .tga） |

### 6. 摄像机

| 涉及内容 | 代码路径 |
|----------|----------|
| 根据 POV 仰角动态调距（环视底部拉近、顶部推远） | `Assets\_Scripts\GamePlay\CustomCameras\CameraOrbitDistance.cs` |

### 7. CD / 硬直 / 缓冲系统

全部参数集中在 `PlayerController` 的 Inspector 字段中，通过 `IStateContext` 暴露：

| 参数 | 默认值 | 用途 |
|------|--------|------|
| `_evadeFrontCooldown` | 0.3s | 前闪避 CD，CD 内不可被打断 |
| `_evadeBackCooldown` | 0.7s | 后撤步 CD |
| `_comboWindowDuration` | 0.6s | 连击窗口，动画结束后该时间内再按攻击进入下一段 |
| `_inputBufferTime` | 0.05s | 输入缓冲，方向快速切换时防止误判停止 |
| `_rootMotionScale` | 1.0 | Root Motion 位移缩放 |

---

## 二、场景与预制体

| 资源 | 路径 | 说明 |
|------|------|------|
| 主场景 | `Assets\Scenes\SampleScene.unity` | 游戏运行场景 |
| 天空 Demo | `Assets\Export Asset\SimpleSky\Scenes\Demo.unity` | SimpleSky 演示（独立） |
| 角色 Prefab | `Assets\Prefab\安比.prefab` | 挂载 PlayerController + InputController + Animator + CharacterController |

---

## 三、状态流转图

```
                    ┌──────────────┐
         松手/动画完 │              │ 有输入
       ┌─────────── │     Idle     │ ───────────┐
       ▼            │              │            ▼
┌──────────┐        └──────────────┘      ┌──────────┐
│ EvadeBack│◄─── Evade（无方向）          │   Walk   │
│ (CD 0.7s)│                              │ (三阶段)  │
└────┬─────┘                              └──────────┘
     │ CD结束+有方向 → Walk                    ▲
     │ CD结束+无方向 → Idle                    │ 松手→RunEnd→Idle
     │                                         │
┌────┴─────┐     CD结束+有方向 → Run     ┌──────────┐
│EvadeFront │ ──────────────────────────► │   Run    │
│ (CD 0.3s) │                             │ (Evade触发)│
└────┬─────┘     CD结束+无方向 → Idle     └──────────┘
     │                                         │
     └─── Evade+有方向（可原地重刷动画）        │ 松手→RunEnd→Idle
                                               │
                  ┌──────────────┐              │
        攻击输入  │              │              │
      ─────────► │NormalAttack  │ ◄─────────────┘
                 │ (四段连击)    │   连击窗口内输入攻击 → 下一段
                 └──────┬───────┘   窗口超时/End播完 → Idle
                        │
                        └── 手动移动可在 Attack 期间打断？(当前不允许)
```

---

## 四、第三方资产

| 资产 | 路径 | 用途 |
|------|------|------|
| NiloToonURP 卡通着色器 | `Assets\Export Asset\UnityURPToonLitShaderExample-master\` | 角色卡通渲染着色器 |
| SimpleSky 天空系统 | `Assets\Export Asset\SimpleSky\` | 动态天空盒（Offset UV 换昼夜） |
| Cinemachine | (Package Manager) | 摄像机跟随与 POV 旋转 |

---

## 五、URP 画质档位

`Assets\Settings\URP\` 下 7 档画质（Very Low → Ultra），每档含 `*_PipelineAsset` + `*_ForwardRenderer` 两个 asset 文件。

---

## AI 使用指南

修改某功能时，按以下路径定位代码：

| 用户说… | 先看这个文件 |
|----------|---------------|
| 改按键 / 改输入 | `IInputable.cs` → `InputController.cs` → `Input System.inputactions` |
| 加新状态 | `IState.cs` → 参考一个具体 State → `MovementStateMachine.cs` 注册 |
| 改闪避逻辑 | `EvadeFrontState.cs` + `EvadeBackState.cs` + `MovementStateMachine.cs` 的 Update() |
| 改攻击 / 连击 | `NormalAttackState.cs` + `AnimationHashes.cs` |
| 改移动 / 转向 | `WalkState.cs` / `RunState.cs` 的 LateUpdate()（旋转代码在此） |
| 改 CD / 硬直 / 缓冲 | `PlayerController.cs` 的 SerializeField + `IStateContext` 暴露的属性 |
| 改 Root Motion | `PlayerController.cs` 的 `OnAnimatorMove()` |
| 改摄像机 | `CameraOrbitDistance.cs` |
| 改动画名 / 衔接 | `AnimationHashes.cs` + `Anbi.controller` |
| 改调试 UI | `PlayerDebugDisplay.cs` |
| 加角色 | `Players/` 目录 + 对应 Animator Controller |
