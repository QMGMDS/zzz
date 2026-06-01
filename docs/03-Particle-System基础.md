# Particle System 基础

## 一、什么是 Particle System

**Particle System（粒子系统）** 是 Unity 内置的、最基础也最常用的特效系统。它通过**发射并控制大量微小粒子（Particles）** 来模拟火、烟、爆炸、雨、雪、魔法等视觉效果。

### 核心概念

一个粒子系统 = 一台"粒子发射器"。每个粒子独立的属性：

```
位置（Position）  →  每一帧都会变化
速度（Velocity）  →  方向和速率
大小（Size）      →  可以随时间变化
颜色（Color）     →  可以随时间变化（含透明度）
旋转（Rotation）  →  可以随时间变化
寿命（Lifetime）  →  粒子从出生到消亡的时间
```

### 粒子的生命周期

```
发射（Spawn） → 更新（Update） → 消亡（Death）
    │              │                │
  出生位置       每一帧移动/        粒子消失
  初始大小       变化颜色/           可触发子发射器
  初始颜色       变化大小/
  初始速度       变化旋转/
```

### Unity Particle System 的架构

Unity 的粒子系统采用了**模块化**设计。打开 Inspector，你会看到一串模块开关：

```
Particle System
├── Main              ← 全局参数（生命周期、速度、大小等等）
├── Emission          ← 发射速率、爆发
├── Shape             ← 发射形状
├── Velocity over Lifetime   ← 速度变化
├── Color over Lifetime      ← 颜色渐变
├── Size over Lifetime       ← 大小变化
├── Rotation over Lifetime   ← 旋转变化
├── Noise             ← 随机扰动
├── Trails            ← 拖尾
├── Sub Emitters      ← 子发射器
├── ...               ← 更多模块
└── Renderer          ← 渲染方式
```

> **核心思路**：把模块看作"滤镜"或"效果器"——**勾选一个模块，就多一层控制**。不用的模块关掉以节省性能。

---

## 二、快速上手

### 创建第一个粒子系统

1. **Hierarchy 右键 → Effects → Particle System**
2. 你会看到一个默认的粒子喷射效果
3. 选中它，看 Inspector 中的 `Particle System` 组件

### 立刻试试

| 操作 | 效果 |
|------|------|
| 把 `Main → Start Speed` 从 5 改成 20 | 粒子射得更远 |
| 把 `Main → Start Size` 从 1 改成 0.3 | 粒子变小 |
| 在 `Emission` 里点 `+` 添加一个 Burst | 点击一次爆发一堆粒子 |

> **任何时候按空格**可以暂停/继续播放 Scene 视图中的粒子效果。

---

## 三、核心模块详解

### 3.1 Main 模块

粒子系统的**心脏**，控制全局参数。

| 参数 | 作用 | 练手 |
|------|------|------|
| **Duration** | 一次播放持续时长（秒） | 改成 2，看粒子 2 秒后停 |
| **Looping** | 是否循环播放 | 关了，粒子只播一次 |
| **Start Lifetime** | 每个粒子的寿命（秒） | 改成 10，粒子活更久 |
| **Start Speed** | 粒子的初始速度 | 改成 0，粒子原地不动 |
| **Start Size** | 粒子的初始大小 | 改成 0.1 → 2（随机区间） |
| **Start Color** | 粒子的初始颜色 | 改成红色，粒子变红了 |
| **Gravity Modifier** | 重力影响（0=无，1=正常） | 改成 1，粒子会下落 |
| **Simulation Space** | **Local**=随物体移动 / **World**=世界坐标独立 | 切换看区别 |

> **小技巧**：Start Lifetime / Speed / Size 右边的下拉菜单可以选 `Random Between Two Constants`，让粒子在区间内随机取值，效果更自然。

---

### 3.2 Emission 模块

控制**什么时候发射、发射多少**。

| 参数 | 作用 | 练手 |
|------|------|------|
| **Rate over Time** | 每秒发射多少粒子 | 改成 200，粒子变密 |
| **Rate over Distance** | 每移动单位距离发射多少粒子 | 配合移动物体用 |
| **Bursts** | 在指定时间"爆发"大量粒子 | 点 `+`，Time=0，Count=50，瞬间爆 50 个 |

> **Burst 是做爆炸效果的关键**。

---

### 3.3 Shape 模块

决定粒子从**什么形状的区域**发射出来。

| 形状 | 效果 | 典型用途 |
|------|------|---------|
| **Cone**（锥体） | 从锥体喷出 | **火焰**、喷泉 |
| **Sphere**（球体） | 从球体表面/内部发出 | **爆炸**、烟雾 |
| **Box**（盒子） | 从长方体区域发出 | 下雨、碎片 |
| **Circle**（圆形） | 从平面圆形发出 | **光环**、魔法阵 |
| **Mesh**（网格） | 从模型表面发出 | 物体燃烧、碎裂 |
| **Cone + Angle=0** | 变成"圆柱"射流 | 激光、光束 |

> **练手**：Shape 从 Cone 改成 Sphere，粒子从"喷出"变成"向四面八方炸开"——立刻就是爆炸效果的基础。

