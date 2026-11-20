using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;

public class ExperimentSceneSmokeTests
{
    private static IEnumerator LoadMainScene()
    {
        // Always reload so oxygen and legacy state from a previous test cannot leak.
        yield return SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Single);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MainScene_HasCoreReferences()
    {
        yield return LoadMainScene();
        Assert.NotNull(ExperimentRunner.Instance);
        Assert.NotNull(ExperimentRunner.LaboratoryResetHandler);
        Assert.NotNull(OxygenSupply.Instance);
        Assert.NotNull(ExperimentRunner.Instance.GetDefinition(1));
        Assert.NotNull(ExperimentRunner.Instance.GetDefinition(2));
        Assert.GreaterOrEqual(Object.FindObjectsOfType<LabItem>().Length, 14);
        Assert.IsFalse(OxygenSupply.Instance.Initialized);
    }

    [UnityTest]
    public IEnumerator OxygenSupply_InitializesOnlyOnce()
    {
        yield return LoadMainScene();
        OxygenSupply supply = OxygenSupply.Instance;
        supply.ResetSupply();

        Assert.IsTrue(supply.InitializeForExperiment());
        float initialAmount = supply.Amount;
        Assert.IsFalse(supply.InitializeForExperiment());
        Assert.AreEqual(initialAmount, supply.Amount);

        supply.ConsumeOxygen(100F);
        Assert.IsFalse(supply.InitializeForExperiment());
        Assert.AreEqual(initialAmount - 100F, supply.Amount, 0.001F);
    }

    [UnityTest]
    public IEnumerator ResetMakeO2_ClearsDirtyState()
    {
        yield return LoadMainScene();
        ExperimentRunner runner = ExperimentRunner.Instance;
        runner.SelectExperiment(1);
        foreach (LabItemType itemType in runner.CurrentDefinition.requiredEquipment)
            runner.SetEquipmentOnPosition(itemType, true);
        runner.MarkPrepared();
        runner.SetStepCompleted("take_reagent", true);
        runner.SetCleanupCompleted("spoon", true);
        Assert.Greater(runner.GetScore(), 0F);

        runner.ResetCurrentExperiment();
        yield return null;

        Assert.NotNull(ExperimentRunner.LaboratoryResetHandler);
        Assert.AreEqual(ExperimentPhase.Preparing, runner.Phase);
        Assert.AreEqual(0F, runner.GetScore());
        Assert.AreEqual("take_reagent", runner.CurrentStep.stepId);
        Assert.IsFalse(runner.AreRequiredEquipmentReady());
        Assert.AreEqual(1, Object.FindObjectsOfType<ExperimentRunner>().Length);
        Assert.AreEqual(1, Object.FindObjectsOfType<OxygenSupply>().Length);
    }

    [UnityTest]
    public IEnumerator ResetMatchReFire_RestoresPreparationAndInitialOxygen()
    {
        yield return LoadMainScene();
        ExperimentRunner runner = ExperimentRunner.Instance;
        runner.SelectExperiment(2);
        foreach (LabItemType itemType in runner.CurrentDefinition.requiredEquipment)
            runner.SetEquipmentOnPosition(itemType, true);
        runner.MarkPrepared();
        runner.SetStepCompleted("ignite_match_and_lamp", true);
        OxygenSupply.Instance.InitializeForExperiment();
        OxygenSupply.Instance.ConsumeOxygen(500F);
        Assert.Greater(runner.GetScore(), 0F);

        runner.ResetCurrentExperiment();
        yield return null;

        Assert.AreEqual(ExperimentPhase.Preparing, runner.Phase);
        Assert.AreEqual("ignite_match_and_lamp", runner.CurrentStep.stepId);
        Assert.AreEqual(0F, runner.GetScore());
        Assert.AreEqual(OxygenSupply.MaxOxygen, OxygenSupply.Instance.Amount, 0.001F);
        Assert.IsTrue(OxygenSupply.Instance.Initialized);
        Assert.AreEqual(1, Object.FindObjectsOfType<ExperimentRunner>().Length);
        Assert.AreEqual(1, Object.FindObjectsOfType<OxygenSupply>().Length);
    }

    [UnityTest]
    public IEnumerator SameProjectSelection_StartsNewOxygenRun()
    {
        yield return LoadMainScene();
        ExperimentRunner runner = ExperimentRunner.Instance;
        runner.SelectExperiment(2);
        OxygenSupply supply = OxygenSupply.Instance;
        supply.ConsumeOxygen(supply.Amount);
        Assert.AreEqual(0F, supply.Amount, 0.001F);
        int previousRun = runner.ExperimentRunId;

        runner.SelectExperiment(2);
        yield return null;

        Assert.Greater(runner.ExperimentRunId, previousRun);
        Assert.AreEqual(OxygenSupply.MaxOxygen, supply.Amount, 0.001F);
        Assert.IsTrue(supply.Initialized);
    }

