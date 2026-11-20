# 三阶段优化实施报告

本报告记录按“基线整理 → 架构与实验正确性 → 模型/液体/Quest 配置”顺序完成的工作。所有修改均有原始备份，未将静态检查冒充 Unity 编译或真机验证。

## 阶段 1：基线整理

### 编译与字体

- 将 `OutputEvent Helpers` 示例移至备份归档，移除其对 HDRP/Cinemachine 的编译依赖风险；未发现业务场景使用这些示例。
- 使用完整的 `NotoSansSC.ttf`（约 17.8 MB）和 `OFL.txt`，新增 `Assets/Editor/ChineseFontRepair.cs`。
- 在 Unity 菜单 **VR Chemistry → Repair Chinese Font** 中可生成动态中文 TMP 字体资产，并绑定场景缺失的 GUID `d43e48d21e6f5b245bd50c421a0e3044`。
- 字体生成功能已写入工程，但**尚未在 Unity 内执行**；执行后应检查全部中文 UI。

### 动态碰撞与灯光

- 将 15 个动态非 Convex MeshCollider 改为 Convex，覆盖多支试管、GlassBottle 和器材下的 Coll 子碰撞体，避免动态刚体与非凸网格组合的 PhysX 限制。
- 关闭 13 个局部灯的实时阴影，保留太阳主光阴影。
- 将 7 个过亮局部灯的强度上限设为 6，避免大量高强调光同时照亮器材。

### Shader、模型和资源

- 四个 Lab Shader 均加入 `CBUFFER_START(UnityPerMaterial)` / `CBUFFER_END`，补齐 SRP Batcher 所需的材质常量缓冲。
- `TestTubeRack.fbx`、`a.fbx`、`18中式陶瓷洗手盆.fbx` 启用 `meshCompression: 2`，关闭不需要的动画导入。
- 归档未使用的天空盒、GrassFlowers、koray OBJ、city.fbx 和原始字体目录，全部保存在原始备份的 `Archive` 子目录。
- 当前天空盒 6 张 TIF 已转成 JPEG 并重绑材质，体积从约 72 MB 降至约 2 MB。
- 当前 `Assets` 总大小约 **224 MB**，此前约 **1.37 GB**。

## 阶段 2：架构与实验正确性

### 状态与重置

- `Score` 改为计算旧状态哈希，仅在旧状态变化时调用 `SyncRunnerState`，不再每帧全量反写 Runner。
- 新增 `ILaboratoryResettable` 与 `LaboratoryLayoutController`，统一处理：
  - 器材父级、位置、旋转和缩放。
  - Rigidbody 速度、重力、阻尼和运动学状态。
  - 抓取对象的启用/停用和 Socket 关系恢复。
  - 火柴、药品、火焰、气泡、氧气、提示和归位状态。
- `ReG` 已改用 `ResetCurrentExperiment`，Runner 保留 `ResetLaboratory` 作为完整场景重载回退路径。
- `MakeO2`、`GetPowder`、`O2IntoBottle`、`CanSee_AirBubbles`、`VisualEffectGraph` 均接入重置。

### 加热与化学流程

- 新增 `MakeO2.TryCompleteHeatingStep`，只有酒精灯已点燃且试管处于火焰中才允许完成加热步骤。
- 场景中原来直接写 `Make_O2_Step5_To_Step6` 的回调已替换，去掉了无条件变绿的提示回调。
- `DelayStartAirBubbles` 只有存在可燃烧药品时才播放气泡。

### 氧气与收集

- 新增 `OxygenSupply` 并挂到 GlassBottle：氧气不再属于火柴，火柴销毁/重建不会清空瓶氧。
- `InitializeForExperiment` 每个实验只允许初始化一次，重复点击按钮不能补满氧气。
- `O2IntoBottle` 加入 `minimumCollectionAmount = 120`，未达到收集量不会完成收集步骤。
- 氧气耗尽后的提示改为可使用外部火源重新点燃；余烬复燃仍受氧气量限制。
- 新增 6 个 PlayMode 测试，检查核心引用、按局氧气初始化、制氧脏状态重置、复燃供应恢复、同项目重新选择和垃圾桶评分。

## 阶段 3：模型、液体与 Quest 配置

### 材质实例与液体

