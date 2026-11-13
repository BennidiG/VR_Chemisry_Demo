# 材质与液体效果使用指南

本轮新增 4 个 URP Shader 和 9 个材质，已接入实验场景。旧材质均保留，便于对照和回退。

## 1. 新增 Shader

目录：`Assets/Shaders/Lab`

| Shader | 用途与特点 |
|---|---|
| `LabGlass.shader` | 透明底色、Fresnel 边缘反射、简化高光；不使用 GrabPass 或真实折射。 |
| `LabLiquid.shader` | 局部高度液位裁剪、顶部泡沫、正弦波扰动、边缘光，并响应 `_WobbleX/_WobbleZ`。 |
| `LabEquipment.shader` | 不透明器材材质，主光漫反射、高光、Metallic/Smoothness 和自发光。 |
| `LabBubble.shader` | 透明气泡粒子，支持贴图、Tint 和顶点色。 |

实现均使用 `Core.hlsl` 与 `Lighting.hlsl`，不包含 URP 10.8.1 不存在的 `Unlit.hlsl`。`LabEquipment` 另含 `ShadowCaster` Pass，并在 Forward 中采样主光阴影，支持投影和接影。

## 2. 材质与场景接入

目录：`Assets/Materials/Lab`

| 材质 | 接入情况 |
|---|---|
| `LabGlass.mat` | 替换 18 处普通玻璃引用。 |
| `LabGlass_Frosted.mat` | 替换 9 处磨砂玻璃引用。 |
| `LabLiquid_Water.mat` | 替换水面及旧液体材质，共 10 处。 |
| `LabLiquid_Alcohol.mat` | 替换酒精灯内液体材质。 |
| `LabEquipment_Metal.mat` | 替换 11 处金属材质，移除不必要的透明裁剪。 |
| `LabEquipment_Ceramic.mat` | 陶瓷材质预设，可分配给洗手盆等器材。 |
| `LabBubble.mat` | 替换旧气泡材质，并补上 `ParticleBubbles` 原先为空的材质字段。 |
| `LabDesk.mat` | 保留实验台原贴图、法线和金属度图，降低过强自发光。 |
| `LabConsole.mat` | 保留控制台贴图，降低原先过高的蓝色自发光。 |

## 3. 液位与泡沫调参

`LabLiquid` 使用**相对容器世界原点的世界 Y 高度**，避免容器旋转时液面完全跟随模型坐标轴旋转：

```text
surfaceHeight = originWorldY + lerp(_MinHeight, _MaxHeight, _FillAmount)
```

- `_MinHeight` / `_MaxHeight`：相对容器世界原点的米制最低、最高偏移。
- `_FillAmount`：0 表示最低液位，1 表示最高液位。
- `_FoamWidth`：液面下侧泡沫过渡带宽度。
- `_WaveAmplitude`：波动幅度，建议小值，避免液面穿出器皿。
- `_WaveFrequency` / `_WaveSpeed`：波动频率与速度。
- `_WobbleX` / `_WobbleZ`：由现有 `Wobble.cs` 根据器材运动自动写入，产生轻微倾斜与晃动。

场景中的 11 个液体 Renderer 已添加 `LabLiquidVolume`：Play 时按各自世界包围盒自动设置 `_MinHeight/_MaxHeight`，避免 Cylinder、Sphere 和 LiquidLevel 共用错误高度；`localScale` 改变时会重新计算。材质中的 `-0.05/0.05` 仅作为未运行组件时的预览/回退值；关闭 `autoFitToRenderer` 后可使用 `heightOverride` 手动调整。

- 水：`_FillAmount=0.65`。
- 酒精：`_FillAmount=0.55`。

如果液面过高、过低或整个液体被裁掉，优先检查该对象的 `LabLiquidVolume` 与包围盒，再调整高度覆盖或填充值。液体只裁剪已有网格，不自动生成任意高度的截面封盖；需要完整封顶的模型应保留/配置独立液面网格。

## 4. 玻璃与器材调参

### 玻璃

- `_BaseColor.a`：基础透明度。
- `_FresnelAlpha`：斜视时边缘透明度增量。
- `_RimPower`：边缘光范围，越大越集中。
- `_RimStrength`：边缘反射亮度。
- `_SpecularStrength`：简化高光强度。
- `_Cull=2`：清透玻璃使用背面剔除，磨砂玻璃和液体保留双面设置。
- `_ZWrite=0`：关闭透明写入，液体 Queue 为 3000，玻璃为 3010。

### 金属与陶瓷

- `_Metallic`：金属度，影响高光比例，不是完整物理 PBR。
- `_Smoothness`：高光集中度。
- `_SpecularStrength`：高光强度。
- `_Ambient`：简化环境补光，避免器材在背光面全黑。

`LabEquipment_Ceramic.mat` 是预设，未强制替换现有水盆材质。

## 5. 性能边界

- 不使用 GrabPass、真实折射、屏幕模糊、几何 Shader 或 Compute Shader。
- 新 Shader 均为单 Pass，并只使用主光的近似光照，没有逐像素附加灯循环。
- 保留原 High 配置，新增独立 Quest 配置；没有直接改写原 High 的 URP、分辨率、MSAA 或阴影参数。
- 透明玻璃和液体仍会产生 overdraw；在 Quest 上应避免大量重叠器皿同时占据屏幕。前后面不做逐三角形排序，复杂网格需检查自身透明穿插。
- 教学高亮只改变颜色/自发光，保留玻璃和液体的原始 alpha，不会因高亮变得不透明；自定义 Lab Shader 支持自发光脉冲，URP Lit 材质以基色脉冲为主。
- 持久液位使用每个对象缓存的材质实例，避免动态属性块覆盖；Wobble 和短时高亮使用 MaterialPropertyBlock。MaterialPropertyBlock 会使对应对象退出 Unity 2020.3 的 SRP Batcher，净收益需用 Frame Debugger/Profiler 验证。
- 法线和贴图较完整的实验台继续使用 URP Lit，以免简化 shader 破坏原有表面细节。

建议先在 Unity Editor 中检查透明排序、液面位置、材质高亮恢复和气泡显示，再在 Quest 上记录 CPU/GPU 帧时间。若性能不足，优先减少透明层数、实时灯和阴影，而不是先增加更复杂的光照效果。

## 6. 验证情况与备份

已完成静态检查：

- 新 Shader 的括号、URP 标签和 include 路径检查；并对照 URP 10.8.1 官方 [Core.hlsl](https://raw.githubusercontent.com/Unity-Technologies/Graphics/v10.8.1/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl)、[Lighting.hlsl](https://raw.githubusercontent.com/Unity-Technologies/Graphics/v10.8.1/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl) 与 [ShaderVariablesFunctions.hlsl](https://raw.githubusercontent.com/Unity-Technologies/Graphics/v10.8.1/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl) 确认函数来源。
- 新材质与 Shader GUID 对照。
- 旧材质引用清理和新材质引用数量核对。
- 场景重复 fileID 与悬空组件检查。
- `_WobbleX/_WobbleZ` 和教学高亮所需的 `_BaseColor/_EmissionColor` 属性检查。

当前环境没有 Unity 编辑器，因此**未实际执行 Shader 编译、导入或画面验证**。首次打开工程后，请先确认 Console 没有 Shader 编译错误，再检查透明效果。

本轮原资产备份：`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_material_backup_20261113_104043`。
