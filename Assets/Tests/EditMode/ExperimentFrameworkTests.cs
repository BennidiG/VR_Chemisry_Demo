using System;
using NUnit.Framework;
using UnityEngine;

public class ExperimentFrameworkTests
{
    private ExperimentRunner runner;
    private bool ownsRunner;
    private Language previousLanguage;

    [SetUp]
    public void SetUp()
    {
        previousLanguage = LocalizationManager.CurrentLanguage;
        if (ExperimentRunner.Instance == null)
        {
            runner = new GameObject("ExperimentRunnerTests").AddComponent<ExperimentRunner>();
            ownsRunner = true;
        }
        else
        {
            runner = ExperimentRunner.Instance;
            ownsRunner = false;
        }
        runner.SetMode(ExperimentMode.Guided);
    }

    [TearDown]
    public void TearDown()
    {
        LocalizationManager.SetLanguage(previousLanguage);
        if (ownsRunner && runner != null)
            Object.DestroyImmediate(runner.gameObject);
    }

    [Test]
    public void Definitions_AreCompleteAndUseExpectedWeights()
    {
        ExperimentDefinition makeO2 = runner.GetDefinition(1);
        ExperimentDefinition matchReFire = runner.GetDefinition(2);

        Assert.NotNull(makeO2);
        Assert.NotNull(matchReFire);
        CollectionAssert.IsEmpty(makeO2.ValidateConfiguration());
        CollectionAssert.IsEmpty(matchReFire.ValidateConfiguration());
        Assert.AreEqual(7, makeO2.steps.Length);
        Assert.AreEqual(3, matchReFire.steps.Length);
        Assert.AreEqual(48F, makeO2.GetTotalWeight(), 0.001F);
        Assert.AreEqual(20F, matchReFire.GetTotalWeight(), 0.001F);
    }

    [Test]
    public void Preparation_UsesConfiguredEquipment()
    {
        runner.SelectExperiment(2);
        Assert.AreEqual(ExperimentPhase.Preparing, runner.Phase);
        Assert.IsFalse(runner.AreRequiredEquipmentReady());

        runner.SetEquipmentOnPosition(LabItemType.Match, true);
        runner.SetEquipmentOnPosition(LabItemType.GlassBottle, true);
        Assert.IsFalse(runner.AreRequiredEquipmentReady());
        runner.SetEquipmentOnPosition(LabItemType.AlcoholLamp, true);
        Assert.IsTrue(runner.AreRequiredEquipmentReady());

        runner.MarkPrepared();
        Assert.AreEqual(ExperimentPhase.InProgress, runner.Phase);
    }

    [Test]
    public void ScoreAndPhase_FollowStepsAndCleanup()
    {
        runner.SelectExperiment(1);
        PrepareCurrentExperiment();
        foreach (ExperimentStepDefinition step in runner.CurrentDefinition.steps)
            runner.SetStepCompleted(step.stepId, true);

        Assert.IsTrue(runner.AreAllStepsCompleted());
        Assert.AreEqual(ExperimentPhase.AwaitingCleanup, runner.Phase);
        Assert.AreEqual(35F / 48F * 100F, runner.GetScore(), 0.001F);

        foreach (CleanupCriterion criterion in runner.CurrentDefinition.cleanupCriteria)
            runner.SetCleanupCompleted(criterion.cleanupId, true);

        Assert.AreEqual(ExperimentPhase.Completed, runner.Phase);
        Assert.AreEqual(100F, runner.GetScore(), 0.001F);
    }

    [Test]
    public void MatchReFire_MaximumScoreIsOneHundred()
    {
        runner.SelectExperiment(2);
        PrepareCurrentExperiment();
        foreach (ExperimentStepDefinition step in runner.CurrentDefinition.steps)
            runner.SetStepCompleted(step.stepId, true);
        Assert.AreEqual(75F, runner.GetScore(), 0.001F);

        foreach (CleanupCriterion criterion in runner.CurrentDefinition.cleanupCriteria)
            runner.SetCleanupCompleted(criterion.cleanupId, true);
        Assert.AreEqual(100F, runner.GetScore(), 0.001F);
    }

