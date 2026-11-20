# 全面性能优化实施报告

本次在 P0/P1/P2 基础上继续优化物理、模型、透明效果、更新频率、材质分级、纹理和发布工具。没有 Unity/Quest 实测的部分单独列出，不把静态检查当成性能结果。

## 1. 物理碰撞优化

新增 `Assets/Editor/LabPhysicsOptimizationTools.cs`：

- 菜单：**VR Chemistry → Optimization → Apply Physics Proxies**。
- 主场景打开时自动执行一次，支持 `sceneOpened`。
- 试管和氧气瓶使用 CapsuleCollider 作为主要物理代理。
- 原 MeshCollider 只禁用、不删除，便于回滚。
- 保留 Trigger 网格、Box/Sphere/Capsule 和其他碰撞类型，避免破坏药品、Socket、加热和氧气接触。
- 构建前准备流程也会执行物理代理。

物理代理是按模型局部包围盒计算的近似形状，必须在 Unity 中重新验证抓取、Socket、药品接触、加热和氧气收集。

## 2. LOD3 与 Quest 近景

已实际生成第三级模型：

| 模型 | LOD3 三角面 | 相对原模型 |
|---|---:|---:|
| `a.fbx` | 60,455 | 约 8% |
| 洗手盆 | 9,384 | 约 12% |
| 试管架 | 3,112 | 约 12% |

新增 `LabQuestLodController`：

- 桌面保留原始 LOD0 用于检查。
- Android 从 LOD2 开始，再切换 LOD3，避免近景默认使用原始高模或 LOD1。
- 已有三级 LODGroup 会补建 LOD3，不会重复创建旧 Renderer。
- LOD 子对象复制 Wobble、液位和 cap 配置。
- LOD 名称匹配失败或歧义时跳过，不猜错模型。

统计文件：`Assets/Modle/Optimized/optimization_report.json`。

### cap 材质共享与 Quest 构建剔除

- `LabLiquidCap` 按液体源共享 cap 材质并引用计数，避免每个 LOD 创建独立 cap 材质。
- 物理代理刚体 Solver Iterations 为 4、Velocity Iterations 为 1，仅影响代理对象。
- `LabQuestBuildPreprocessor` 在 Android 构建前临时使用 LOD2/LOD3，并清空原始高模和 LOD1 的 Mesh/Collider 引用，避免它们进入 APK。
- 构建完成或预处理异常时恢复 Editor 中的 LODGroup、Mesh 和 Collider 引用，不保存剥离后的场景。
- Android VFX 播放速率和粒子预算已降低，桌面保持原效果。

## 3. 更新频率与透明效果

- 高亮脉冲更新限制为 30 Hz。
- Wobble 限制为 30 Hz，静止时跳过无效属性写入；累计 deltaTime，动画速度不因降频改变。
- cap 只在位置、旋转、缩放、填充量、Wobble 或 LOD 可见性变化时更新。
- Android cap 段数最多 24，且随当前 LOD Renderer 显示或隐藏。
- 火焰、燃烧长度和溶解逻辑只在对应状态有效时执行。
- 教学 UI 复用字典、列表和 StringBuilder，减少周期性 GC。
- 氧气文本按 0.1 秒限频刷新。
- Android 材质分级：限制液体 `_WaveAmplitude` 和气泡 `_EmissionStrength`，桌面保持原效果。

## 4. QuestLow 与性能证据

新增 `UniversalRP-QuestLow.asset` 和质量档：

- Render Scale `0.75`。
- 主光阴影 `512`。
- 阴影距离 `15`。
- 更少的附加光。
- 保留 SRP Batcher。

默认 Android 仍使用 Quest，不强制降低画质。调试时：

- **F8**：在 Quest 和 QuestLow 之间切换。
- **F9**：采样 10 秒并输出 JSON。

性能 JSON 包含：

- 帧时间、估算 FPS、GC 和内存。
- 当前质量档与 Render Scale。
- Editor `UnityStats` 的 Draw Calls、三角形和批次。
- Unity、设备、显卡信息。

这些数据用于 A/B 对比，不代表 Quest GPU 帧率已经验证。

## 5. 字体与纹理

- 中文 TMP 修复工具预生成实验资产和常用 UI 字符，减少首次 glyph 卡顿。
- Android 构建前对大纹理设置 2048 上限和 `ETC2_RGBA8` 压缩。
- 原始桌面资源不被强制改写。
- 新纹理工具：`VR Chemistry → Optimization → Apply Android Texture Budgets`。

## 6. 构建与 CI

`ProjectBuildValidation.PrepareAssetsForBuild` 现在按顺序执行：

1. 批处理自动打开 SampleScene。
2. 生成/修复中文字体。
3. 接入 LOD 和 LOD3。
4. 应用物理代理。
5. 应用 Android 纹理预算。
6. 执行项目验证。
7. 开发或签名 Android 构建。

构建验证新增对物理工具、LOD3、QuestLow 和新增元数据的检查。

本地 CI 仍为：

```powershell
./Tools/run_unity_validation.ps1
```

可跳过构建或测试：

```powershell
./Tools/run_unity_validation.ps1 -SkipBuild
./Tools/run_unity_validation.ps1 -SkipTests
```

## 7. 五轮复核结果

本轮执行五轮全场景检查：

- **240 项检查，0 错误**。
- **350 项模拟检查全部通过**。
- **79 项化学模拟通过**，详见 [CHEMISTRY_CORE_GUIDE.md](CHEMISTRY_CORE_GUIDE.md)。
- 场景文档：**1,590**。
- GameObject：**364**。
- 组件：**1,240**。
- 自定义 UnityEvent 方法：**134**。
- LOD1/LOD2/LOD3 网格名称：无缺失、无歧义。
- CI PowerShell 语法：通过。

完整机器可读结果：`Logs/full_scene_check.json`。

## 8. 仍需实测

- Unity C# 和 Shader 实际编译。
- 48 个 Unity Test Runner 测试。
- 物理代理的抓取、碰撞和 Socket。
- LOD 实际切换、液面、透明排序和 cap。
- Quest Multiview、Subsampled、QuestLow 和实际帧率。
- 中文字体图集、纹理内存、APK 构建与发布签名。

本轮优化前备份：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_performance_backup_20261116_102942`

当前修改尚未提交或推送 Git。
