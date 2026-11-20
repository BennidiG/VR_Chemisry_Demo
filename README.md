# VR Chemistry Lab

面向 **Oculus Quest 2** 的 Unity VR 化学实验室原型。玩家通过手柄射线、抓取和 Socket 装配器材，按照步骤完成实验，并根据操作与器材归位情况获得评分。

## 当前实验

- **制取氧气**：取药、装配试管与导管、固定、加热、收集氧气。
- **火柴复燃**：点燃火柴、甩灭并保留火星、在限时内放入氧气瓶复燃。

## 技术环境

- Unity `2020.3.30f1c1`
- URP / Visual Effect Graph `10.8.1`
- XR Interaction Toolkit `2.0.1`
- Oculus XR `1.11.2`
- Input System
- Android：ARM64、IL2CPP、Oculus Loader

首次打开工程时，让 Unity 按 `Packages/manifest.json` 恢复依赖并完成资源导入。

## 场景

- `Assets/Scenes/SampleScene.unity`：主实验场景，也是当前唯一启用的构建场景。
- `Assets/Scenes/Test.unity`：模型展示与导入检查场景，不包含完整 XR 交互系统。

## 代码结构

- `Assets/Scripts/ExperimentalFlowCode`：制氧、燃烧、氧气收集与药品逻辑。
- `Assets/Scripts/Experiments/Core`：实验定义、统一状态机、器材标识和本地化核心程序集。
- `Assets/Scripts/Experiments`：步骤/归位引导、高亮、语言与模式切换、反馈和编辑器调试。
- `Assets/Resources/Experiments`：两个实验的步骤、器材、权重、归位条件和中英文资产。
- `Assets/Scripts/InitialDefinition`：实验准备、步骤、提示、计分、器材生成与当前火柴事件转发。
- `Assets/Scripts/Tirgger`：碰撞触发与交互效果（目录名沿用原工程拼写）。
- `Assets/Scripts/SceneObjectFinder.cs`：场景对象查找，支持未激活的步骤面板。

## 构建与验证

1. 使用上述 Unity 版本打开工程并确认 Console 无编译错误。
2. 在编辑器 Play Mode 中分别完整执行两个实验。
3. 验证火柴丢弃/重建、实验切换、器材移出、双 O2 碰撞体边缘移动、烧尽、氧气耗尽和重复重置。
4. 使用 Android、ARM64、IL2CPP 构建 APK，并在 Quest 2 上验证移动、射线、抓取、Socket、VFX 与中文 UI。

详细的本次修复清单、验证边界、真机测试清单和后续开发路线见 [PROJECT_REVIEW.md](PROJECT_REVIEW.md)。实验资产、教学模式、中英文切换、无头显调试按键和 EditMode 测试见 [FRAMEWORK_GUIDE.md](FRAMEWORK_GUIDE.md)。新增的 URP 玻璃、液体、器材与气泡材质及调参说明见 [MATERIALS_GUIDE.md](MATERIALS_GUIDE.md)。覆盖代码架构、编译、测试、化学流程、材质模型、性能与资源发布的全面优化路线见 [OPTIMIZATION_REVIEW.md](OPTIMIZATION_REVIEW.md)。三阶段实施结果和 Unity 内待执行步骤见 [OPTIMIZATION_IMPLEMENTATION.md](OPTIMIZATION_IMPLEMENTATION.md)。P1 的真实模型减面、LOD 接入和凸包代理见 [MODEL_OPTIMIZATION_GUIDE.md](MODEL_OPTIMIZATION_GUIDE.md)。无 Unity 环境下的 P0/P1 模拟验收结果见 [SIMULATION_REPORT.md](SIMULATION_REPORT.md)。P2 的性能采样、项目验证、CI 和发布步骤见 [P2_PERFORMANCE_RELEASE.md](P2_PERFORMANCE_RELEASE.md)。五轮全场景检查与修复记录见 [FULL_SCENE_CHECK.md](FULL_SCENE_CHECK.md)。最新全面性能优化见 [PERFORMANCE_IMPLEMENTATION.md](PERFORMANCE_IMPLEMENTATION.md)。模拟实测与几何检查报告见 [SIMULATED_RUNTIME_TEST.md](SIMULATED_RUNTIME_TEST.md)。阶段 1 化学核心、反应资产与兼容迁移见 [CHEMISTRY_CORE_GUIDE.md](CHEMISTRY_CORE_GUIDE.md)。

> 当前审查环境没有 Unity 编辑器和 Quest 设备，因此已完成静态检查，但未宣称 Unity 编译、Play Mode 或真机测试通过。
