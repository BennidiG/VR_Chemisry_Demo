# P1 架构与模型优化指南

## 1. 架构收敛

### 唯一旧状态适配器

新增 `Assets/Scripts/InitialDefinition/LegacyExperimentAdapter.cs`，挂在 `Prepare_On_Position` 上。

- `Score` 保留旧 UnityEvent 和旧布尔字段，作为兼容层。
- `Score.LegacyStateChanged` 在旧状态变化时发出事件。
- `LegacyExperimentAdapter` 是唯一将旧字段写入 `ExperimentRunner` 的组件。
- `Score.SyncRunnerState()` 保留为兼容入口，但只发出事件，不再直接写 Runner。
- Runner 的 `SetStepCompletedFromLegacy` / `SetCleanupCompletedFromLegacy` 仅由该适配器调用。

这仍然保留旧碰撞流程，后续可以逐步把碰撞脚本直接改为 Runner 事件，而不必一次性重写 XR 交互。

### XRI 重置

- `LaboratoryLayoutController` 在恢复父级和 Transform 前，显式执行 `SelectExit` 和 `HoverExit`。
- 保存并恢复 `XRGrabInteractable.enabled`。
- `CreateMatch` 重建火柴后刷新 `MatchActionRelay` 缓存。
- 新火柴下一帧通过 `SelectEnter` 尝试恢复 MatchSocket 的实际选择，而不是仅修改 `startingSelectedInteractable`。
- 同帧重复重置有保护，但“先销毁火柴再重置”仍允许重建。

### 材质状态

新增 `Assets/Scripts/Experiments/Core/LabMaterialState.cs`：

- 按 Renderer + materialIndex 保存共享属性块。
- 高亮按 LabItem 记录所有者。
- 取消高亮只恢复颜色/自发光字段，不整块回滚液位、Wobble 或其他效果。
- Wobble 和溶解效果也通过统一入口写入。
- `LabItem` 保留材质槽 alpha，试管架与试管的嵌套高亮不会互相覆盖。

持久液位仍使用缓存材质实例，避免动态属性块覆盖 `_MinHeight/_MaxHeight`。

## 2. 实际模型优化结果

已使用 Blender 4.2.3 LTS 在临时目录中生成真实减面文件，原始 FBX 未覆盖。输出目录：

`Assets/Modle/Optimized/`

| 模型 | 原三角面 | LOD1 | LOD2 | LOD3 | 凸包代理 |
|---|---:|---:|---:|---:|---:|
| `a.fbx` | 753,292 | 338,097（约45%） | 150,210（约20%） | 60,455（约8%） | 12,937 |
| `18中式陶瓷洗手盆.fbx` | 78,336 | 39,166（约50%） | 19,575（约25%） | 9,384（约12%） | 未替换凹形静态碰撞 |
| `TestTubeRack.fbx` | 25,956 | 12,978（约50%） | 6,484（约25%） | 3,112（约12%） | 1,668 |

完整统计见 `Assets/Modle/Optimized/optimization_report.json`。

文件大小示例：

- `a_LOD1.fbx`：约 9.84 MiB。
- `a_LOD2.fbx`：约 4.82 MiB。
- `a_PROXY.fbx`：约 0.90 MiB。
- `rack_PROXY.fbx`：约 0.13 MiB。

这些是实际生成文件的统计，不是估算，也不是只修改导入参数。

## 3. Unity 中接入 LOD

新增 `Assets/Editor/ModelOptimizationTools.cs`：

- 菜单：**VR Chemistry → Optimization → Apply LOD Assets**。
- 打开主场景后，编辑器会尝试自动执行一次；同一会话稍后才打开主场景时，也会通过 `sceneOpened` 再次检查。可以手动重复执行。
- 按原始网格名称匹配 LOD1/LOD2，名称不唯一时拒绝猜测并跳过。
- 创建 LOD0（原始）/LOD1/LOD2/LOD3 Renderer 和 LODGroup，并复制 Wobble、液位及 cap 配置。
- Android 通过 `LabQuestLodController` 从 LOD2 开始，避免近景默认使用原始高模或 LOD1；已有三级 LODGroup 会补建 LOD3。
- 过渡屏幕比例：`0.65 / 0.25 / 0.01`。
- 仅对带非 Kinematic Rigidbody 的凸 MeshCollider 使用凸包代理。
- 洗手盆等静态凹形碰撞体保留原状，不用凸包替代。

重新生成模型的命令（需要本机 Blender 4.2+）：

```powershell
$env:VR_PROJECT_ROOT = 'C:\Users\Cherry\Desktop\VR_Chemisry_Demo'
blender.exe -b --factory-startup --python 'C:\Users\Cherry\Desktop\VR_Chemisry_Demo\Tools\optimize_models.py'
```

脚本位于 `Tools/optimize_models.py`，会重新生成 LOD、代理和统计报告，并在每次导入前重置 Blender 数据以保持网格名称可匹配。`Tools/check_lod_names.py` 可独立验证名称对照；当前 a、洗手盆和试管架的 LOD1/LOD2 均无缺失或歧义。

## 4. 回归测试

当前共有 **37 个 EditMode + 11 个 PlayMode = 48 个测试**。新增 P1 覆盖：

- `LegacyAdapter_OnlyWriterUpdatesRunner`：验证适配器同步和撤销归位状态。
- `MatchReset_RestoresSocketSelection`：验证火柴重建、数量唯一和 MatchSocket 实际选择。

仍需在 Unity 中执行 Test Runner，并验证：

- LOD 网格名称匹配和视觉外观。
- 高亮在 LOD 切换后仍然正确。
- 凸包代理与抓取、药品、塞子、加热碰撞的兼容性。
- XRI 重建后 MatchSocket 的选择和准备阶段。
- 连续重置后的材质恢复和火柴绑定。

## 5. 备份与限制

P1 修改前备份：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_P1_backup_20261113_171548`

当前环境没有运行 Unity，因此以下不能宣称已通过：

- C# 语义编译和 Unity Test Runner。
- LODGroup 实际切换和模型外观。
- XRI 选择恢复和真实抓取。
- Shader 编译、Quest 帧率和透明排序。

`a.fbx` 的 LOD1/LOD2 仍比原模型小很多，但属于全部几何统计；主场景只使用其中一部分，实际收益需以 Unity 导入后的三角形和 Frame Debugger 为准。
