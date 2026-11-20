# VR 化学实验室：全面优化审查与实施路线

## 1. 审查范围与结论

本审查基于当前工作区的代码、场景、Shader、材质、模型、配置和资源，不依赖之前的描述。当前项目已经具备：两条实验流程、XR 抓取/Socket、数据驱动实验资产、步骤/归位引导、中英文、三种教学模式、音效反馈和一套移动端近似材质。

**下一阶段的主要矛盾不是“功能不够多”，而是：旧流程与新 Runner 双轨并存、验证体系不足、实时光影和模型资源成本偏高、资源目录缺少生产级整理。**

建议先完成可编译、可测试、可重复运行的基线，再做架构迁移和视觉升级。

### 当前证据快照

| 项目 | 当前情况 |
|---|---|
| 工程资源规模 | 约 1.37 GB |
| 原始字体 | 406 个 TTF/TTC，约 825.7 MB；本次主场景/TMP 源文件扫描未找到其 GUID 引用 |
| 完全重复资源 | 31 组、63 个文件，额外占用约 290.4 MiB，主要来自字体 |
| `Assets/Resources` | 约 148.9 MB；其中当前使用的天空盒约 72.2 MB，未使用天空盒目录约 76.7 MB |
| 主场景 | 364 个 GameObject、94 个 MeshRenderer、16 盏灯、63 个 MeshCollider |
| 灯光 | 16 盏中 14 盏开启阴影，多个聚光灯强度为 20，最高超过 96 |
| 代码热点 | 42 处 `GameObject.Find`、27 处 `FindObjectOfType`、71 个静态基础类型字段 |
| 材质实例 | 高亮、液体、Wobble 仍使用 `renderer.material(s)`；`MaterialPropertyBlock` 使用数为 0 |
| 新自定义 Shader | 未声明 `UnityPerMaterial`，因此不能使用 SRP Batcher |
| 模型重点 | `TestTubeRack.fbx` 约 32.5 MB、场景引用 46 次、未启用网格压缩、允许导入动画 |
| 测试 | 37 个 EditMode + 11 个 PlayMode（含 26 个化学核心和 3 个化学集成测试），未覆盖完整场景/XR/Shader；本环境未实际运行 Unity Test Runner |

## 2. 优先级总表

| 优先级 | 优化项 | 主要收益 | 成本 |
|---|---|---|---|
| P0 | 建立 Unity 编译、EditMode、PlayMode、Android 构建验证链 | 知道当前版本是否真正可用，防止“静态通过但运行失败” | 中 |
| P0 | 恢复/重建 18 个 TMP 组件使用的中文字体资产 | 解决 36 行缺失字体引用，避免中文回退、方框或空白 | 低中 |
| P0 | 处理 OutputEvent Helpers 对 HDRP/Cinemachine 的未安装引用 | 消除示例程序集导致的潜在编译阻断 | 低 |
| P0 | 新 Shader 增加 `UnityPerMaterial` 常量缓冲 | 恢复 SRP Batcher 兼容性，减少状态切换 | 低 |
| P0 | 修复 15 个非凸 MeshCollider 位于动态 Rigidbody 下的组合 | 避免 PhysX 限制导致器材碰撞失效或运行时报错 | 低中 |
| P0 | 减少实时阴影灯数量，优先处理 14 个投影灯 | 直接降低 GPU/阴影渲染开销 | 低 |
| P0 | 归档已确认的无用字体、天空盒、示例和大模型 | 降低导入时间、维护成本和潜在包体 | 低 |
| P1 | 修复加热步骤绕过、氧气归属、重复补氧和重置残留 | 保证实验条件真实有效，避免考核被直接跳过 | 中高 |
| P1 | 消除 Runner 与旧 Score 的双轨反写 | 避免状态覆盖、评分不一致和顺序绕过 | 高 |
| P1 | 建立真正统一的物理+状态重置 | 避免场景重载带来的停顿和隐式状态残留 | 中高 |
| P1 | 用 MaterialPropertyBlock/Shader 高亮替代材质实例 | 降低材质实例、GC、内存和批处理破坏 | 中 |
| P1 | 优化 `a.fbx`、试管架、洗手盆及 MeshCollider | 降低高三角模型、动画数据、重复器材和碰撞成本 | 中 |
| P1 | 建立 PlayMode 场景验收和 Shader 检查 | 覆盖现有 9 个核心测试未触及的问题 | 中 |
| P1 | 明确反应物、反应条件、收集量和安全流程 | 提升化学教学正确性，而不是只播放效果 | 中 |
| P2 | Unity/XRI/OpenXR 升级与 Quest 渲染路线 | 延长维护周期并改善设备覆盖 | 高 |
| P2 | 液体封顶、分级材质和高质量模式 | 提升近景视觉表现 | 中高 |

