# 模拟实测报告

本次在没有 Unity Editor、.NET SDK 和 Quest 设备的环境中，进行了实验流程模拟、Quest 构建模拟、Blender 几何检查和五轮全场景复核。结果不代表 Unity、APK 或 Quest 实测通过。

## 1. 模拟结果汇总

| 测试套件 | 结果 |
|---|---:|
| `Tools/simulate_runtime.py` 流程与构建模拟 | **810 / 810 通过** |
| `Tools/full_scene_check.py` 五轮全场景检查 | **240 / 240 通过** |
| `Tools/simulate_p0_p1.py` P0/P1 逻辑模拟 | **350 / 350 通过** |
| `Tools/simulate_chemistry.py` 化学核心模拟 | **79 / 79 通过** |
| `Tools/check_lod_geometry.py` Blender 几何检查 | **通过** |
| `Tools/check_lod_names.py` LOD 名称匹配 | **无缺失、无歧义** |

这些检查是不同套件，不能简单相加为“Unity 测试数”。

## 2. 实验流程模拟

已按帧/步骤模拟：

- 制氧步骤按顺序完成，跳步和重复加热被拒绝。
- 制氧从空瓶开始，新批次清空旧收集量。
- 加热前不能完成收集，累计 120 个生产单位后才能完成收集。
- 复燃初始化氧气为 10000。
- 复燃消耗氧气，120 帧后减少 120 单位。
- 氧气耗尽后，同项目重新开始能恢复氧气。
- 同一批次重复初始化不会补氧。
- 火柴销毁后可重建，Socket 恢复选择。
- 同帧重复重建被抑制。
- 旧状态适配器可以写入和撤销归位状态。
- 垃圾桶禁用器材并清除归位分。

## 3. Quest 与构建模拟

已验证模拟逻辑：

- Android 使用 LOD2/LOD3，而不是原始模型和 LOD1。
- 原始模型和 LOD1 是构建剔除目标。
- LOD2/LOD3 的三角面严格低于原始模型。
- 构建成功和构建异常都能恢复原始 LOD 链。
- Android cap 使用 24 段网格。
- cap 位于液面裁剪面下方，随当前 LOD 可见性变化。
- 已有 `LiquidLevel` 不重复创建 cap。
- 物理 Solver 预算为 4 / 1。
- Android VFX 播放速率、粒子预算、波动幅度和气泡自发光降低。

## 4. Blender 几何检查

### 网格健康

- 原始 `a.fbx` 的 **377 个退化三角形全部清理为 0**。
- LOD 和凸包代理均未发现 NaN 或非法坐标。
- 原始模型与 LOD 的网格名称无缺失、无歧义。
- UV 数量完全保留：
  - `a.fbx`：102 → 102 个 UV 网格。
  - 洗手盆：27 → 27 个 UV 网格。
  - 试管架：5 → 5 个 UV 网格。
- 凸包代理的退化三角形也为 0。

### 清理后的实际统计

| 模型 | LOD1 | LOD2 | LOD3 | 代理 |
|---|---:|---:|---:|---:|
| `a.fbx` | 338,097 | 150,210 | 60,455 | 12,937 |
| 洗手盆 | 39,166 | 19,575 | 9,384 | 未替换凹形碰撞 |
| 试管架 | 12,978 | 6,484 | 3,112 | 1,668 |

所有模型均保留在 `Assets/Modle/Optimized/`，原始 FBX 未覆盖。

## 5. 五轮全场景复核

| 轮次 | 内容 | 结果 |
|---|---|---|
| 第 1 轮 | 场景结构、组件归属、对象引用 | PASS |
| 第 2 轮 | UnityEvent、公开方法、程序集 | PASS |
| 第 3 轮 | 实验流程、氧气、重置、材质状态 | PASS |
| 第 4 轮 | Shader、材质、LOD 和模型名称 | PASS |
| 第 5 轮 | CI、构建、Quest、发布配置 | PASS |

汇总：

- 场景文档：**1,590**
- GameObject：**364**
- 组件：**1,240**
- 自定义 UnityEvent：**134**
- 五轮检查：**240 项，0 错误**
- P0/P1 模拟：**350 项通过**
- 化学核心模拟：**79 项通过**

完整机器可读报告：

- `Logs/simulated_runtime_test.json`
- `Logs/full_scene_check.json`
- `Logs/lod_geometry_report.json`

## 6. 仍需真实环境验证

以下不能由模拟替代：

1. Unity C# 和 Shader 编译。
2. 中文字体实际生成和 TMP 显示。
3. 37 个 EditMode 和 11 个 PlayMode 测试实际通过。
4. 物理代理抓取、Socket、药品接触和氧气接触。
5. cap 的透明排序、液面和 LOD 切换外观。
6. Android APK 构建和 Quest 实际帧率。
7. Quest 的 Multiview、Subsampled、XR 输入和舒适度。

可重复执行：

```powershell
C:\Users\Cherry\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe Tools/simulate_runtime.py
C:\Users\Cherry\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe Tools/full_scene_check.py
```

几何检查需要 Blender：

```powershell
$env:VR_PROJECT_ROOT = 'C:\Users\Cherry\Desktop\VR_Chemisry_Demo'
blender.exe -b --factory-startup --python 'C:\Users\Cherry\Desktop\VR_Chemisry_Demo\Tools\check_lod_geometry.py'
```

本轮性能优化前备份：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_performance_backup_20261116_102942`

当前修改尚未提交或推送 Git。
