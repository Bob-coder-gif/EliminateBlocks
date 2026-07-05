# 消消乐 Demo（Match-3）

基于 Unity + C# 实现的消消乐（三消）游戏 Demo，糖果卡通像素风格。实现了从核心玩法逻辑到计分、特殊消除、死局处理、胜负结算的完整闭环，代码按职责分模块组织。

## 环境

- Unity 2022.3.62（其它 2022.3.x 版本应也可运行）
- 语言：C#
- 开发平台：Windows + Visual Studio
- 目标平台：Android（已验证打包）；iOS 需 Mac + Xcode，未打包

## 玩法功能

- 8 向以外的相邻块交换（点选两个相邻块交换），无效交换自动弹回
- 横向 / 纵向三连及以上消除
- 特殊形状消除（见下）
- 消除后上方块下落、顶部补充新块
- 连锁反应（连击）
- 计分、步数限制、目标分数与胜负结算
- 死局检测：无可消除步时自动洗牌至可玩
- 消除粒子特效
- 竖屏摄像机自适应、背景铺满、UI 安全区适配（避开圆角 / 刘海）

## 特殊消除规则

| 形状 | 效果 |
| --- | --- |
| 横向 4 连 | 消除所在整行 |
| 横向 ≥5 连 | 消除所在行及上下各一行（共三行） |
| 纵向 4 连 | 消除所在整列 |
| 纵向 ≥5 连 | 消除所在列及左右各一列（共三列） |
| T / L / + 形 | 消除交汇块所在的一行 + 一列，交汇块为中心 |

多个条件同时命中时取并集，不递归（整行 / 整列扫出的其它连不再触发各自特效；下落后形成的新匹配算作下一轮连击）。

## 计分规则

单轮基础分：

```
基础分 = (检测到的连数 - 1) * 5 + 本轮实际消除总数
```

一次交换的总得分（连击分只在连锁全部结束后按最终连击数加一次，避免每轮重复累计）：

```
总分 = 各轮基础分之和 + 5 * 连击数
```

- “检测到的连数”为本轮所有匹配段长度之和
- “实际消除总数”含被整行 / 整列带走的块（去重）
- “连击数”为本次交换引发的连锁轮数

## 胜负结算

- 有效步：只有产生消除的交换才计一步（无效交换不计）
- 达到目标分数即判胜利；用完步数仍未达标判失败
- 任一条件满足即结算：停止输入，显示胜负与最终得分，提供“重新开始”按钮
- 默认目标 5000 分 / 30 步，可在 GameBoard 组件的 Inspector 中调整

## 项目结构

脚本位于 `Assets/Script/Match3/`，按职责分模块：

```
Match3/
├── Core/
│   ├── BoardGrid.cs      棋盘数据（二维格子 + 坐标换算）
│   ├── Tile.cs           单个块（平滑移动、类型 / 高亮）
│   └── GameBoard.cs      总指挥（组装子系统、协程调度、计分结算、UI）
├── Spawn/
│   ├── TileFactory.cs    运行时生成块
│   ├── BoardSetup.cs     开局铺盘（不出现初始三连）
│   └── Refiller.cs       顶部补充新块
├── Input/
│   └── InputHandler.cs   点击选中与相邻交换请求（事件解耦）
├── Match/
│   ├── MatchFinder.cs    找出所有连续段（run）
│   ├── MoveValidator.cs  死局检测（是否还有可走步）
│   └── ClearResolver.cs  按形状规则计算最终消除范围
├── Move/
│   ├── TileSwapper.cs    交换两块、相邻判断
│   ├── Gravity.cs        下落填空
│   └── Shuffler.cs       死局洗牌（Fisher–Yates）
├── Delete/
│   └── TileClearer.cs    清除、销毁块、生成消除特效
└── Score/
    └── ScoreManager.cs   计分、步数、目标与胜负判断
```

数据统一存放在 `BoardGrid`，`GameBoard` 作为唯一 MonoBehaviour 组装并调度其余纯逻辑类。Unity 中文件夹不参与编译，各类靠 `Match3` 命名空间互相引用。

## 运行方式

1. 用 Unity 打开项目。
2. 场景中新建空 GameObject，挂上 `GameBoard` 脚本（其余类由它在代码中实例化，无需手动挂载）。
3. 在 Inspector 中配置：
   - `Candy Sprites`：糖果 Sprite 数组（建议 5~6 种）
   - `Background`：背景的 SpriteRenderer（可选）
   - `Clear Effect Prefab`：消除粒子特效预制体（可选，留空则无特效）
   - `Width` / `Height`：棋盘尺寸（默认 7 × 9）
   - `Target Score` / `Max Steps`：关卡目标
4. 运行，点击两个相邻块进行交换。

素材导入要点（像素风）：Sprite 的 `Filter Mode` 设为 `Point (no filter)`，`Compression` 设为 `None`，`Pixels Per Unit` 与素材像素尺寸一致（示例为 64）。

## 打包 Android

1. Unity Hub 安装 Android Build Support（含 SDK & NDK、OpenJDK）。
2. File → Build Settings → 选 Android → Switch Platform。
3. Player Settings 设置唯一的 Package Name，Default Orientation 设为 Portrait。
4. 将游戏场景加入 Scenes In Build。
5. Build（生成 APK）或 Build And Run（USB 连真机直接安装运行）。

## 已知限制 / 待完善

- UI 使用 IMGUI（`OnGUI`）临时实现，未做正式 UI；正式版建议改用 UGUI（Canvas + Canvas Scaler）以适配各机型。
- 特殊消除目前仅有粒子效果，无直线光效等专门的打击表现。
- 补充新块为纯随机，未做出块权重或“保证有解”之外的调控。
- 特殊消除不递归触发（设计如此）。
- 未做存档、关卡系统、音效。