## 3. 代码架构

### 3.1 最高优先级：消除双轨状态源

**确定问题：** `Score.Update` 每帧调用 `CalculateScore → SyncRunnerState`，将旧静态布尔值通过 `SetStepCompletedFromLegacy` / `SetCleanupCompletedFromLegacy` 写回 Runner。旧接口绕过了 Runner 的准备和顺序守卫。若新逻辑直接调用 Runner 的受保护 API，下一帧又可能被旧值覆盖。

建议：

1. 增加唯一的 `LegacyExperimentAdapter`，只负责将旧 Socket/碰撞事件翻译为 Runner 事件，不再每帧全量反写。
2. 步骤、归位、准备、评分只保存在 Runner；旧 `Score` 字段只作为临时事件输入，不作为第二个事实源。
3. 逐步把 `OnTriggerEnter/Stay` 从对象名判断改成 `LabItem` / `LabItemType` 事件。
4. 迁移完成后删除 `SetStepCompletedFromLegacy` 等绕过接口，而不是长期保留两个写入通道。
5. 用脏标记替代每帧同步：只有步骤、位置或实验选择变化时才更新 Runner 和 UI。

**验证：** 同一帧完成步骤、切换实验、重置、丢弃火柴时，Runner、Score UI、步骤颜色和最终得分必须一致。

### 3.2 统一“重置实验”和“重置实验室”

当前 `ResetCurrentExperiment` 主要清理内存状态，`ReG` 通过重载场景恢复物理对象。两种语义没有统一到一个控制器中。

建议新增 `LaboratoryLayoutController`：

- 保存器材初始位置、旋转、缩放、父子关系和 Socket 状态。
- 重置时先退出抓取/选择，再恢复 Rigidbody 位置和速度。
- 清空药品、液体、火焰、烟雾、氧气计数、步骤和得分。
- 订阅重置的对象必须覆盖 `MakeO2`、`GetPowder`、`O2IntoBottle` 及所有延迟调用，不能只覆盖 Runner 和旧 Score。
- 销毁临时火柴并恢复初始 Prefab 实例。
- 清理高亮、提示文本、模式需要的临时 UI。
- 用一次显式 `ResetExperiment` 替代全场景重载；保留“完整重载”作为开发回退选项。

**验证：** 连续重置 20 次，不出现重复火柴、材质实例累积、Socket 悬空、旧火焰、旧氧气或静态状态残留。

### 3.3 用显式引用替代名称和全局查找

当前仍有 42 处 `GameObject.Find`、27 处 `FindObjectOfType`。`SceneObjectFinder` 在未激活对象查找失败时还会扫描更多对象。

建议：

- `LabItem` 只保留类型和运行时状态，不再用名称表达化学语义。
- `ExperimentRunner` 通过明确注册表取得器材，不通过全局 `Find`。
- UI 引用、火焰、液体、碰撞体和步骤文本使用序列化字段或 `RequireComponent`。
- 用 `LabItemRegistry` 管理动态火柴实例，避免销毁后缓存悬空。
- 避免 `GetOrCreate` 在运行时隐式创建管理对象，改为明确的场景 Bootstrap。
- `GetPowder` 的 8 份重复分支改为数组/字典和统一药品状态逻辑。

