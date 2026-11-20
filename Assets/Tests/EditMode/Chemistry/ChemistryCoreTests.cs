using System.Collections.Generic;
using NUnit.Framework;

public class ChemistryCoreTests
{
    private static ReactionDefinition MakeOxygenDefinition()
    {
        return new ReactionDefinition
        {
            Id = "make_oxygen",
            Equation = "2KClO3 -> 2KCl + 3O2",
            Reactants = new List<ReactionParticipant>
            {
                new ReactionParticipant("kclo3", 2F, ReactionParticipantRole.Reactant, SubstanceState.Solid),
                new ReactionParticipant("mno2", 1F, ReactionParticipantRole.Catalyst, SubstanceState.Solid)
            },
            Products = new List<ReactionParticipant>
            {
                new ReactionParticipant("kcl", 2F, ReactionParticipantRole.Product, SubstanceState.Solid),
                new ReactionParticipant("o2", 3F, ReactionParticipantRole.Product, SubstanceState.Gas)
            },
            Conditions = new ReactionCondition { MinimumTemperatureC = 200F, CatalystSubstanceId = "mno2", RequiresGasCollection = true },
            ProgressPerSecond = 0.4F
        };
    }

    private static ReactionDefinition MakeRefireDefinition()
    {
        return new ReactionDefinition
        {
            Id = "match_refire",
            Equation = "C + O2 -> CO2",
            Reactants = new List<ReactionParticipant>
            {
                new ReactionParticipant("carbon", 1F, ReactionParticipantRole.Reactant, SubstanceState.Solid),
                new ReactionParticipant("o2", 1F, ReactionParticipantRole.Reactant, SubstanceState.Gas)
            },
            Products = new List<ReactionParticipant> { new ReactionParticipant("co2", 1F, ReactionParticipantRole.Product, SubstanceState.Gas) },
            Conditions = new ReactionCondition { RequiresIgnition = true, RequiresHotEmber = true, RequiresOxygen = true },
            ProgressPerSecond = 1F,
            Exothermic = true
        };
    }

    private static ReactionDefinition MakeNeutralizationDefinition()
    {
        return new ReactionDefinition
        {
            Id = "neutralization",
            Equation = "HCl + NaOH -> NaCl + H2O",
            Reactants = new List<ReactionParticipant>
            {
                new ReactionParticipant("hcl", 1F, ReactionParticipantRole.Reactant, SubstanceState.Aqueous),
                new ReactionParticipant("naoh", 1F, ReactionParticipantRole.Reactant, SubstanceState.Aqueous)
            },
            Products = new List<ReactionParticipant>
            {
                new ReactionParticipant("nacl", 1F, ReactionParticipantRole.Product, SubstanceState.Aqueous),
                new ReactionParticipant("h2o", 1F, ReactionParticipantRole.Product, SubstanceState.Liquid)
            },
            Conditions = new ReactionCondition { RequiresMixing = true, MinimumMixing = 0.25F },
            ProgressPerSecond = 0.25F,
            Exothermic = true
        };
    }

    private static ReactionContext MakeContext()
    {
        ReactionContext context = new ReactionContext
        {
            IsHeating = true,
            HasHotEmber = true,
            IsIgnited = true,
            HasOxygen = true,
            GasCollectionConnected = true,
            MixingAmount = 1F
        };
        context.Mixture.SetTemperature(250F);
        context.Mixture.AddReagent(new ReagentPortion("kclo3", 0.0001F));
        context.Catalysts.Add("mno2");
        return context;
    }

    [Test]
    public void MakeOxygen_ComputesStoichiometry()
    {
        ReactionResult result = ReactionEngine.Evaluate(MakeOxygenDefinition(), MakeContext());
        Assert.IsTrue(result.Succeeded, result.FailureReason);
        Assert.AreEqual(0.00005F, result.ExtentMol, 0.0000001F);
        Assert.AreEqual(0.00015F, result.GasProduced["o2"], 0.0000001F);
        Assert.AreEqual(0.0001F, result.SolidProduced["kcl"], 0.0000001F);
        Assert.AreEqual(0.0001F, result.ConsumedReagents["kclo3"], 0.0000001F);
        Assert.AreEqual(60F, result.GasProduced["o2"] * result.ProgressPerSecond * 1000000F, 0.001F);
    }