- `LabItem` 高亮改用 `MaterialPropertyBlock`，不再为每个高亮对象创建材质副本。
- `LabLiquidVolume` 改用 `MaterialPropertyBlock`，并在 localScale 改变时重新计算液位范围。
- `Wobble` 和火柴溶解效果改用 `MaterialPropertyBlock`；各效果先读取并复用当前属性块，再写自己的字段，避免互相覆盖和持续分配。
- 统一重置使用 `CreateMatch.ResetMatch()`，销毁当前火柴并从 Prefab 重建，确保点燃后被销毁的火柴头也能恢复。
- 高亮继续保留原始 alpha，避免玻璃因高亮变得不透明。
- 清透玻璃材质 `_Cull` 改为背面剔除，减少双面透明 overdraw；磨砂玻璃和液体仍按需要保留双面设置。

### Quest 配置

- 新增 `Assets/Settings/UniversalRP-Quest.asset`：
  - Render Scale `0.85`。
  - 主光阴影 `1024`。
  - 阴影距离 `25`。
  - 关闭附加灯阴影。
  - 附加光使用顶点光路径。
  - 保留 SRP Batcher。
- `QualitySettings` 新增 Quest 档，Android 默认选择 Quest（索引 3），Standalone 仍保留 High（索引 2）。
- 这只是配置优化，**Quest 实机帧率、视觉效果和兼容性尚未验证**。

### 构建验证入口

- 新增 `Assets/Editor/ProjectBuildValidation.cs`，菜单 **VR Chemistry → Validation → Build Android APK**。
- 新增 `Assets/Tests/PlayMode/ExperimentSceneSmokeTests.cs` 与测试程序集。
- 当前测试总数：**37 个 EditMode + 11 个 PlayMode = 48 个**。

## 当前验证结果与未执行事项

已完成静态检查：

- 46 个 C# 文件括号和结构检查。
- 场景重复 fileID、悬空组件和新回调检查。
- 动态非凸碰撞与局部灯阴影检查。
- Shader 的 URP include、SRP 常量缓冲和材质属性块检查。
- 氧气供应、重置、收集阈值和 Quest 配置检查。
- 资源归档、JPEG 天空盒和字体文件完整性检查。

**未在当前环境执行：**

1. Unity 导入、C# 语义编译和 Shader 编译。
2. `Repair Chinese Font` 菜单生成字体资产。
3. Unity Test Runner 的 EditMode/PlayMode 测试。
4. Android APK 构建、安装和 Quest 真机验证。
5. `a.fbx` 的实际减面/LOD 制作；目前只完成导入压缩和动画关闭，没有 Blender 可用。
6. 复杂瓶口的液体 cap 透明排序和视觉穿插仍需 Unity 检查；运行时 `LabLiquidCap` 已加入，但不替代所有独立瓶口网格。
7. Unity/XRI/OpenXR 版本升级；本阶段保持原版本，避免盲升级造成新回归。

## 追加交叉复核修复

- 恢复实际使用的 `Metal_MetallicSmoothness.tif` 与 `OBS_Albedo.tif`，保留原 GUID；清理 Stick 未读取的断链槽位及天空盒冗余 `_Tex` 条目。
- `LabEquipment` 增加 `ShadowCaster` Pass，并在 Forward 中通过 `TransformWorldToShadowCoord` 和 `mainLight.shadowAttenuation` 处理投影/接影。
- 中文字体工具补充 atlas/material 子资产持久化、TMP `font` 与 `fontSharedMaterial` 场景重绑和场景保存。
- 氧气选择/显式重置分开：同项目选择不清空供应；项目切换或显式重置时，Runner 直接初始化项目2供应，新火柴通过 `AmountChanged` 同步。
- 收集氧气要求项目1、步骤6、有效接触和真正产氧，避免提前累计跳过步骤。
- 删除酒精灯碰撞脚本的提前变绿，统一由加热守卫通过后修改提示。
- 垃圾桶中的持久 `LabItem` 改为禁用，布局重置可恢复；抓取启用状态也加入快照。
- 液体持久范围使用缓存材质实例，避免动态块覆盖；同时监听位置、旋转和父级缩放变化。
- 高亮按 Renderer 与材质槽分别保存属性块，试管架清理同时高亮试管架及试管。
- PlayMode 测试强制重载场景并制造脏状态，补充项目2供应恢复断言，现为 9 个 EditMode + 4 个 PlayMode。