### 3.4 拆分程序集和职责

目前只有核心实验模型独立为 `VRChemistry.Experiments.Core`，大量旧脚本仍在 `Assembly-CSharp`。

建议逐步形成：

- `VRChemistry.Experiments.Core`：纯数据、状态机、评分，不引用 Unity 场景对象。
- `VRChemistry.LabItems`：器材、连接、归位和物理状态。
- `VRChemistry.Reactions`：药品、温度、反应、产物。
- `VRChemistry.XR`：XRI 适配、输入和震动。
- `VRChemistry.Presentation`：UI、材质、VFX、音效。
- `VRChemistry.Legacy`：短期兼容代码，迁移完成后删除。

同时统一命名：`Tirgger`、`Destory`、`Makeing`、`AlcoholLmap`、`Offest`、`Disslpate`、`Modle` 等需要规范化。重命名文件/目录时必须同时移动 `.meta`，不能直接改资源 GUID 或忽略 UnityEvent 方法名。

### 3.5 清理模板和重复逻辑

`ControlOffest`、`DissolveDirection`、`SimpleCameraController`、未使用的 `ReadInput`/`FrictionFire` 等模板或测试脚本应与正式逻辑隔离。建议先确认无场景引用，再移到 `Assets/Development` 或归档，避免维护者误以为它们属于实验流程。

`TestActionController` 在左右手柄各挂一份，Select 时输出 `Debug.Log`，属于发布日志残留。应删除该日志或仅在 `UNITY_EDITOR` / `DEVELOPMENT_BUILD` 下输出，避免正式版本持续产生无意义日志。

## 4. 编译、依赖与发布

### 4.1 先建立真正的编译基线

当前环境没有 Unity 编辑器，之前的检查主要是文本、YAML、GUID 和部分 API 核对，**不能替代 C# 语义编译、Shader 编译、URP 导入或 Android 构建**。

另一个需要优先处理的编译风险是：`Assets/Samples/Visual Effect Graph/10.8.1/OutputEvent Helpers` 的 Runtime 和 Editor asmdef 都无条件引用 `Unity.RenderPipelines.HighDefinition.Runtime` 与 `Cinemachine`，但项目未安装这两个包。当前场景未发现业务使用这些示例，建议优先归档未使用的 OutputEvent Helpers，而不是为了示例引入 HDRP/Cinemachine 两套额外依赖；若确实需要示例功能，再补齐对应包并验证 URP 兼容性。

建议建立可重复的验证命令：

```text
Unity.exe -batchmode -projectPath <project> -runTests -testPlatform EditMode -quit -logFile <log>
Unity.exe -batchmode -projectPath <project> -buildTarget Android -quit -logFile <build-log>
```

实际 CI 需要准备对应 Unity 版本和许可证，并保存测试 XML、Editor.log、APK 和构建报告。每次合并至少执行：导入、C# 编译、EditMode、PlayMode、Shader 编译和 Android 构建。

### 4.2 精简依赖

`Packages/manifest.json` 仍包含 ARFoundation、Timeline、Collab 和多个 IDE 包。AR 功能在当前桌面式 VR 化学实验室中没有明显用途，建议确认后移除 ARFoundation；Timeline 若未使用也移除。IDE 包主要影响开发环境，可按团队工具链保留。

`EditorBuildSettings.asset` 的 OpenXR 配置和 OpenXR Loader 已在 P2 清理并归档；当前保留 Oculus Loader。后续如迁移 OpenXR，应另开分支验证，不与 Oculus 配置混用。

- 短期继续使用 Oculus Loader，清理无用 OpenXR 残留；或
- 在独立分支迁移 OpenXR，统一 loader、输入、材质和设备测试。

不要在同一阶段同时升级 Unity、XRI、OpenXR、URP 和 Oculus 包，否则难以定位回归来源。

### 4.3 版本和发布设置