    [Test]
    public void MakeOxygen_RequiresHeatAndCatalyst()
    {
        ReactionContext context = MakeContext();
        context.Mixture.SetTemperature(25F);
        ReactionResult cold = ReactionEngine.Evaluate(MakeOxygenDefinition(), context);
        Assert.IsFalse(cold.Succeeded);
        StringAssert.Contains("Temperature", cold.FailureReason);

        context.Mixture.SetTemperature(250F);
        context.Catalysts.Clear();
        ReactionResult noCatalyst = ReactionEngine.Evaluate(MakeOxygenDefinition(), context);
        Assert.IsFalse(noCatalyst.Succeeded);
        StringAssert.Contains("catalyst", noCatalyst.FailureReason);
    }

    [Test]
    public void MakeOxygen_RequiresCollection()
    {
        ReactionContext context = MakeContext();
        context.GasCollectionConnected = false;
        ReactionResult result = ReactionEngine.Evaluate(MakeOxygenDefinition(), context);
        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains("collection", result.FailureReason);
    }

    [Test]
    public void MatchRefire_RequiresEmberAndOxygen()
    {
        ReactionDefinition definition = MakeRefireDefinition();
        ReactionContext context = new ReactionContext { HasHotEmber = false, HasOxygen = true, IsIgnited = true };
        context.Mixture.AddReagent(new ReagentPortion("carbon", 0.0001F));
        context.Mixture.AddReagent(new ReagentPortion("o2", 0.0001F));
        Assert.IsFalse(ReactionEngine.Evaluate(definition, context).Succeeded);

        context.HasHotEmber = true;
        context.HasOxygen = false;
        Assert.IsFalse(ReactionEngine.Evaluate(definition, context).Succeeded);

        context.HasOxygen = true;
        Assert.IsTrue(ReactionEngine.Evaluate(definition, context).Succeeded);
    }

    [Test]
    public void MatchRefire_ConsumesOxygenAndProducesCarbonDioxide()
    {
        ReactionDefinition definition = MakeRefireDefinition();
        ReactionContext context = new ReactionContext { HasHotEmber = true, HasOxygen = true, IsIgnited = true };
        context.Mixture.AddReagent(new ReagentPortion("carbon", 0.0002F));
        context.Mixture.AddReagent(new ReagentPortion("o2", 0.0001F));
        ReactionResult result = ReactionEngine.Evaluate(definition, context);
        Assert.IsTrue(result.Succeeded, result.FailureReason);
        Assert.AreEqual(0.0001F, result.GasConsumed["o2"], 0.0000001F);
        Assert.AreEqual(0.0001F, result.GasProduced["co2"], 0.0000001F);
        Assert.IsTrue(result.HeatReleased);
    }

    [Test]
    public void Neutralization_UsesLimitingReagentAndTracksLiquidWater()
    {
        ReactionDefinition definition = MakeNeutralizationDefinition();
        ReactionContext context = new ReactionContext { MixingAmount = 1F };
        context.Mixture.AddReagent(new ReagentPortion("hcl", 0.001F));
        context.Mixture.AddReagent(new ReagentPortion("naoh", 0.002F));
        ReactionResult result = ReactionEngine.Evaluate(definition, context);
        Assert.IsTrue(result.Succeeded, result.FailureReason);
        Assert.AreEqual("hcl", result.LimitingReagentId);
        Assert.AreEqual(0.001F, result.AqueousProduced["nacl"], 0.000001F);
        Assert.AreEqual(0.001F, result.LiquidProduced["h2o"], 0.000001F);
    }

    [Test]
    public void Neutralization_RequiresMixing()
    {
        ReactionContext context = new ReactionContext { MixingAmount = 0F };
        context.Mixture.AddReagent(new ReagentPortion("hcl", 0.001F));
        context.Mixture.AddReagent(new ReagentPortion("naoh", 0.001F));
        ReactionResult result = ReactionEngine.Evaluate(MakeNeutralizationDefinition(), context);
        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains("Mix", result.FailureReason);
    }

