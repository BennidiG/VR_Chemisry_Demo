# 阶段 1：建立化学核心

## 1. 目标与边界

本阶段建立了可独立测试的化学核心，并将现有制氧和复燃接入兼容适配层。它不是完整化学实验室，也不追求分子级模拟；当前目标是让物质、用量、反应条件、产物和质量守恒可计算、可复用、可验证。

本次修改均在 `C:\Users\Cherry\Desktop\VR_Chemisry_Demo`。

## 2. 核心结构

### 有明确生命周期的反应过程

`ReactionEngine.Evaluate()` 保持纯计算，不直接修改混合物。`ReactionProcess` 负责按时间提交：

- `Start()` 计算一次完整化学计量，保存初始混合物快照。
- `Tick(deltaTime)` 按 `ProgressPerSecond` 消耗药品并生成气体、液体、水溶液和固体。
- 条件暂时不满足时进入 `Paused`，条件恢复后继续。
- 药品不足或数据非法时进入 `Failed`，不会先消耗一部分再失败。
- `Completed` 后继续 Tick 不会重复产气或消耗；只有实际 `Progress >= 1` 才完成，不用提前 epsilon 完成。
- 最终 Tick 被截断时，`ElapsedSeconds` 只累计实际反应时间；零进度增量不会误完成。
- 每一步消耗和产物提交都检查结果，失败用 Tick 前快照回滚；消耗容差按数值量级限制，避免少耗多产。
- 多气体/固体产物先累计预检总量，避免单项通过但整 Tick 溢出。
- `Cancel()` 停止过程，`Reset()` 默认恢复初始混合物，`Reset(false)` 只重置进度而保留当前混合物。
- `ChemistrySimulationService.StartProcess()` 可从反应资产创建并启动过程。

`MixtureState` 现在按物质分别保存 `GasComponents` 和 `SolidComponents`，`GetAmount()` 同时考虑液体、气体和固体，生成的氧气可以继续作为下一步复燃的反应物。`Clone()/CopyFrom()` 用于重置时恢复完整状态。

### 纯 C# 核心：`VRChemistry.Chemistry.Core`

目录：`Assets/Scripts/Chemistry/Core`

- `ChemicalSubstance`：物质 ID、名称、化学式、摩尔质量、状态和危险等级。
- `ReagentPortion`：物质 ID、物质的量、体积和浓度。
- `MixtureState`：容量、当前体积、温度、pH、沉淀、气体和药品组成。
- `ReactionParticipant`：物质、系数、角色和相态。
- `ReactionCondition`：温度、催化剂、点燃、余烬、氧气、混合、水、电解和收集条件。
- `ReactionContext`：混合物、催化剂、加热、点燃、余烬、氧气、混合和收集状态。
- `ReactionResult`：成功/失败原因、限制试剂、进度、反应物消耗、气体/液体/固体产物、热量和质量守恒。
- `ReactionEngine`：条件检查、限制试剂计算和产物计算。

核心程序集不引用 UnityEngine，可单独进行化学逻辑测试。

### Unity 资产与服务：`VRChemistry.Runtime`

- `SubstanceDefinitionAsset`：物质 ScriptableObject，包含摩尔质量、状态、危险等级和显示颜色。
- `ReactionDefinitionAsset`：反应 ScriptableObject，包含反应物、产物、条件、进度和安全说明。
- `ChemistryDatabase`：从 `Resources/Chemistry` 加载物质和反应，检查重复 ID、非法配置和未知物质。
- `ChemistrySimulationService`：提供反应求值，并根据摩尔质量计算输入/输出质量。
- `ChemistryLegacyAdapter`：把现有 MakeO2/O2IntoBottle/VisualEffectGraph 的操作转换成化学上下文。

服务已挂到 `Prepare_On_Position`，启动时加载反应资产。

## 3. 当前反应资产

### 制氧：`make_oxygen`

```text
2KClO3 -> 2KCl + 3O2
```

- 反应物：`kclo3`，系数 2。
- 催化剂：`mno2`。
- 产物：`kcl`（固体）和 `o2`（气体），系数分别为 2、3。
- 条件：至少 200°C、MnO₂、连接气体收集装置。
- 当前兼容用药品包视为 KClO₃/MnO₂ 组合，不再用无身份的 Powder 参与核心计算。
- 进度：`0.4/s`，产氧速率与旧逻辑兼容为 60 个显示单位/秒。

在 `0.0001 mol KClO3` 下：

- 限制试剂量：`0.00005 mol` 反应进度。
- KCl：`0.0001 mol`。
- O₂：`0.00015 mol`。
- 兼容显示产氧量：`0.00015 × 0.4 × 1,000,000 = 60/s`。

### 复燃：`match_refire`

```text
C + O2 -> CO2
```

- 需要点燃条件、热余烬和氧气。
- 氧气不足或余烬冷却时，反应失败并返回原因。
- 产物 CO₂ 不支持燃烧。
- 复燃过程中消耗氧气，保留现有 `OxygenSupply.Amount` 和 `AmountChanged` 显示兼容。
- `AmountMol` 使用 `1 mol = 1,000,000` 个兼容显示单位，初始 10000 单位等于 0.01 mol。

### 中和：`neutralization`

```text
HCl + NaOH -> NaCl + H2O
```