主场景有 18 个 TMP 文本组件的 `m_fontAsset/m_sharedMaterial` 共 36 行引用指向 GUID `d43e48d21e6f5b245bd50c421a0e3044`，但全工程未找到对应资产或 `.meta`。这是确定的字体资源断链，中文回退、方框或空白程度需 PlayMode 验证。应优先恢复或重建许可明确的中文字体资产，并统一重绑，而不是依赖全部系统字体目录。

当前 Unity `2020.3.30f1c1`、URP/VFX `10.8.1`、XRI `2.0.1`、Oculus `1.11.2` 都较旧。建议先在原版本完成稳定基线，再评估受支持的 Unity LTS 与新 XRI/OpenXR 兼容矩阵。

发布前还需处理：

- 自定义包名、版本号和版本代码。
- 正式签名 keystore。
- Android 目标 SDK 和商店要求。
- ARM64/IL2CPP 构建验证。
- Vulkan 与目标固件、URP/VFX 的兼容性验证。
- 安装、启动、暂停/恢复、退出和更新流程。

## 5. 测试体系

### 5.1 当前缺口

现有 9 个 EditMode 测试主要覆盖 Runner 的资产、准备、评分、重置、语言和归位，未覆盖：

- 主场景组件和材质完整性。
- Socket 的真实进入/离开。
- 火柴销毁/重建及事件转发。
- O2 双碰撞体、灯帽 hover、燃烧和耗氧。
- Shader 编译、透明排序、液体高度和双眼效果。
- UI 布局、模式切换和 XR 输入。

### 5.2 建议的测试金字塔

**纯 C# 单元测试**

- ExperimentDefinition、Runner、评分、顺序和语言。
- ReactionDefinition 的反应条件、温度、计量和产物。
- 实验室布局快照和恢复。

**PlayMode 场景测试**

- 加载主场景，检查 Runner、器材、材质、Socket 和 UI 引用。
- 完整执行制氧 7 步和 13 项归位、复燃 3 步和 5 项归位。
- 准备阶段移出器材、快速切换实验、重置、连续重建火柴。
- 双 O2 碰撞体边缘移动、烧尽、氧气耗尽、灯帽 hover。
- 提前放入集气瓶、未放药品、熄灭酒精灯和错误操作。

**视觉与 Shader 测试**

- 材质不为粉色，Console 无 Shader 错误。
- 玻璃/液体透明排序、液面、泡沫和气泡。
- 高亮前后材质颜色、alpha、法线和自发光恢复。
- 近景/远景、不同语言、不同模式和不同光照条件下的截图对比。

**性能测试**

- 记录 CPU/GPU 帧时间、Draw Call、批处理状态、透明层数、三角形、材质实例、GC Alloc 和包体。
- 对比优化前后，而不是仅写“适合 Quest”。

### 5.3 没有 Quest 时的替代方案

- 保留 F1–F5 作为状态/UI 调试，不将其视为物理实验通过。
- 使用 XRI Device Simulator 验证抓取和射线，补充明确的编辑器模拟场景。
- 在 Windows CI 执行单元测试、PlayMode 测试和独立 Player 启动。
- 用编辑器截图和 Frame Debugger 检查材质、透明排序和批处理。
- Android 模拟器不能证明 Quest 的 XR 输入、性能和视觉正确性，最终仍需设备验证。

## 6. 实验流程与化学教学

### 6.1 明确反应模型

当前制氧药品和反应模型仍然抽象，尚不是完整的反应数据模型。建议由化学教学内容负责人明确：药品、方程式、催化剂、温度、气体产量、收集方式和安全步骤，再写成 `ReactionDefinition`。

木条复燃提示中的 `O2 + C = CO2` 是简化表达，木条并非纯碳，应标注为示意而不是完整燃烧模型。

### 6.2 优先修复的确定性流程问题