    [Test]
    public void Mixture_EnforcesCapacityAndConsumption()
    {
        MixtureState mixture = new MixtureState { CapacityMl = 10F };
        Assert.IsFalse(mixture.AddReagent(new ReagentPortion("hcl", 0.001F, 11F)));
        Assert.IsTrue(mixture.AddReagent(new ReagentPortion("hcl", 0.001F, 4F)));
        Assert.IsFalse(mixture.AddReagent(new ReagentPortion("naoh", 0.001F, 7F)));
        Assert.AreEqual(0.001F, mixture.Consume("hcl", 0.002F), 0.000001F);
        Assert.AreEqual(0F, mixture.GetAmount("hcl"), 0.000001F);
    }

    [Test]
    public void DuplicateParticipants_AreAggregatedInsteadOfOverwritten()
    {
        ReactionDefinition definition = MakeNeutralizationDefinition();
        definition.Reactants[0].Coefficient = 0.5F;
        definition.Reactants.Add(new ReactionParticipant("hcl", 0.5F, ReactionParticipantRole.Reactant, SubstanceState.Aqueous));
        definition.Products.Add(new ReactionParticipant("nacl", 1F, ReactionParticipantRole.Product, SubstanceState.Aqueous));

        ReactionContext context = new ReactionContext { MixingAmount = 1F };
        context.Mixture.AddReagent(new ReagentPortion("hcl", 0.001F));
        context.Mixture.AddReagent(new ReagentPortion("naoh", 0.002F));
        ReactionResult result = ReactionEngine.Evaluate(definition, context);
        Assert.IsTrue(result.Succeeded, result.FailureReason);
        Assert.AreEqual(0.001F, result.ConsumedReagents["hcl"], 0.000001F);
        Assert.AreEqual(0.002F, result.AqueousProduced["nacl"], 0.000001F);
    }

    [Test]
    public void NonFiniteValues_AreRejected()
    {
        MixtureState mixture = new MixtureState();
        Assert.IsFalse(mixture.AddReagent(new ReagentPortion("hcl", float.NaN, 1F)));
        Assert.IsFalse(mixture.AddReagent(new ReagentPortion("hcl", float.PositiveInfinity, 1F)));
        Assert.AreEqual(0F, mixture.VolumeMl);

        ReactionDefinition definition = MakeNeutralizationDefinition();
        definition.Reactants[0].Coefficient = float.NaN;
        ReactionContext context = new ReactionContext { MixingAmount = 1F };
        context.Mixture.AddReagent(new ReagentPortion("hcl", 0.001F));
        context.Mixture.AddReagent(new ReagentPortion("naoh", 0.001F));
        Assert.IsFalse(ReactionEngine.Evaluate(definition, context).Succeeded);

        definition = MakeNeutralizationDefinition();
        definition.ProgressPerSecond = float.PositiveInfinity;
        Assert.IsFalse(ReactionEngine.Evaluate(definition, context).Succeeded);
    }

    [Test]
    public void NegativeAndZeroMinimumTemperature_AreHandledExplicitly()
    {
        ReactionDefinition definition = MakeOxygenDefinition();
        ReactionContext context = MakeContext();
        definition.Conditions.MinimumTemperatureC = -20F;
        context.Mixture.SetTemperature(-25F);
        Assert.IsFalse(ReactionEngine.Evaluate(definition, context).Succeeded);

        context.Mixture.SetTemperature(-10F);
        Assert.IsTrue(ReactionEngine.Evaluate(definition, context).Succeeded);

        definition.Conditions.HasMinimumTemperature = true;
        definition.Conditions.MinimumTemperatureC = 0F;
        context.Mixture.SetTemperature(-1F);
        Assert.IsFalse(ReactionEngine.Evaluate(definition, context).Succeeded);
        context.Mixture.SetTemperature(0F);
        Assert.IsTrue(ReactionEngine.Evaluate(definition, context).Succeeded);
    }