1. 打开工程并等待依赖导入，先处理 Console 中的编译或 Shader 错误。
2. 执行 **VR Chemistry → Repair Chinese Font**，检查全部 TMP 中文文本。
3. 运行 **Window → General → Test Runner → EditMode**，再运行 PlayMode。
4. 使用 `F1/F2 → F3 → F4 → F5` 验证流程，并测试 `R` 重置后的器材和氧气状态。
5. 执行 **VR Chemistry → Validation → Build Android APK**，检查构建日志和 APK。
6. 后续有设备时再验证 XR 输入、材质透明排序、液面、震动和帧率。

## P0 修复：编译、显示与实验可完成性

- `LabEquipment.shader` 的 `GetMainLight(output.shadowCoord)` 修正为 `GetMainLight(input.shadowCoord)`，消除片元函数使用未定义变量的问题。
- `Stick.mat`、`Wood_2/3/4.mat` 迁移到 URP Lab Equipment，保留原贴图并统一高亮属性。
- 氧气改为按 `ExperimentRunId` 初始化：每次新实验批次重置，但同一批次的重复按钮不能补氧；制氧从空瓶开始，复燃按批次初始化。
- 垃圾桶禁用 LabItem 时清除对应归位评分，避免器材已丢弃但仍获得归位分。
- 中文字体工具增加 `InitializeOnLoad` 自动修复：打开主场景后自动生成字体、持久化图集/材质并重绑 TMP，也保留手动菜单入口。
- PlayMode 新增“同项目重新开始氧气”和“垃圾桶清除归位分”回归测试，当前共 9 个 EditMode + 6 个 PlayMode。

当前环境没有运行 Unity，因此字体资产生成、C# 语义编译、Shader 编译、Test Runner 和实验实际操作仍需在 Unity 中验证。

## P1 修复：架构与模型

- 新增 `LegacyExperimentAdapter`，Score 只发事件，所有旧状态到 Runner 的映射集中在一个适配器中。
- XRI 重置显式退出 select/hover；火柴重建刷新 Relay，并在下一帧尝试恢复 MatchSocket 的实际选择。
- 新增 `VRChemistry.Runtime` 程序集，PlayMode 测试显式引用运行时代码和 XR Interaction Toolkit，不再依赖不可引用的 `Assembly-CSharp`。
- 新增 `LabMaterialState`，按 Renderer/材质槽共享高亮、Wobble、溶解状态，取消高亮不再整块回滚其他效果。
- 使用 Blender 真实生成 `a.fbx`、洗手盆、试管架的 LOD1/LOD2 和凸包代理，详见 [MODEL_OPTIMIZATION_GUIDE.md](MODEL_OPTIMIZATION_GUIDE.md)。
- 新增 `ModelOptimizationTools`，Unity 打开主场景后自动或手动按名称接入 LODGroup；名称不唯一时跳过，避免错配。
- PlayMode 增加唯一适配器和 MatchSocket 选择测试，当前共 9 个 EditMode + 8 个 PlayMode。

模型文件已实际生成，但 Unity 导入、LOD 切换、碰撞代理、XRI 选择恢复和 Test Runner 仍需在 Unity 中验证。无 Unity 模拟验收已达到 231/231 通过，详见 [SIMULATION_REPORT.md](SIMULATION_REPORT.md)。

## P2 修复：性能与发布

- `ScoreChanged` 事件驱动计分刷新，减少每帧 TMP 字符串和布局更新；注册表缓存复用 ActiveItems。
- 液位纯移动不再重算，LOD 子对象复制 Wobble、液位和 cap 配置；10 个液体主体启用 `LabLiquidCap`，随 Renderer/LOD 可见性显示。
- 教学 UI 复用高亮字典、列表和 StringBuilder，氧气文本按 0.1 秒限频刷新，减少周期性 GC。
- 新增 `LabPerformanceMonitor`，F9 采样并输出帧时间、GC 和设备信息 JSON。
- 新增 `LabProjectValidation`，覆盖场景、程序集、碰撞、Shader、材质、字体、LOD、Quest 和签名配置。
- 新增本地 CI 脚本和 GitHub Actions：导入、EditMode、PlayMode、项目验证、Android 构建和日志归档。
- 清理无 OpenXR 包的残留设置，移除未使用 ARFoundation；Quest 使用 Multiview 和 Subsampled Layout，需设备验证。
- 新增开发/签名 Android 构建入口及 `build-info.json`。
- 详见 [P2_PERFORMANCE_RELEASE.md](P2_PERFORMANCE_RELEASE.md)。