- **加热条件可被绕过：** 主场景酒精灯 Socket 的 SelectEntered 直接调用 `Score.Make_O2_Step5_To_Step6`，绕开 `AlcoholLmapConDesk` 中的 `IfLampIsFire()` 判断。应由 Runner/实验条件服务统一验证“酒精灯已点燃且试管位置正确”，不能让 UnityEvent 直接写步骤完成。
- **重复补氧：** 复燃按钮最终调用 `MatchActionRelay.Start_ReFireStick`，当前会把 `O2ParticleCount` 直接设为 10000。重复点击可无限补满，考核模式也有效。应只允许在实验初始化或氧气供应重置时设置初始值，而不是每次选择按钮都补满。
- **氧气归属错误：** 氧气计数存放在火柴的 `VisualEffectGraph` 上，但收集逻辑向火柴加氧；火柴销毁/重建会清空计数。应把氧气量移到氧气瓶或独立的 `OxygenSupply`，火柴只保存燃烧/余烬状态。
- **耗尽与重新点燃语义不一致：** 氧气耗尽清除 `Can_refire`，但 `startfire` 不检查该状态，摩擦路径仍可能重新普通燃烧。需要明确“外部火源可重新点燃”和“耗尽后不能靠余烬复燃”的独立规则，并统一提示。
- **重置覆盖不完整：** `MakeO2`、`GetPowder`、`O2IntoBottle` 没有统一订阅 `ExperimentReset`，药品、Hot、MakeingO2 和延迟调用可能跨实验残留。
- **气泡条件不统一：** `MakeO2.DelayStartAirBubbles` 当前直接 `airBubbles.Play()`，即使 `If_Get_Powder.Can_Burn()` 为 false，视觉气泡也可能出现。应将气泡显示与实际反应条件统一。
- **收集完成过早：** `O2IntoBottle.TryCompleteCollectionStep` 在真正产氧的接触帧即可完成步骤，没有最小收集量或收集时间。建议加入收集量阈值、瓶盖状态和连续产氧时间。

### 6.3 加强安全流程

建议把安全规则从提示文字升级为可验证步骤：

- 酒精灯倾斜、熄灭方式和灯帽状态。
- 加热前检查气密性。
- 排水集气结束时，先处理导管与热源，避免倒吸。
- 错误操作的恢复路径，而不是只显示“危险”。
- 器材归位不仅得分，还应说明哪里不规范以及为什么。

### 6.4 评分与模式

目前自由练习主要是隐藏引导，并非真正自由反应沙盒。应明确命名为“自由练习”，或在以后增加真实自由操作规则。

考核模式可以增加：错误次数、危险操作、时间、器材归位、步骤回看和最终解释。事件日志比单纯 100 分更适合教学评估。

## 7. XR 与教学 UX

- 增加首次使用微教程：射线、抓取、释放、Socket、复位和语言切换。
- 提供坐姿/站姿、身高校准、左右手、移动/转向/防晕动选项。
- 高亮不能只依赖颜色，应加入图标、轮廓或形状，兼顾色弱用户。
- `Hints` 面板使用固定尺寸和自动字号，需验证长英文、长中文是否溢出；必要时改布局组件和滚动文本。
- 先恢复缺失的中文字体资产，再检查所有 TMP 引用、回退字体和不同语言下的字形是否完整。
- 提供 Socket 放置预览、错误原因、可恢复操作和完成确认。
- 音效/震动是辅助反馈，不能成为唯一完成提示。
- 继续保留中英文、三种模式和无头显调试入口，这些是现有优势。

## 8. 材质、Shader 与渲染性能

### 8.1 SRP Batcher

4 个新 Lab Shader 的属性直接声明为 HLSL 全局变量，没有 `CBUFFER_START(UnityPerMaterial)`。当前 URP 高质量资产又开启了 `m_UseSRPBatcher=1`，因此这些 Shader 无法利用 SRP Batcher。

建议将材质参数放入 `UnityPerMaterial`，纹理和 sampler 保持在缓冲区外，并用 Frame Debugger/URP 兼容性检查确认，而不是把 `multi_compile_instancing` 当成 SRP Batcher 支持。

### 8.2 材质实例与 GC