    [Test]
    public void NullParticipants_ReturnConfigurationFailure()
    {
        ReactionDefinition definition = MakeOxygenDefinition();
        definition.Reactants.Add(null);
        ReactionResult result = ReactionEngine.Evaluate(definition, MakeContext());
        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains("null participant", result.FailureReason);
    }

    [Test]
    public void Mixture_UpdatesConcentrationOnMergeAndConsume()
    {
        MixtureState mixture = new MixtureState { CapacityMl = 100F };
        Assert.IsTrue(mixture.AddReagent(new ReagentPortion("hcl", 1F, 10F)));
        Assert.IsTrue(mixture.AddReagent(new ReagentPortion("hcl", 1F, 10F)));
        Assert.AreEqual(100F, mixture.Reagents[0].ConcentrationMolPerLiter, 0.00001F);

        Assert.AreEqual(1F, mixture.Consume("hcl", 1F), 0.00001F);
        Assert.AreEqual(100F, mixture.Reagents[0].ConcentrationMolPerLiter, 0.00001F);
        Assert.AreEqual(10F, mixture.VolumeMl, 0.00001F);
    }

    [Test]
    public void ReactionProcess_TickConsumesAndProducesExactlyOnce()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        Assert.IsTrue(process.Start().Succeeded);

        ReactionTickResult first = process.Tick(1F);
        Assert.AreEqual(ReactionRunStatus.Running, first.Status);
        Assert.AreEqual(0.4F, first.Progress, 0.000001F);
        Assert.AreEqual(0.00006F, context.Mixture.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0.00006F, context.Mixture.GetGasAmount("o2"), 0.0000001F);

        ReactionTickResult second = process.Tick(2F);
        Assert.AreEqual(ReactionRunStatus.Completed, second.Status);
        Assert.AreEqual(0.00015F, context.Mixture.GetGasAmount("o2"), 0.0000001F);
        Assert.AreEqual(0.0001F, context.Mixture.GetSolidAmount("kcl"), 0.0000001F);
        Assert.AreEqual(0.0001F, context.Mixture.PrecipitateMol, 0.0000001F);
        Assert.AreEqual(2.5F, process.ElapsedSeconds, 0.000001F);