---

### 3.4 Renderer 模块

决定粒子**长什么样、怎么渲染**。

#### 核心参数

| 参数 | 作用 |
|------|------|
| **Render Mode** | Billboard（始终面向相机）、Mesh（3D 模型）、Stretch（拉伸） |
| **Material** | 用什么材质渲染粒子 |
| **Sort Mode** | By Distance（远到近）、Youngest First（最新的在前面） |
| **Sorting Fudge** | 调整与其它透明物体的排序 |

#### 混合模式（Blend Mode）—— 极其重要

混合模式通过 **Material** 来控制。Unity 内置的粒子材质模板：

| 材质名 | 模式 | 效果 | 适用场景 |
|--------|------|------|---------|
| `Particles/Standard Unlit` | 可选 Additive / Alpha Blended | 见下 | 通用 |
| `Particles/Additive` | **Additive** | 重叠部分会叠加变亮，有发光感 | **火焰、光效、魔法** |
| `Particles/Alpha Blended` | **Alpha Blended** | 半透明叠加，有层次感 | **烟雾、雾气、水花** |
| `Particles/Multiply` | Multiply | 重叠部分变暗 | 暗色特效、阴影 |

> **Additive vs Alpha Blended 核心区别**：
> - Additive：黑色部分完全透明，白色部分发光，颜色叠加会变**亮**
> - Alpha Blended：按透明度混合，颜色叠加不会变亮，适合有厚度的半透明效果
>
> **火焰用 Additive，烟雾用 Alpha Blended，记住这一条就够了。**

---

### 3.5 Color over Lifetime

粒子颜色（含透明度）随时间变化。

- 打开该模块，点击 **Color** 的渐变条
- 底部色标控制颜色，顶部色标控制透明度（Alpha）

**练手**：做一个"出生时亮 → 逐渐变暗 → 消失"的效果：
```
Time=0%   颜色=亮橙黄   Alpha=255
Time=50%  颜色=暗红     Alpha=200
Time=100% 颜色=黑       Alpha=0
```
这就是火焰的基础配色。

---

### 3.6 Size over Lifetime

粒子大小随时间变化。

- 打开模块，选择一个曲线预设
- 常用模式：

```
[从0变大 → 维持 → 缩小到0]    ← 出生和死亡都平滑
[从最大 → 缩小到0]            ← 只缩小（爆炸碎片）
[从0 → 变到最大]              ← 只放大（烟雾膨胀）
```

> **练手**：对默认粒子，把 Size over Lifetime 改成"从 0 到 1 再到 0"的曲线，粒子的生命周期变得有"节奏感"了。

---

### 3.7 Velocity over Lifetime

粒子速度的**额外控制**，叠加在初始速度之上。

| 参数 | 作用 | 练手 |
|------|------|------|
| **Linear** | 三个轴向的额外速度 | X=5，粒子会向右飘 |
| **Orbital** | 绕中心做圆周运动 | Z=90，粒子绕 Z 轴转圈 |
| **Radial** | 径向速度（往外拉或往里拉） | 负值 = 粒子被吸回中心 |

> **Orbital 是实现"环绕魔法球"效果的关键**。

---

### 3.8 Noise 模块

给粒子运动添加**随机扰动**，让效果更自然、不呆板。

| 参数 | 作用 |
|------|------|
| **Strength** | 扰动强度 |
| **Frequency** | 扰动的频率（越小越平滑，越大越碎） |
| **Scroll Speed** | 扰动随时间的变化速度 |
| **Octaves** | 细节层数（越高越丰富，越耗性能） |

> **练手**：做一个默认 Cone 粒子，打开 Noise，Strength=1，Frequency=0.3——粒子立马从"笔直射出去"变成"像火焰一样飘忽不定"。**火焰效果的灵魂就在这里**。

---

### 3.9 Sub Emitters 模块

在粒子的**特定时刻**触发另一个粒子系统。

| 触发时机 | 说明 |
|---------|------|
| **Birth** | 粒子出生时触发子发射器 |
| **Death** | 粒子死亡时触发子发射器 |
| **Collision** | 粒子碰撞时触发子发射器 |

> 典型用法：爆炸粒子消亡时触发烟雾子发射器，**一个系统做爆炸，一个系统做烟雾**，分开控制更灵活。

---

### 3.10 Trails 模块

为粒子添加**拖尾**效果。

| 参数 | 作用 |
|------|------|
| **Lifetime** | 拖尾持续时长 |
| **Minimum Vertex Distance** | 拖尾顶点的最小间距 |
| **Material** | 拖尾材质 |
| **Color over Lifetime** | 拖尾颜色随时间的渐变 |
| **Die with Particles** | 粒子死亡时拖尾是否立即消失 |

> **练手**：增加 Start Speed，打开 Trails，设 Lifetime=0.5——粒子变成"流星"带着尾巴飞过。

---

## 四、练手案例

### 案例 1：火焰

**目标**：一个持续燃烧的火焰

**步骤**：