`LabItem`、`LabLiquidVolume`、`Wobble` 和溶解材质仍会创建材质实例。按当前子层级统计，14 个 LabItem 都触发一次高亮时最多约产生 60 个材质实例，TestTubeRacks 单项约 26 个；其父级还嵌套 TestTube1，存在两个 LabItem 分别保存/恢复同一批材质颜色的所有权冲突风险。建议：

- 使用 `MaterialPropertyBlock` 设置 `_BaseColor`、`_EmissionColor`、液位和 Wobble 参数。
- 以 Renderer 为高亮状态唯一所有者，父/子 LabItem 不分别创建和恢复材质；聚合试管架不应与其中的试管互相覆盖高亮状态。
- 将高亮脉冲放到 Shader 的时间函数中，减少 C# 每帧遍历材质。
- 缓存 Renderer、材质属性和原始状态，避免每帧字符串拼接和无效文本赋值。
- 对频繁变化的液体属性统一由一个控制器管理，避免多个脚本各自创建/恢复材质实例。

### 8.3 透明玻璃与液体

玻璃/液体采用双面透明和关闭 ZWrite，存在自身透明穿插和 overdraw 风险。建议：

- 封闭玻璃尽量使用背面剔除和有限透明层数。
- 避免多个大面积透明模型重叠。
- 在设备上检查前后面、瓶口、液体和气泡的排序。
- 高质量桌面模式再考虑深度纹理/折射，不要直接放到 Quest 默认路径。
- 液体仍缺少任意高度自动封盖；需要独立 cap 网格或专用几何方案。
- `LabLiquidVolume` 当前在 Start 计算包围盒，若运行时改变缩放，应提供显式刷新，而不是每帧重新计算。

### 8.4 灯光和阴影

主场景 16 盏灯中 14 盏启用阴影，且 URP 高质量配置使用 2048 主光阴影、50 米阴影距离和附加灯阴影。建议：

- 火焰和局部器材灯尽量只提供视觉补光，不投实时阴影。
- 环境主光保留必要阴影，减少聚光灯数量、强度和覆盖范围。
- 用 VFX 自发光替代部分真实点光源。
- 烘焙静态房间照明，避免大量实时灯叠加。
- 单独建立 Quest 配置，不要继续以桌面 High 作为默认性能依据。

### 8.5 模型和碰撞体

**优先修复动态非凸碰撞：** 当前场景共有 63 个 MeshCollider（32 个 Convex、31 个非 Convex），其中 15 个非 Convex MeshCollider 位于非 Kinematic Rigidbody 下，涉及多支试管、GlassBottle、DrugBottle、LampCap/Coll 等。Unity/PhysX 对非凸网格碰撞体与动态刚体的组合有限制。手持器材应优先改成 Capsule/Box/组合凸碰撞体，或在形状允许时使用 Convex；只有静态环境才考虑非 Convex MeshCollider。不要仅为了保留外形而让抓取器材继续使用动态非凸网格。

`TestTubeRack.fbx` 约 32.5 MB、场景引用 46 次，是明确的重点模型。源三角化估算约 25,956 tris，面数并非最极端，但体积和动画/导入数据仍需整理。`18中式陶瓷洗手盆.fbx` 约 78,336 tris、场景引用 15 次；`a.fbx` 约 753,292 tris、场景引用 3 次，几何优化价值更高。以上为源文件估算，不冒充 Unity 导入后的实时统计。

建议：

- 优先对 `a.fbx` 做减面/LOD；洗手盆保留外观轮廓并简化碰撞；试管架合并重复器材为 Prefab 变体，重点清理动画和导入数据。
- 启用合适的网格压缩，关闭不需要的动画导入。
- 12 支试管不要各自使用复杂 MeshCollider；用简单 Capsule/Box 或组合碰撞体。
- 为精密模型制作 LOD，近景保留细节，远景减少三角形。
- 统一单位、枢轴、法线、切线和缩放，避免依赖极端非均匀缩放。
- 内置 Sphere/Cylinder 与 FBX 风格统一，避免实验器皿外观差异过大。

