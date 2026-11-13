using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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
}