## 五轮全场景检查

已按场景结构、UnityEvent/程序集、实验逻辑模拟、Shader/模型资源、CI/发布配置五个层面完成复核，结果全部通过。期间修正了 14 个异常 30 位 GUID、补齐 Editor `.meta`，并修正检查器对 RectTransform 和 TrashCan 的误报。详见 [FULL_SCENE_CHECK.md](FULL_SCENE_CHECK.md)。

## 全面性能优化

- 物理代理：试管和氧气瓶使用 CapsuleCollider，原 MeshCollider 禁用保留，Trigger 和其他碰撞类型不变。
- LOD3：实际生成 `a_LOD3`、`basin_LOD3`、`rack_LOD3`；已有 LODGroup 可升级，Android 近景使用 LOD2。
- 更新节流：高亮/Wobble 30Hz、cap 变更驱动、静止 VFX 状态跳过一次更新、氧气 UI 0.1 秒刷新。
- QuestLow：新增可选低成本质量档，F8 与 Quest 切换；Android 材质分级限制波动和气泡自发光。
- 构建前自动应用物理代理和 Android 纹理预算；性能报告增加 Editor UnityStats Draw Calls、三角形和批次。
- cap 材质按液体源共享并引用计数；Android 构建预处理临时剔除原始高模和 LOD1，构建后恢复 Editor 引用。
- 代理刚体 Solver Iterations 为 4，Android VFX 播放速率和粒子预算已降低。
- 详见 [PERFORMANCE_IMPLEMENTATION.md](PERFORMANCE_IMPLEMENTATION.md)。

## 模拟实测补充

已完成流程模拟 810 项、五轮检查 240 项、P0/P1 模拟 350 项和化学核心模拟 79 项。Blender 几何检查确认原始 `a.fbx` 的 377 个退化三角形全部清理为 0，UV 保留，LOD/代理无 NaN、无缺失名称。详见 [SIMULATED_RUNTIME_TEST.md](SIMULATED_RUNTIME_TEST.md)。

## 阶段 1：化学核心

已建立 `VRChemistry.Chemistry.Core` 纯 C# 化学核心，包含物质、药品用量、混合物、反应条件、限制试剂、气体/液体/固体产物和质量守恒。

- 新增 `SubstanceDefinitionAsset`、`ReactionDefinitionAsset`、`ChemistryDatabase` 和 `ChemistrySimulationService`。
- 制氧使用 `2KClO3 -> 2KCl + 3O2`，包含 MnO₂、温度和收集条件。
- 复燃使用 `C + O2 -> CO2`，包含点燃、余烬和氧气条件。
- 中和反应用于验证限制试剂和液体产物扩展。
- 通过 `ChemistryLegacyAdapter` 接入现有 MakeO2、O2IntoBottle 和 VisualEffectGraph，保留旧 UnityEvent、步骤和 VFX 兼容。
- 新增 26 个化学核心测试、3 个化学集成 PlayMode 测试，以及质量守恒、重复物质、非法数值、负温度、浓度和反应过程生命周期模拟。
- 新增 `ReactionProcess`：按时间提交消耗/产物，支持暂停、恢复、完成幂等、失败不部分提交、取消和快照重置。
- `MixtureState` 按物质追踪气体和固体组成，生成氧气可以继续参与复燃。
- 五轮检查 240 项通过，P0/P1 模拟 350 项通过，化学模拟 79 项通过。详见 [CHEMISTRY_CORE_GUIDE.md](CHEMISTRY_CORE_GUIDE.md)。

## 备份

本次实施前的原始版本位于：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_optimization_backup_20261113_142543`

归档资源位于该备份目录下的 `Archive` 子目录。建议后续将工程纳入 Git/Git LFS，建立每次优化的小提交和标签。
