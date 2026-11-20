# 五轮全场景检查报告

## 最终结果

五轮检查均通过，未发现当前剩余的结构、逻辑、资源或发布配置错误。

| 轮次 | 范围 | 结果 |
|---|---|---|
| 第 1 轮 | 场景结构、组件归属、Transform、对象引用 | PASS |
| 第 2 轮 | UnityEvent、公开方法、程序集与测试引用 | PASS |
| 第 3 轮 | 实验流程、氧气、重置、材质状态模拟 | PASS |
| 第 4 轮 | Shader、材质、LOD 文件和网格名称 | PASS |
| 第 5 轮 | CI、构建入口、Quest、发布配置与元数据 | PASS |

- 场景文档：**1,589**
- GameObject：**364**
- 检查组件：**1,221**
- 自定义 UnityEvent 方法：**134**，全部可解析
- P0/P1 模拟检查：**350 / 350 通过**
- 化学核心模拟：**79 / 79 通过**
- 本轮五轮检查：**240 项，0 错误**

完整机器可读报告：`Logs/full_scene_check.json`。

## 检查中发现并修复的问题

### 1. 异常 GUID

发现 14 个自定义 GUID 只有 30 位，不是 Unity 要求的 32 位。已统一修正并同步：

- 场景中的 `m_Script` 引用。
- 新增脚本和 `.meta`。
- 项目验证器的必需 GUID 列表。
- 全场景检查脚本。
- 相关资源引用。

现在 Assets 中所有 `.meta` 的 GUID 均为 32 位，未发现重复 GUID。

### 2. 缺失 Editor 元数据

已补齐：

- `Assets/Editor/ProjectBuildValidation.cs.meta`
- `Assets/Editor/ChineseFontRepair.cs.meta`

并检查了 LOD、运行时程序集、性能采样器和验证器的元数据。

### 3. 检查器误报

- `RectTransform` 使用 `!u!224`，不能按普通 Transform `!u!4` 检查，已修正。
- 垃圾桶实际对象名为 `TrashCan`，不是 `Trash`，已修正检查器。

这些不是场景中的实际错误。

## 各轮详细内容

### 第 1 轮：场景结构

检查：

- 重复 fileID。
- 组件列表与 `m_GameObject` 归属。
- Transform/RectTransform 父子引用。
- `Prepare_On_Position`、`Equipment`、`MatchSocket`、`GlassBottle`、`Cork`、`AlcoholLamp`、`TrashCan`、`Score` 等关键对象。
- 氧气供应、布局重置和旧状态适配器组件。

结果：全部通过。

### 第 2 轮：UnityEvent 和程序集

检查：

- 134 个自定义 UnityEvent 方法均为公开且存在。
- `VRChemistry.Runtime` 的 XR、Input、VFX、TMP、UI、URP 引用。
- PlayMode 对运行时代码和 XRI 的引用。
- Score 不再直接写 Runner。
- 37 个 EditMode 和 11 个 PlayMode 测试数量。

结果：全部通过。

### 第 3 轮：实验和材质状态模拟

验证：

- 氧气耗尽后同项目重新开始。
- 同批次重复初始化不补氧。
- 制氧新批次清空旧收集量。
- 加热步骤和最低收集量。
- 旧状态适配器归位同步。
- 垃圾桶禁用器材后清除归位分。
- 火柴销毁、重建和 Socket 选择。
- 高亮 alpha、Wobble、液位和多所有者恢复。
- 事件驱动计分和液位纯移动节流。

结果：350 项流程模拟和 79 项化学模拟全部通过。

### 第 4 轮：Shader 与模型

验证：

- Equipment 使用 `input.shadowCoord`，包含 ShadowCaster。
- Lab Shader 的 `UnityPerMaterial` 和 URP include。
- 木材材质已迁移到 URP Lab Equipment。
- LOD/代理文件和 `.meta` 存在且三角面减少。
- Blender 原模型与 LOD 网格名称无缺失、无歧义。

结果：全部通过。

### 第 5 轮：构建、CI 和发布

验证：

- `LightRenderingMode.PerVertex` 使用正确。
- 批处理会自动打开 SampleScene。
- 开发/签名构建入口和 `build-info.json`。
- CI PowerShell 语法。
- OpenXR 残留清理，Oculus 必需配置保留。
- ARFoundation/ARSubsystems 清理，SubsystemRegistration 保留。
- Quest Multiview、Subsampled Layout、质量档和交付元数据。

结果：全部通过。

## 仍需 Unity/设备验证

本轮是静态检查和逻辑/资源模拟，不替代 Unity 实测：

1. C# 和 Shader 实际编译。
2. 中文字体实际生成与 TMP 显示。
3. 37 个 EditMode 和 11 个 PlayMode 测试实际通过。
4. LOD 实际切换、液面、透明排序和抓取。
5. Android APK 构建和 Quest 帧率。

可重复执行：

```powershell
C:\Users\Cherry\.dsh\dsh-runtimes\dsh-primary-runtime\dependencies\python\python.exe Tools/full_scene_check.py
```

本轮新增/更新的检查脚本：`Tools/full_scene_check.py`、`Tools/simulate_p0_p1.py`、`Tools/check_lod_names.py`。

当前修改尚未提交或推送 Git。

## 后续优化复核

新增液体 cap、教学 UI 集合复用和氧气文本限频后，再次执行五轮检查：

- **179 项检查，0 错误**。
- **297 项模拟检查全部通过**。
- 10 个液体主体启用运行时 cap，已有 `LiquidLevel` 不重复生成。
- cap 随 Renderer/LOD 可见性显示或隐藏。
- LOD 子对象继承液位、Wobble 和 cap 配置。
- 场景插入脚本曾误删 10 个液位组件，已从备份恢复并用一次性正则替换修正，最终 1,589 个场景文档和全部组件引用均完整。

新增验证覆盖 cap 初始化、可见性、UI 缓存、氧气限频和 LOD cap 配置。

## 全面性能优化复核

最新一轮已覆盖物理代理、LOD3、QuestLow、更新节流、材质分级、纹理预算和性能统计：

- 五轮检查 **240 项，0 错误**。
- 模拟检查 **350 / 350 通过**。
- 化学模拟 **79 / 79 通过**。
- 场景文档 **1,590**，组件 **1,240**。
- LOD3 已生成并加入接入工具，Android 从 LOD1 开始。
- 物理代理保留原 MeshCollider 禁用副本，Trigger 和其他碰撞类型不替换。
- CI 构建准备会打开主场景并应用 LOD、物理代理和纹理预算。

详见 [PERFORMANCE_IMPLEMENTATION.md](PERFORMANCE_IMPLEMENTATION.md)。模拟实测与几何检查报告见 [SIMULATED_RUNTIME_TEST.md](SIMULATED_RUNTIME_TEST.md)：流程模拟 810 项通过，原始 377 个退化面已清理为 0，UV 保留，LOD/代理名称无缺失或歧义。