- 用于验证核心的限制试剂、混合条件、液体产物和后续扩展。
- NaCl 记录为水溶液产物，水记录为液体产物，避免混淆。
- 当前只作为核心扩展示例，尚未添加完整实验流程和器材。

## 4. 旧流程如何迁移

```text
旧 Powder / 加热 / 收集 / 余烬操作
              ↓
       ChemistryLegacyAdapter
              ↓
      ReactionContext + ReactionDefinition
              ↓
        ReactionEngine / ChemistrySimulationService
              ↓
      条件结果、产物、质量守恒、旧 UI/VFX
```

- `MakeO2.HotEnough()` 先调用制氧核心，成功后才允许 `MakeingO2True()`。
- `O2IntoBottle` 通过核心计算产氧量，再按 `ProgressPerSecond` 累积到 `OxygenSupply`。
- `VisualEffectGraph.refire()` 先验证余烬和氧气，再执行原有复燃表现。
- 原有步骤、分数、UnityEvent、XR Socket 和 `ExperimentRunner` 不被强行重写。
- `ExperimentRunner` 负责流程和评分，化学服务负责反应结果。

## 5. 质量守恒

`ChemistrySimulationService` 在反应成功后计算：

```text
InputMass  = Σ（反应物消耗摩尔量 × 摩尔质量）
OutputMass = Σ（各产物摩尔量 × 摩尔质量）
MassBalanceError = OutputMass - InputMass
```

同时检查未知物质，避免物质缺失时把质量平衡错误隐藏为 0。

## 6. 测试与复核

### 本次独立复核修复

- 反应资产中的 `SubstanceId/Coefficient/Role/Phase` 和条件字段统一为 Unity 序列化所需的 PascalCase，避免物质 ID 为空。
- `ChemistryUnits.GasUnitsPerMole` 放到下层 `VRChemistry.Experiments.Core`，避免 `OxygenSupply` 反向引用 Runtime 形成循环依赖。
- 重复物质条目按 ID 汇总系数和产物；非法系数、NaN、无穷、空参与者和 null ID 返回失败而不抛异常。
- 负温度条件受支持，`HasMinimumTemperature` 可显式要求最低 0°C。
- 混合物合并、消耗后重新计算浓度和体积，容量检查拒绝非法数值。
- 无效反应资产不会注册，未知物质会进入质量守恒错误状态。
- 制氧气泡和产氧量均读取实际导管连接状态，核心失败时停止气泡。

新增 26 个纯 C# 化学核心测试，覆盖：

- 制氧的限制试剂、KCl/O2 系数和 60 单位/秒兼容速率。
- 反应过程的时间推进、暂停/恢复、完成幂等、失败不部分提交、取消和快照重置。
- 气体/固体按物质组成保存，生成氧气可继续参与复燃。
- 加热、催化剂和收集条件。
- 复燃的余烬、氧气、CO₂ 和放热。
- 中和的限制试剂、NaCl 和水。
- 混合条件。
- 容器容量和药品消耗。
- 非法系数。

新增 3 个 PlayMode 化学集成测试：

- `Chemistry_MakeOxygen_UsesCoreAndConservesMass`
- `Chemistry_Refire_RequiresEmberAndConservesMass`
- `Chemistry_StartProcess_UsesAssetsAndCommits`

当前测试总数：**37 个 EditMode + 11 个 PlayMode = 48 个**。

Python 化学模拟：**79 / 79 通过**，包括 KClO₃、复燃、中和、质量守恒、容量、氧气单位换算、重复物质、非法数值、负温度、浓度、反应过程生命周期、精确尾段、溢出回滚和深拷贝。

五轮全场景复核：**240 项检查通过**；P0/P1 模拟：**350 / 350 通过**；化学模拟：**79 / 79 通过**。

## 7. 当前限制

- 反应速率是教学用的简化速率，不是动力学或热力学模拟。
- 液体产物按物质的量提交，暂不自动换算液体体积；气体压力/体积、pH 曲线和温度自动变化仍需后续模型。
- 同一 `MixtureState` 默认由一个 `ReactionProcess` 管理；`Reset(true)` 是整份混合物快照回滚，会覆盖同容器上的其他过程或外部改动。
- `MixtureState` 的温度、pH、沉淀和气体字段已建立，但当前实验只使用其中一部分。
- 现有 Powder 是兼容用的 KClO₃/MnO₂ 药品包，后续应拆成独立药品、用量和添加操作。
- 失败原因目前主要是英文，下一阶段应接入中文/英文提示和可操作纠错。
- 还没有自由加药、任意混合、pH 曲线、过滤、蒸馏和完整实验目录。
- 尚未在 Unity Editor、Test Runner 或 Quest 上执行真实验证。

## 8. 下一阶段建议

阶段 2 应实现 `LabItemInstance`、药品容器和操作意图：

- 独立器材实例 ID。
- `AddReagent`、`Mix`、`Heat`、`CollectGas`、`Measure`、`Dispose`。
- 按容器保存混合物、用量和条件。
- 通过 `ActionIntent` 驱动物理交互，而不是只依赖对象名称。
- 用 `ObservationBook` 记录操作、测量和反应现象。
- 让自由练习与指定实验共享同一个化学核心。

阶段 1 修改前备份：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_phase1_backup_20261117_181047`

本次核心深化优化前备份：

`C:\Users\Cherry\AppData\Local\Temp\VR_Chemistry_phase1_review_backup_20261119_164100`
