using System;
using System.Collections.Generic;
using UnityEngine;

public enum LabItemType
{
    None = 0,
    Match = 1,
    Spoon = 2,
    DrugBottle = 3,
    DrugBottleCap = 4,
    TestTube = 5,
    TubeClamp = 6,
    Basin = 7,
    Catheter = 8,
    AlcoholLamp = 9,
    AlcoholLampCap = 10,
    GlassBottle = 11,
    GlassBottleCap = 12,
    Cork = 13,
    TestTubeRack = 14,
    Desk = 15
}

[Serializable]
public class ExperimentStepDefinition
{
    public string stepId;
    public string chineseTitle;
    public string englishTitle;
    [TextArea] public string chineseInstruction;
    [TextArea] public string englishInstruction;
    [TextArea] public string chineseSafetyNote;
    [TextArea] public string englishSafetyNote;
    [Min(0F)] public float scoreWeight = 5F;
    public LabItemType[] highlightItems = new LabItemType[0];

    public string GetTitle()
    {
        return LocalizationManager.Get(chineseTitle, englishTitle);
    }

    public string GetInstruction()
    {
        return LocalizationManager.Get(chineseInstruction, englishInstruction);
    }

    public string GetSafetyNote()
    {
        return LocalizationManager.Get(chineseSafetyNote, englishSafetyNote);
    }
}

[Serializable]
public class CleanupCriterion
{
    public string cleanupId;
    public LabItemType itemType;
    public string chineseName;
    public string englishName;
    [Min(0F)] public float scoreWeight = 1F;

    public string GetName()
    {
        return LocalizationManager.Get(chineseName, englishName);
    }
}

[CreateAssetMenu(fileName = "ExperimentDefinition", menuName = "VR Chemistry/Experiment Definition")]
public class ExperimentDefinition : ScriptableObject
{
    [Min(1)] public int projectNumber = 1;
    public string experimentId;
    public string chineseName;
    public string englishName;
    [TextArea] public string chineseDescription;
    [TextArea] public string englishDescription;
    public LabItemType[] requiredEquipment = new LabItemType[0];
    public ExperimentStepDefinition[] steps = new ExperimentStepDefinition[0];
    public CleanupCriterion[] cleanupCriteria = new CleanupCriterion[0];

    public string GetName()
    {
        return LocalizationManager.Get(chineseName, englishName);
    }

    public string GetDescription()
    {
        return LocalizationManager.Get(chineseDescription, englishDescription);
    }

    public ExperimentStepDefinition GetStep(string stepId)
    {
        foreach (ExperimentStepDefinition step in steps)
        {
            if (step.stepId == stepId)
                return step;
        }
        return null;
    }

    public CleanupCriterion GetCleanup(string cleanupId)
    {
        foreach (CleanupCriterion criterion in cleanupCriteria)
        {
            if (criterion.cleanupId == cleanupId)
                return criterion;
        }
        return null;
    }

    public float GetTotalWeight()
    {
        float total = 0F;
        foreach (ExperimentStepDefinition step in steps)
            total += step.scoreWeight;
        foreach (CleanupCriterion criterion in cleanupCriteria)
            total += criterion.scoreWeight;
        return total;
    }

    public List<string> ValidateConfiguration()
    {
        List<string> errors = new List<string>();
        if (string.IsNullOrWhiteSpace(experimentId))
            errors.Add("experimentId is empty");
        if (steps.Length == 0)
            errors.Add("No steps configured");
        if (GetTotalWeight() <= 0F)
            errors.Add("Total score weight must be positive");

        HashSet<string> stepIds = new HashSet<string>();
        foreach (ExperimentStepDefinition step in steps)
        {
            if (string.IsNullOrWhiteSpace(step.stepId))
                errors.Add("A step has an empty stepId");
            else if (!stepIds.Add(step.stepId))
                errors.Add("Duplicate stepId: " + step.stepId);
            if (string.IsNullOrWhiteSpace(step.chineseTitle) || string.IsNullOrWhiteSpace(step.englishTitle))
                errors.Add("Missing localized title: " + step.stepId);
        }

        HashSet<string> cleanupIds = new HashSet<string>();
        foreach (CleanupCriterion criterion in cleanupCriteria)
        {
            if (string.IsNullOrWhiteSpace(criterion.cleanupId))
                errors.Add("A cleanup criterion has an empty cleanupId");
            else if (!cleanupIds.Add(criterion.cleanupId))
                errors.Add("Duplicate cleanupId: " + criterion.cleanupId);
        }
        return errors;
    }
}