        float oxygenAfterCompletion = context.Mixture.GetGasAmount("o2");
        ReactionTickResult repeated = process.Tick(10F);
        Assert.AreEqual(ReactionRunStatus.Completed, repeated.Status);
        Assert.AreEqual(0F, repeated.DeltaProgress);
        Assert.AreEqual(oxygenAfterCompletion, context.Mixture.GetGasAmount("o2"), 0.0000001F);
    }

    [Test]
    public void ReactionProcess_PausesAndResumesOnConditions()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        process.Start();
        process.Tick(0.5F);
        context.GasCollectionConnected = false;

        ReactionTickResult paused = process.Tick(1F);
        Assert.AreEqual(ReactionRunStatus.Paused, paused.Status);
        Assert.AreEqual(0.2F, paused.Progress, 0.000001F);
        Assert.AreEqual(0.00008F, context.Mixture.GetAmount("kclo3"), 0.0000001F);

        context.GasCollectionConnected = true;
        ReactionTickResult resumed = process.Tick(0.5F);
        Assert.AreEqual(ReactionRunStatus.Running, resumed.Status);
        Assert.AreEqual(0.4F, resumed.Progress, 0.000001F);
    }

    [Test]
    public void ReactionProcess_InvalidTimeDoesNotMutate()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        process.Start();
        Assert.AreEqual(0F, process.Tick(0F).Progress);
        ReactionTickResult negative = process.Tick(-1F);
        Assert.AreEqual(ReactionRunStatus.Failed, negative.Status);
        Assert.AreEqual(0F, process.Progress);
        Assert.AreEqual(0.0001F, context.Mixture.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0F, context.Mixture.GasAmountMol);

        process.Reset();
        process.Start();
        ReactionTickResult nan = process.Tick(float.NaN);
        Assert.AreEqual(ReactionRunStatus.Failed, nan.Status);
        Assert.AreEqual(0F, context.Mixture.GasAmountMol);
    }

    [Test]
    public void ReactionProcess_InsufficientReagentFailsWithoutPartialCommit()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        process.Start();
        context.Mixture.Consume("kclo3", 0.00009F);

        ReactionTickResult tick = process.Tick(1F);
        Assert.AreEqual(ReactionRunStatus.Failed, tick.Status);
        Assert.AreEqual(0F, process.Progress);
        Assert.AreEqual(0.00001F, context.Mixture.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0F, context.Mixture.GasAmountMol);
    }

    [Test]
    public void ReactionProcess_CancelAndResetRestoreSnapshot()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        process.Start();
        process.Tick(0.5F);
        process.Cancel();
        Assert.AreEqual(ReactionRunStatus.Cancelled, process.Tick(1F).Status);
        Assert.AreEqual(0.2F, process.Progress, 0.000001F);

        process.Reset();
        Assert.AreEqual(ReactionRunStatus.NotStarted, process.Status);
        Assert.AreEqual(0.0001F, context.Mixture.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0F, context.Mixture.GasAmountMol);
        Assert.IsTrue(process.Start().Succeeded);
        Assert.AreEqual(0.2F, process.Tick(0.5F).Progress, 0.000001F);
    }

    [Test]
    public void ReactionProcess_GasProductsCanFeedRefire()
    {
        ReactionContext context = MakeContext();
        ReactionProcess oxygen = new ReactionProcess(MakeOxygenDefinition(), context);
        oxygen.Start();
        oxygen.Tick(2.5F);
        Assert.AreEqual(0.00015F, context.Mixture.GetGasAmount("o2"), 0.0000001F);

        context.Mixture.AddReagent(new ReagentPortion("carbon", 0.0001F));
        context.HasHotEmber = true;
        context.HasOxygen = true;
        context.IsIgnited = true;
        ReactionProcess refire = new ReactionProcess(MakeRefireDefinition(), context);
        Assert.IsTrue(refire.Start().Succeeded);
        Assert.AreEqual(ReactionRunStatus.Completed, refire.Tick(1F).Status);
        Assert.AreEqual(0.00005F, context.Mixture.GetGasAmount("o2"), 0.0000001F);
        Assert.AreEqual(0.0001F, context.Mixture.GetGasAmount("co2"), 0.0000001F);
    }

    [Test]
    public void ReactionProcess_ResetWithoutRestoreKeepsCurrentMixture()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        process.Start();
        process.Tick(0.5F);
        process.Reset(false);
        Assert.AreEqual(ReactionRunStatus.NotStarted, process.Status);
        Assert.AreEqual(0.00008F, context.Mixture.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0.00003F, context.Mixture.GetGasAmount("o2"), 0.0000001F);
    }

    [Test]
    public void ReactionProcess_StartWhileRunningDoesNotResetProgress()
    {
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), MakeContext());
        process.Start();
        process.Tick(0.5F);
        ReactionResult secondStart = process.Start();
        Assert.IsFalse(secondStart.Succeeded);
        Assert.AreEqual(ReactionRunStatus.Running, process.Status);
        Assert.AreEqual(0.2F, process.Progress, 0.000001F);
    }

    [Test]
    public void ReactionProcess_TinyShortageRollsBackWithoutProduction()
    {
        ReactionContext context = MakeContext();
        ReactionProcess process = new ReactionProcess(MakeOxygenDefinition(), context);
        process.Start();
        context.Mixture.Consume("kclo3", 0.00006003F);
        float before = context.Mixture.GetAmount("kclo3");

        ReactionTickResult tick = process.Tick(1F);
        Assert.AreEqual(ReactionRunStatus.Failed, tick.Status);
        Assert.AreEqual(before, context.Mixture.GetAmount("kclo3"), 0F);
        Assert.AreEqual(0F, context.Mixture.GasAmountMol);
        Assert.AreEqual(0F, process.Progress);
        Assert.AreEqual(0, tick.Consumed.Count);
        Assert.AreEqual(0, tick.Produced.Count);
    }

    [Test]
    public void ReactionProcess_MultiProductOverflowRollsBackWholeTick()
    {
        ReactionDefinition definition = MakeOxygenDefinition();
        definition.ProgressPerSecond = 1F;
        definition.Products = new List<ReactionParticipant>
        {
            new ReactionParticipant("o2", 1e38F, ReactionParticipantRole.Product, SubstanceState.Gas),
            new ReactionParticipant("co2", 1e38F, ReactionParticipantRole.Product, SubstanceState.Gas)
        };
        ReactionContext context = MakeContext();
        context.Mixture.Consume("kclo3", context.Mixture.GetAmount("kclo3"));
        context.Mixture.AddReagent(new ReagentPortion("kclo3", 2F));
        context.Mixture.GasAmountMol = 2e38F;
        ReactionProcess process = new ReactionProcess(definition, context);
        Assert.IsTrue(process.Start().Succeeded);

        ReactionTickResult tick = process.Tick(1F);
        Assert.AreEqual(ReactionRunStatus.Failed, tick.Status);
        Assert.AreEqual(2F, context.Mixture.GetAmount("kclo3"), 0F);
        Assert.AreEqual(2e38F, context.Mixture.GasAmountMol, 0F);
        Assert.AreEqual(0, context.Mixture.GasComponents.Count);
        Assert.AreEqual(0, tick.Consumed.Count);
        Assert.AreEqual(0, tick.Produced.Count);
    }

    [Test]
    public void ReactionProcess_ProgressBoundariesAreExact()
    {
        ReactionDefinition underflowDefinition = MakeOxygenDefinition();
        underflowDefinition.ProgressPerSecond = float.Epsilon;
        ReactionProcess underflow = new ReactionProcess(underflowDefinition, MakeContext());
        underflow.Start();
        ReactionTickResult zeroDelta = underflow.Tick(float.Epsilon);
        Assert.AreEqual(ReactionRunStatus.Running, zeroDelta.Status);
        Assert.AreEqual(0F, zeroDelta.Progress);

        ReactionDefinition fractionalDefinition = MakeOxygenDefinition();
        fractionalDefinition.ProgressPerSecond = 0.333333F;
        ReactionProcess fractional = new ReactionProcess(fractionalDefinition, MakeContext());
        fractional.Start();
        fractional.Tick(1F);
        fractional.Tick(1F);
        ReactionTickResult third = fractional.Tick(1F);
        Assert.AreEqual(ReactionRunStatus.Running, third.Status);
        Assert.IsTrue(fractional.Progress < 1F);
        Assert.AreEqual(ReactionRunStatus.Completed, fractional.Tick(0.00001F).Status);
        Assert.AreEqual(1F, fractional.Progress);
    }

    [Test]
    public void MixtureState_CloneDoesNotShareReagentOrGasState()
    {
        MixtureState original = MakeContext().Mixture;
        original.AddProduct("o2", SubstanceState.Gas, 0.001F);
        original.AddProduct("kcl", SubstanceState.Solid, 0.002F);
        MixtureState clone = original.Clone();

        clone.Consume("kclo3", 0.00005F);
        clone.AddProduct("o2", SubstanceState.Gas, 0.001F);
        Assert.AreEqual(0.0001F, original.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0.00005F, clone.GetAmount("kclo3"), 0.0000001F);
        Assert.AreEqual(0.001F, original.GetGasAmount("o2"), 0.000001F);
        Assert.AreEqual(0.002F, clone.GetGasAmount("o2"), 0.000001F);
        Assert.AreEqual(0.002F, clone.GetSolidAmount("kcl"), 0.000001F);
    }

    [Test]
    public void ReactionDefinition_RejectsInvalidCoefficient()
    {
        ReactionDefinition definition = MakeOxygenDefinition();
        definition.Reactants[0].Coefficient = 0F;
        ReactionResult result = ReactionEngine.Evaluate(definition, MakeContext());
        Assert.IsFalse(result.Succeeded);
        StringAssert.Contains("coefficient", result.FailureReason);
    }
}
