# P0/P1 无 Unity 模拟验收报告

## 结论

在没有 Unity Editor、.NET SDK 和 Quest 设备的当前环境中，已完成静态检查和逻辑模拟。

- 自动模拟检查：**350 / 350 通过**。
- 化学核心模拟：**79 / 79 通过**，详见 [CHEMISTRY_CORE_GUIDE.md](CHEMISTRY_CORE_GUIDE.md)。
- Blender 网格名称对照：**全部通过**。
- Unity 编译、Shader 编译、Test Runner、实际 XR 操作和 Quest 性能：**未执行**。

本报告不能替代 Unity 的实际编译和运行结果。

## 覆盖内容

### 源码与程序集

- C# 文件括号和结构。
- `VRChemistry.Runtime` 的 Core、XR、Input、VFX、TMP、UI、URP 引用。
- `VRChemistry.PlayModeTests` 对运行时代码和 XRI 的引用。
- `Score` 不再直接调用 Runner 的旧状态写入方法。
- `LegacyExperimentAdapter` 的事件订阅和场景挂载。

### P0 流程模拟

- 氧气耗尽后，同项目重新开始能初始化新批次。
- 同批次重复初始化不会补氧。
- 制氧从空瓶开始，新批次清除旧收集量。
- 收集必须满足加热步骤和 120 的最低阈值。
- Score → LegacyExperimentAdapter → Runner 的归位状态同步。
- 垃圾桶禁用器材后清除归位状态。
- 火柴销毁后重建、同帧去重和 Socket 选择状态。

### 材质状态模拟

- 高亮保留原始 alpha。
- 高亮不覆盖液位范围和 Wobble。
- 多所有者高亮在移除一个所有者后仍保持另一个所有者。
- 最后一个所有者移除后恢复颜色，不影响 Wobble。

### P1 模型与 LOD

- `a.fbx`、洗手盆和试管架的 LOD1/LOD2 文件均存在且三角面减少。
- `a_PROXY.fbx` 和 `rack_PROXY.fbx` 均存在且面数下降。
- 所有模型 `.meta` 文件存在。
- 优化前后网格名称对照无缺失、无歧义。
- LOD 工具允许已有 LODGroup 的对象补接碰撞代理。
- LOD 工具支持 `sceneOpened` 自动检查。

## 重要发现并修复

第一次 Blender 导出虽然完成了减面，但同一进程重复导入造成 Mesh 数据名称后缀变化，导致 Unity 按名称接入会全部跳过。

已修改 `Tools/optimize_models.py`，每次导入前重置 Blender 场景数据，并重新生成所有 LOD/代理。随后用 `Tools/check_lod_names.py` 验证：

```text
a.fbx -> a_LOD1.fbx   missing=[] ambiguous=[]
a.fbx -> a_LOD2.fbx   missing=[] ambiguous=[]
basin   -> LOD1/LOD2  missing=[] ambiguous=[]
rack    -> LOD1/LOD2  missing=[] ambiguous=[]
```

## 可重复执行

逻辑模拟：

```powershell
C:\Users\Cherry\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe Tools/simulate_p0_p1.py
```

模型名称检查（需要 Blender）：

```powershell
$env:VR_PROJECT_ROOT = 'C:\Users\Cherry\Desktop\VR_Chemisry_Demo'
blender.exe -b --factory-startup --python 'C:\Users\Cherry\Desktop\VR_Chemisry_Demo\Tools\check_lod_names.py'
```

JSON 结果：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_P0_P1_simulation.json`

## 仍需 Unity 验收

1. 打开 `C:\Users\Cherry\Desktop\VR_Chemisry_Demo`，等待 C#、Shader 和资源导入。
2. 等待中文字体自动修复；必要时手动运行 **VR Chemistry → Repair Chinese Font**。
3. 检查 LOD 自动接入日志；必要时手动运行 **VR Chemistry → Optimization → Apply LOD Assets**。
4. 运行 37 个 EditMode 和 11 个 PlayMode 测试。
5. 验证 MatchSocket 实际选择、抓取、碰撞、液面和 LOD 切换。
6. 之后再进入 P2 的 Profiler、CI、OpenXR 和发布验证。

本次模拟新增的脚本位于 `Tools/simulate_p0_p1.py` 和 `Tools/check_lod_names.py`，未修改 Unity 场景运行结果。