| 模块 | 设置 |
|------|------|
| Main | Start Lifetime=1.5~2.5，Start Speed=1~3，Start Size=0.5~1.5 |
| Emission | Rate=30~50 |
| Shape | Cone，Angle=10~15，Radius=0.3 |
| Renderer | Material=`Particles/Additive`（用自带的 Additive 材质或新建） |
| Color over Lifetime | 0%(白黄, A=255) → 50%(橙, A=200) → 100%(暗红, A=0) |
| Size over Lifetime | 从 0.3 逐渐变大到 1.5，然后缩到 0 |
| Noise ✅ | Strength=0.5~1，Frequency=0.3~0.5 |

**效果**：持续的火焰喷射，粒子飘忽不定，出生时亮白 → 变橙 → 变暗消散。

> **进阶**：叠加第二层粒子系统，用更小的粒子、更快的速度、纯黄色，做"火芯"燃烧最亮的部分。

---

### 案例 2：爆炸

**目标**：一个瞬间爆发的爆炸效果

**步骤**：

| 模块 | 设置 |
|------|------|
| Main | Looping=❌，Start Lifetime=0.5~1.5，Start Speed=5~15，Start Size=0.2~0.8 |
| Emission | Rate=0，添加 Burst：Time=0，Count=50~100 |
| Shape | Sphere，Radius=0.3 |
| Renderer | Material=`Particles/Additive` |
| Color over Lifetime | 0%(白黄, A=255) → 100%(红, A=0) |
| Size over Lifetime | 从 1 逐渐缩到 0 |
| Velocity over Lifetime | Radial= -2 ~ -5（让粒子向外扩撒更快） |

**效果**：点击 Play，所有粒子瞬间向四面八方炸开，快速变大后缩小消散。

> **进阶**：叠加第二层 Particle System 用 Alpha Blended 材质做烟雾，Start Delay 延迟 0.1 秒触发，用 Burst 发射少一些的大粒子，从白变灰变透明。

---

### 案例 3：魔法光球

**目标**：一个环绕旋转的发光魔法球

**步骤**：

| 模块 | 设置 |
|------|------|
| Main | Start Lifetime=3~5，Start Speed=0，Start Size=0.2~0.4 |
| Emission | Rate=20~30 |
| Shape | Circle，Radius=1，Arc=360 |
| Renderer | Material=`Particles/Additive`，用圆形发光 Sprite 纹理 |
| Color over Lifetime | 蓝紫渐变，Alpha 出生时低 → 最高 → 消亡时低 |
| Size over Lifetime | 从 0.5 → 1.5 → 0.5 脉动 |
| Velocity over Lifetime | Orbital Z=60~120（绕 Z 轴旋转） |

**效果**：粒子在大约 1 秒后就会形成一个环绕的光环，持续旋转。

> **进阶**：中间再加一个 Particle System 做核心光球，用 Start Speed=0，Size 变化+发光颜色，加上 Noise 让光球微微浮动。

---

## 五、常见技巧与坑

### ✅ 技巧

| 技巧 | 说明 |
|------|------|
| **多层叠加 > 单层复杂** | 2~3 个简单的粒子系统叠加，效果远好于一个超复杂的系统 |
| **先调形状和运动，再调颜色** | 容易犯的错误：一上来就调颜色，结果运动不对全部重来 |
| **多 Start 用区间随机** | 永远不要用单一固定值，用 `Random Between Two Constants` |
| **曲线比固定值灵活** | 习惯把 Size/Lifetime 的数值改成 `Curve`，微调空间更大 |
| **Prefab 保存** | 做好一个特效，拖到 Project 窗口存为 Prefab |
| **Prewarm** | 循环粒子勾选 Prewarm，进入场景时粒子系统已经是"正在运行"的状态 |
| **粒子对帧率不敏感** | 粒子系统不受帧率影响，100fps 和 30fps 表现一致 |

### ❌ 常见坑

| 坑 | 原因 | 解决 |
|----|------|------|
| 粒子突然消失 | Lifetime 耗尽 | 增大 Start Lifetime |
| 粒子穿透地面看不见 | 粒子跑到相机背后 | 检查 Renderer→Sorting Fudge，或改 Simulation Space=World |
| Scene 看得到，Game 看不到 | Renderer 的 Order in Layer 不对 | 调整 Renderer 排序 |
| 粒子是方形/黑块 | 没给合适的材质/纹理 | 给一个带 Sprite 的粒子材质 |
| 粒子不动/绕圈 | Start Speed=0 且没加其他速度 | 加 Velocity over Lifetime 或调高 Start Speed |

---

## 小结

```
1. Main 先调全局     ← 寿命、速度、大小、颜色
2. Emission 定密度   ← Rate 还是 Burst
3. Shape 定方向      ← 从哪出、朝哪散
4. Renderer 定材质   ← Additive / Alpha Blended
5. Color/Size 定节奏 ← Curve 控制变化
6. Noise 定自然度   ← 让效果"活"起来
7. 叠加多层          ← 复杂效果 = 多个简单系统
```

> **Particle System 是你做特效的"瑞士军刀"**。别想着一个参数搞定一切，关键是理解每个模块**控制什么维度**，然后**组合使用**。