    [Test]
    public void Steps_RequirePreparationAndSequentialOrder()
    {
        runner.SelectExperiment(1);
        runner.SetStepCompleted("take_reagent", true);
        Assert.IsFalse(runner.IsStepCompleted("take_reagent"));

        PrepareCurrentExperiment();
        runner.SetStepCompleted("attach_cork", true);
        Assert.IsFalse(runner.IsStepCompleted("attach_cork"));
        runner.SetStepCompleted("take_reagent", true);
        Assert.IsTrue(runner.IsStepCompleted("take_reagent"));
    }

    [Test]
    public void Reset_ClearsExperimentProgress()
    {
        runner.SelectExperiment(1);
        PrepareCurrentExperiment();
        runner.SetStepCompleted("take_reagent", true);
        bool resetRaised = false;
        runner.ExperimentReset += () => resetRaised = true;
        runner.ResetCurrentExperiment();

        Assert.IsTrue(resetRaised);
        Assert.AreEqual(ExperimentPhase.Preparing, runner.Phase);
        Assert.AreEqual("take_reagent", runner.CurrentStep.stepId);
        Assert.AreEqual(0F, runner.GetScore());
        Assert.IsFalse(runner.AreRequiredEquipmentReady());
    }

    [Test]
    public void Localization_SwitchesStepAndScoreText()
    {
        runner.SelectExperiment(1);
        ExperimentStepDefinition firstStep = runner.CurrentDefinition.steps[0];

        LocalizationManager.SetLanguage(Language.English);
        StringAssert.Contains("Step 1", firstStep.GetTitle());
        StringAssert.Contains("score", runner.GetScoreText());

        LocalizationManager.SetLanguage(Language.Chinese);
        StringAssert.Contains("步骤一", firstStep.GetTitle());
        StringAssert.Contains("得分", runner.GetScoreText());
    }

    [Test]
    public void Assessment_HidesLiveScoreUntilCompletion()
    {
        runner.SelectExperiment(2);
        runner.SetMode(ExperimentMode.Assessment);
        PrepareCurrentExperiment();
        foreach (ExperimentStepDefinition step in runner.CurrentDefinition.steps)
            runner.SetStepCompleted(step.stepId, true);

        Assert.IsFalse(runner.GetScoreText().Contains("75"));
        foreach (CleanupCriterion criterion in runner.CurrentDefinition.cleanupCriteria)
            runner.SetCleanupCompleted(criterion.cleanupId, true);
        Assert.IsTrue(runner.GetScoreText().Contains("100"));
    }

    [Test]
    public void LabItem_ReturnHome_RestoresHomeState()
    {
        GameObject itemObject = new GameObject("CorkHomeTest");
        try
        {
            LabItem item = itemObject.AddComponent<LabItem>();
            item.itemType = LabItemType.Cork;
            item.trackHomePosition = true;
            item.homeRadius = 0.2F;

            item.transform.position = new Vector3(2F, 0F, 0F);
            Assert.IsFalse(item.IsAtHome);
            item.DebugReturnHome();
            Assert.IsTrue(item.IsAtHome);
        }
        finally
        {
            Object.DestroyImmediate(itemObject);
        }
    }

    [Test]
    public void ScoreChanged_FiresOnlyOnStateChanges()
    {
        int notifications = 0;
        Action handler = () => notifications++;
        runner.ScoreChanged += handler;
        try
        {
            runner.SelectExperiment(1);
            PrepareCurrentExperiment();
            int before = notifications;
            runner.SetStepCompleted("take_reagent", true);
            Assert.Greater(notifications, before);
        }
        finally
        {
            runner.ScoreChanged -= handler;
        }
    }

    [Test]
    public void LabItemRegistry_Cache_ReusesUntilMembershipChanges()
    {
        GameObject itemObject = new GameObject("RegistryCache");
        LabItem item = itemObject.AddComponent<LabItem>();
        try
        {
            LabItemRegistry.Register(item);
            LabItem[] first = LabItemRegistry.ActiveItems;
            LabItem[] second = LabItemRegistry.ActiveItems;
            Assert.AreSame(first, second);

            LabItemRegistry.Unregister(item);
            Assert.AreNotSame(first, LabItemRegistry.ActiveItems);
        }
        finally
        {
            LabItemRegistry.Unregister(item);
            Object.DestroyImmediate(itemObject);
        }
    }

    private void PrepareCurrentExperiment()
    {
        foreach (LabItemType itemType in runner.CurrentDefinition.requiredEquipment)
            runner.SetEquipmentOnPosition(itemType, true);
        runner.MarkPrepared();
    }
}