## 9. 资源与维护

### 9.1 可优先归档的候选

在完成 Unity 依赖复核后，优先处理：

- 原始字体约 825.7 MB，含多份重复字体；应先恢复/重建合适的中文字体资产并验证全部 UI，再归档其余未使用字体。
- 未使用天空盒目录约 76.7 MB。
- `GrassFlowers` 约 124.9 MB 示例资源。
- 主场景未引用的 `uploads_files_2330517_koray+bey.obj` 约 33.2 MB。
- 主场景未引用的 `Materials/Lobby/city.fbx` 约 35.8 MB。

这些候选应先做依赖报告再移动归档，不能直接按目录删除。未被主场景引用的资源也不一定构成运行时错误，但会增加导入和维护成本；`Resources` 下的内容尤其需要控制。

当前使用的“翠星云”天空盒也约 72.2 MB，可压缩贴图、改格式，或在室内实验室改用更简单环境天空。

### 9.2 建立版本管理

当前目录没有 `.git`。建议：

- 初始化 Git，并保留现有 Unity `.gitignore`。
- 大文件使用 Git LFS，或建立独立的资源归档目录。
- 每次结构迁移建立小提交和标签，不再只依赖临时目录备份。
- 保存测试报告、构建报告和性能对比，便于回退。

## 10. 推荐实施顺序

### 第一阶段：高收益、低风险的基线整理

1. Unity 编译 + 9 个 EditMode 测试 + Android 构建；先归档未使用的 OutputEvent Helpers，排除 HDRP/Cinemachine 引用风险，并恢复缺失的中文字体资产。
2. 先修复 15 个动态非凸 MeshCollider，再验证抓取、碰撞和 Socket。
3. 新 Shader 添加 `UnityPerMaterial`。
4. 关闭局部火焰/器材灯的实时阴影，减少高强调光灯。
5. 归档已确认的无用字体、天空盒、示例和大模型。
6. 处理试管架压缩、减面和动画导入。
7. 把现有静态检查脚本纳入仓库，形成每次可执行的验证命令。

### 第二阶段：架构与实验正确性

1. 用单一事件适配层替代 Score 每帧反写。
2. 实现统一物理+状态重置。
3. 增加 PlayMode 场景验收和 Shader 检查。
4. 将气泡显示与药品条件统一，增加氧气收集量阈值。
5. 明确反应物、方程式和安全步骤。
6. 用 MaterialPropertyBlock 和脏标记降低 GC/UI 更新。

### 第三阶段：模型、液体与 Quest 性能

1. 优化器材网格、LOD、碰撞代理和模型缩放。
2. 完善液体封顶和透明排序。
3. 建立 Quest 专用 URP/质量配置并实测。
4. 在独立分支升级 Unity/XRI/OpenXR，再决定正式渲染路线。

### 暂不建议优先做

多人联机、复杂分子级模拟、真实流体动力学、AI 自动评分和大量新实验。它们会放大当前双轨状态和测试缺口，应先完成稳定基线。

## 11. 最终验收标准

- Unity 导入与 C# 编译无错误，Shader 无错误。
- 缺失字体资产恢复，TMP 引用完整，中英文均无方框或空白。
- 现有 EditMode 测试通过，新增 PlayMode 场景验收通过。
- 制氧和复燃均能从准备到归位完整运行，重置无残留。
- 玻璃、液体、气泡和高亮在主要视角下无严重排序/恢复问题。
- Android ARM64/IL2CPP 构建成功，并能安装启动。
- 获得优化前后包体、Draw Call、批处理、GC、三角形和帧时间对比。
- 有 Quest 后，再确认 XR 输入、震动、双眼、透明材质、性能和舒适度。

## 12. 三阶段实施结果

三个阶段的代码、场景和资源修改已完成，详见 [OPTIMIZATION_IMPLEMENTATION.md](OPTIMIZATION_IMPLEMENTATION.md)。该报告区分了已完成的静态验证与仍需在 Unity/Quest 中执行的字体生成、测试、构建、模型减面和视觉/性能验收。

