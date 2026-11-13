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
- 新增 4 个 PlayMode 测试，检查核心引用、一次性氧气初始化、制氧脏状态重置和复燃供应恢复。

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
- 当前测试总数：**9 个 EditMode + 4 个 PlayMode = 13 个**。

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
6. 液体任意高度自动封盖几何；仍需要独立 cap 网格。
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

## 备份

本次实施前的原始版本位于：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_optimization_backup_20261113_142543`

归档资源位于该备份目录下的 `Archive` 子目录。建议后续将工程纳入 Git/Git LFS，建立每次优化的小提交和标签。