    [UnityTest]
    public IEnumerator Trash_DisabledLabItemClearsCleanup()
    {
        yield return LoadMainScene();
        ExperimentRunner runner = ExperimentRunner.Instance;
        Score score = Object.FindObjectOfType<Score>();
        score.Choose_Project1();
        score.Yes_Spoon_On_Position();
        score.SyncRunnerState();
        Assert.IsTrue(runner.IsCleanupCompleted("spoon"));

        GameObject trashObject = new GameObject("TrashTest");
        TrashTrigger trash = trashObject.AddComponent<TrashTrigger>();
        yield return null;

        GameObject item = new GameObject("Spoon", typeof(Rigidbody), typeof(BoxCollider), typeof(LabItem));
        item.GetComponent<LabItem>().itemType = LabItemType.Spoon;
        trash.OnTriggerEnter(item.GetComponent<Collider>());

        Assert.IsFalse(item.activeSelf);
        score.SyncRunnerState();
        Assert.IsFalse(runner.IsCleanupCompleted("spoon"));

        Object.Destroy(trashObject);
        Object.Destroy(item);
    }

    [UnityTest]
    public IEnumerator LegacyAdapter_OnlyWriterUpdatesRunner()
    {
        yield return LoadMainScene();
        ExperimentRunner runner = ExperimentRunner.Instance;
        Score score = Object.FindObjectOfType<Score>();
        score.Choose_Project1();
        score.Yes_Spoon_On_Position();
        score.SyncRunnerState();
        Assert.IsTrue(runner.IsCleanupCompleted("spoon"));

        score.No_Spoon_On_Position();
        score.SyncRunnerState();
        Assert.IsFalse(runner.IsCleanupCompleted("spoon"));
    }

    [UnityTest]
    public IEnumerator MatchReset_RestoresSocketSelection()
    {
        yield return LoadMainScene();
        Score score = Object.FindObjectOfType<Score>();
        score.Choose_Project2();
        Xr_Ray preparation = Object.FindObjectOfType<Xr_Ray>();
        preparation.Make_O2_ReFire();
        preparation.Yes_Prepare_Match_On_Position();
        preparation.Yes_Prepare_GlassBottle_On_Position();
        preparation.Yes_Prepare_AlcoholLamp_On_Position();
        preparation.Project2_Ready();
        Assert.AreEqual(ExperimentPhase.InProgress, ExperimentRunner.Instance.Phase);
        yield return null;

        CreateMatch creator = Object.FindObjectOfType<CreateMatch>();
        GameObject oldMatch = GameObject.Find("Match");
        Object.Destroy(oldMatch);
        creator.Clear_Old_Match();
        yield return null;
        creator.ResetMatch();
        yield return null;
        yield return null;

        XRSocketInteractor socket = SceneObjectFinder.Find("MatchSocket").GetComponent<XRSocketInteractor>();
        Assert.NotNull(GameObject.Find("Match"));
        Assert.AreEqual(1, Object.FindObjectsOfType<VisualEffectGraph>().Length);
        Assert.IsTrue(socket.hasSelection);
    }

    [UnityTest]
    public IEnumerator Chemistry_MakeOxygen_UsesCoreAndConservesMass()
    {
        yield return LoadMainScene();
        ReactionResult result = ChemistryLegacyAdapter.EvaluateMakeOxygen(true, true, true);
        Assert.IsTrue(result.Succeeded, result.FailureReason);
        Assert.AreEqual(0.00015F, result.GasProduced["o2"], 0.0000001F);
        Assert.AreEqual(result.InputMass, result.OutputMass, 0.000001F);
    }

    [UnityTest]
    public IEnumerator Chemistry_Refire_RequiresEmberAndConservesMass()
    {
        yield return LoadMainScene();
        ReactionResult noEmber = ChemistryLegacyAdapter.EvaluateMatchRefire(false, 10000F);
        Assert.IsFalse(noEmber.Succeeded);

        ReactionResult result = ChemistryLegacyAdapter.EvaluateMatchRefire(true, 10000F);
        Assert.IsTrue(result.Succeeded, result.FailureReason);
        Assert.AreEqual(0.0001F, result.GasProduced["co2"], 0.0000001F);
        Assert.AreEqual(result.InputMass, result.OutputMass, 0.000001F);
    }

    [UnityTest]
    public IEnumerator Chemistry_StartProcess_UsesAssetsAndCommits()
    {
        yield return LoadMainScene();
        ReactionContext context = new ReactionContext { IsHeating = true, GasCollectionConnected = true };
        context.Mixture.SetTemperature(250F);
        context.Mixture.AddReagent(new ReagentPortion("kclo3", 0.0001F));
        context.Catalysts.Add("mno2");

        ReactionProcess process = ChemistrySimulationService.GetOrCreate().StartProcess("make_oxygen", context, out ReactionResult initial);
        Assert.NotNull(process);
        Assert.IsTrue(initial.Succeeded, initial.FailureReason);
        ReactionTickResult tick = process.Tick(2.5F);
        Assert.AreEqual(ReactionRunStatus.Completed, tick.Status);
        Assert.AreEqual(0.00015F, context.Mixture.GetGasAmount("o2"), 0.0000001F);
        Assert.AreEqual(initial.InputMass, initial.OutputMass, 0.000001F);
    }
}
