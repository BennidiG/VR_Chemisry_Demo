# P2 性能与发布指南

本阶段在 P0/P1 基础上加入运行时开销优化、性能采样、项目验证、CI 和发布入口。没有 Unity/Quest 的部分不会被标记为已通过。

## 1. 运行时性能改动

- `ExperimentRunner.ScoreChanged`：分数、步骤、归位、阶段和模式变化时通知。
- `Score`：只在状态或语言变化时刷新 TMP 分数文本，不再每帧重建字符串。
- `LabItemRegistry`：成员不变时复用 ActiveItems 缓存，减少高亮遍历分配。
- `LabLiquidVolume`：纯移动不再重算液位，只在旋转、局部缩放或父级缩放变化时更新。
- `LabLiquidCap`：为 10 个液体主体生成世界水平表面 cap，跳过已有 `LiquidLevel`，并随 Renderer/LOD 可见性显示或隐藏。
- 教学 UI：复用高亮字典、待移除列表、准备列表和 StringBuilder，减少周期性 GC。
- 氧气文本：按 0.1 秒限频刷新，不再每个物理帧更新 TMP。
- LOD 子对象：复制 Wobble、液位和 cap 配置，避免 LOD 切换后液体效果消失。
- `LabPerformanceMonitor`：默认不采样，调试时按 **F9** 采样 10 秒；**F8** 可在 Quest 和 QuestLow 之间切换。

性能 JSON 包含：

- 平均、最低、最高帧时间与估算 FPS。
- GC 起始/结束/增量内存。
- Unity、设备、显卡和质量档信息。
- URP Render Scale、总分配/保留内存和 Mono 内存。

报告路径由 `Application.persistentDataPath` 和 `lab_performance.json` 组成，Console 会输出实际路径。帧时间不是 GPU 时间的替代品，仍需 Frame Debugger/Profiler 和真机验证。

## 2. 项目验证

菜单：

- **VR Chemistry → Validation → Validate Project**
- **VR Chemistry → Validation → Build Android APK**
- **VR Chemistry → Validation → Build Signed Android APK**

验证覆盖：

- SampleScene 是否启用、必要脚本和程序集引用。
- Score 是否仍直接写 Runner。
- 动态非凸 MeshCollider 和局部灯阴影。
- Lab Shader 的 URP include、UnityPerMaterial 和 Equipment 阴影。
- 材质、字体、LOD 文件和导入 Mesh。
- Quest URP、Android ARM64、IL2CPP 和图形 API。
- 签名构建的包名、keystore 和 key alias。

报告：`Logs/LabProjectValidation.txt`。

构建前会自动：

1. 检查当前是否打开 SampleScene。
2. 必要时生成中文字体。
3. 按名称接入 LOD。
4. 执行项目验证。

验证失败会阻止构建，不会生成“成功”的假象。

## 3. CI

本地脚本：`Tools/run_unity_validation.ps1`

需要 Unity 2020.3.30f1c1、Android Build Support、IL2CPP、JDK/SDK/NDK。可用 `UNITY_PATH` 指定 Unity 可执行文件。

```powershell
$env:UNITY_PATH = 'C:\Program Files\Unity\Hub\Editor\2020.3.30f1c1\Editor\Unity.exe'
./Tools/run_unity_validation.ps1
```

只运行导入、测试和验证，不构建：

```powershell
./Tools/run_unity_validation.ps1 -SkipBuild
```

跳过测试，仅验证和构建：

```powershell
./Tools/run_unity_validation.ps1 -SkipTests
```

要求签名构建：

```powershell
./Tools/run_unity_validation.ps1 -Release
```

流程：

```text
导入 → EditMode → PlayMode → 项目验证 → Android 构建
```

所有日志写入 `Logs/CI`。任何 Unity 非零退出、测试失败或验证失败都会让脚本失败。

GitHub Actions：`.github/workflows/unity-validation.yml`

- 使用自托管 Windows Runner。
- `UNITY_PATH` 使用仓库/组织变量，不放密码。
- 支持手动选择跳过构建或要求签名。
- 上传 `Logs/CI` 和 APK（若存在）。
- 当前没有配置真实 Runner，因此不能把 YAML 当成已运行通过的 CI。

## 4. 发布配置

### 开发构建

使用 **Build Android APK**。仍会执行字体、LOD 和项目验证，但允许默认包名和未配置签名。

### 签名构建

使用 **Build Signed Android APK**，必须先配置：

- 非默认 Android Application Identifier。
- `bundleVersion` 和 Android Bundle Version Code。
- 符合目标平台要求的 Min/Target SDK。
- Android Custom Keystore、Key Alias 和本机密码。
- Android ARM64 + IL2CPP。
- Oculus Loader 与 Oculus Settings。

密码只在本机 Unity Player Settings 中配置，不写入仓库、脚本或 CI。

构建成功后生成：

- `Build/Android/VR_Chemistry.apk` 或 `VR_Chemistry-signed.apk`
- `build-info.json`：Unity 版本、包名、版本号、时间、签名类型、输出路径、大小和架构。

## 5. Quest 性能配置

当前保留 Oculus 路线，已清理没有 OpenXR 包的残留设置，并移除未使用的 ARFoundation。

Quest 配置包括：

- `UniversalRP-Quest`：Render Scale `0.85`、主光阴影 `1024`、阴影距离 `25`、附加光顶点路径。
- Oculus Android Multiview：`m_StereoRenderingModeAndroid: 1`。
- Subsampled Layout：开启。
- 保留 Shared Depth Buffer 和 Optimize Buffer Discards。

Multiview/Subsampled 必须在 Quest 上检查：

- 双眼材质和透明效果。
- 高光、液面、气泡和 VFX。
- XR 输入和帧时间。

若出现兼容问题，可回滚 Oculus Settings 中的 Android Multiview/Subsampled，或恢复 P2 前备份，不建议同时升级 Unity/XR/URP 包。

QuestLow 可选档使用 Render Scale `0.75`、主光阴影 `512`、阴影距离 `15`，默认不强制替换 Quest。Android 近景使用 LOD2，构建时临时剔除原始高模和 LOD1，构建后恢复 Editor 引用。cap 材质按液体源共享，物理代理 Solver Iterations 为 4。最新物理代理、LOD3、更新节流和纹理预算见 [PERFORMANCE_IMPLEMENTATION.md](PERFORMANCE_IMPLEMENTATION.md)。

## 6. 建议验收顺序

1. 打开 Unity，等待字体和 LOD 自动准备。
2. 运行 **Validate Project**，处理错误和警告。
3. 运行 37 个 EditMode 和 11 个 PlayMode 测试。
4. 运行两个实验的完整流程和重置。
5. 按 F9 采样，检查 JSON 和 Frame Debugger。
6. 用本地 CI 脚本验证可重复构建。
7. 配置包名和签名后再构建签名 APK。
8. 最后在 Quest 上检查 Multiview、透明材质、液面和帧率。

P2 修改前备份：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_P2_backup_20261114_084903`

当前没有 Unity/Quest 实测结果，不能宣称性能提升幅度、APK 构建成功或真机兼容性已经验证。