## 13. 交叉复核后的补充修复

已恢复实际使用的贴图、补齐 Equipment 阴影、完善字体场景重绑、区分项目选择与显式重置、限制收集顺序、修复垃圾桶恢复与抓取快照、按材质槽处理高亮，并将 PlayMode 测试改为强制重载和脏状态验收。新增实现细节见 [OPTIMIZATION_IMPLEMENTATION.md](OPTIMIZATION_IMPLEMENTATION.md)。

## 14. P0 实施结果

已修正 Equipment 片元阴影变量、迁移当前木材材质到 URP、将氧气改为按实验批次初始化、清除垃圾桶器材的归位评分，并加入编辑器自动中文字体修复。新增同项目重新选择和垃圾桶评分 PlayMode 回归测试。实际 Unity 编译、字体生成、Shader 验证和测试运行仍需在 Unity 中执行。

## 15. P1 架构与模型实施结果

已加入唯一旧状态适配器、XRI 实际选择恢复和共享材质状态管理。已用 Blender 真实生成 `a.fbx`、洗手盆、试管架的 LOD 与凸包代理，并提供 Unity 内按名称接入 LODGroup 的工具。统计和接入步骤见 [MODEL_OPTIMIZATION_GUIDE.md](MODEL_OPTIMIZATION_GUIDE.md)。Unity 编译、LOD 切换、XRI 选择和测试运行仍需验证。

## 16. P2 性能与发布实施结果

已加入事件驱动计分、注册表缓存、液位更新节流、性能 JSON 采样、项目验证、CI、签名构建检查和构建信息输出。已清理 OpenXR 残留与未使用 ARFoundation，Quest Multiview/Subsampled 已配置但需真机验证。另已补充液体 cap、教学 UI 集合复用和氧气文本限频。详见 [P2_PERFORMANCE_RELEASE.md](P2_PERFORMANCE_RELEASE.md)。

## 17. 全面性能优化实施结果

已加入 Capsule 物理代理、LOD3 与 Quest 近景 LOD2 切换、cap 材质共享、cap/高亮/Wobble 更新节流、QuestLow、Android 材质分级、纹理预算和 UnityStats 性能统计。Android 构建预处理临时剔除原始高模和 LOD1，并在构建后恢复 Editor 引用。五轮检查 240 项通过，模拟检查 350 项通过。详见 [PERFORMANCE_IMPLEMENTATION.md](PERFORMANCE_IMPLEMENTATION.md)。

## 18. 阶段 1：化学核心

已建立纯 C# 的 `VRChemistry.Chemistry.Core`，用物质、混合物、反应条件和反应结果替代单一实验中的化学硬编码。制氧、复燃和中和资产已建立，并通过 `ChemistryLegacyAdapter` 接入现有流程。

复核修复了反应资产 PascalCase 字段、下层单位常量循环依赖、重复物质汇总、非法数值、负温度、空参与者、浓度更新和气泡/气路同步问题。当前为 37 个 EditMode + 11 个 PlayMode 测试，五轮检查 240 项、流程模拟 350 项、化学模拟 79 项通过。详见 [CHEMISTRY_CORE_GUIDE.md](CHEMISTRY_CORE_GUIDE.md)。

## 19. 阶段 1 深化：反应过程与混合物提交

新增 `ReactionProcess`，明确区分纯化学求值和按时间提交，支持暂停、恢复、完成幂等、失败不部分提交、取消和快照重置。`MixtureState` 增加 `GasComponents`、`SolidComponents`、`CanConsume`、`CanAddProduct`、`Clone/CopyFrom`，生成的氧气可以继续参与复燃。

新增 12 个反应过程测试和 39 项过程模拟检查，最终为 37 个 EditMode + 11 个 PlayMode 测试，五轮检查 240 项、流程模拟 350 项、化学模拟 79 项通过。另修复精确尾段、实际反应时间、相对浮点容差、实际消耗核对和多产物溢出回滚。
