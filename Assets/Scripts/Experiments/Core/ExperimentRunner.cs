using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ExperimentPhase
{
    None = 0,
    Preparing = 1,
    InProgress = 2,
    AwaitingCleanup = 3,
    Completed = 4
}

public enum ExperimentMode
{
    Guided = 0,
    FreePractice = 1,
    Assessment = 2
}

[DefaultExecutionOrder(-1000)]
public class ExperimentRunner : MonoBehaviour
{
    public static ExperimentRunner Instance { get; private set; }
    public static ILaboratoryResettable LaboratoryResetHandler { get; set; }

    [SerializeField] private ExperimentDefinition makeO2Definition;
    [SerializeField] private ExperimentDefinition matchReFireDefinition;

    private readonly HashSet<string> completedSteps = new HashSet<string>();
    private readonly HashSet<string> completedCleanup = new HashSet<string>();
    private readonly HashSet<LabItemType> equipmentOnPosition = new HashSet<LabItemType>();
    private bool prepared;
    private bool resetOxygenNextSelection;

    public ExperimentDefinition CurrentDefinition { get; private set; }
    public ExperimentPhase Phase { get; private set; } = ExperimentPhase.None;
    public bool ShouldResetOxygen { get; private set; }
    public ExperimentMode CurrentMode { get; private set; } = ExperimentMode.Guided;
    public int CurrentProject => CurrentDefinition != null ? CurrentDefinition.projectNumber : 0;

    public event Action<ExperimentDefinition> ExperimentSelected;
    public event Action<ExperimentMode> ModeChanged;
    public event Action<ExperimentPhase> PhaseChanged;
    public event Action<ExperimentStepDefinition, bool> StepChanged;
    public event Action<CleanupCriterion, bool> CleanupChanged;
    public event Action<LabItemType, bool> EquipmentPlacementChanged;
    public event Action ExperimentReset;

    public ExperimentStepDefinition CurrentStep
    {
        get
        {
            if (CurrentDefinition == null)
                return null;
            foreach (ExperimentStepDefinition step in CurrentDefinition.steps)
            {
                if (!completedSteps.Contains(step.stepId))
                    return step;
            }
            return null;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (makeO2Definition == null)
            makeO2Definition = Resources.Load<ExperimentDefinition>("Experiments/MakeO2");
        if (matchReFireDefinition == null)
            matchReFireDefinition = Resources.Load<ExperimentDefinition>("Experiments/MatchReFire");
        ValidateDefinitions();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static ExperimentRunner GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        Instance = FindObjectOfType<ExperimentRunner>();
        if (Instance == null)
        {
            GameObject runnerObject = new GameObject("ExperimentRunner");
            Instance = runnerObject.AddComponent<ExperimentRunner>();
        }
        return Instance;
    }

    public ExperimentDefinition GetDefinition(int projectNumber)
    {
        if (projectNumber == 1)
            return makeO2Definition;
        if (projectNumber == 2)
            return matchReFireDefinition;
        return null;
    }

    public void SelectExperiment(int projectNumber)
    {
        ExperimentDefinition definition = GetDefinition(projectNumber);
        if (definition == null)
        {
            Debug.LogError("Missing experiment definition for project " + projectNumber);
            return;
        }

        ShouldResetOxygen = resetOxygenNextSelection || CurrentProject != projectNumber;
        CurrentDefinition = definition;
        completedSteps.Clear();
        completedCleanup.Clear();
        equipmentOnPosition.Clear();
        prepared = false;
        Phase = ExperimentPhase.Preparing;

        ExperimentReset?.Invoke();
        ExperimentSelected?.Invoke(definition);
        PhaseChanged?.Invoke(Phase);

        if (ShouldResetOxygen && CurrentProject == 2 && OxygenSupply.Instance != null)
            OxygenSupply.Instance.InitializeForExperiment();
    }

    public void SetEquipmentOnPosition(LabItemType itemType, bool onPosition)
    {
        bool changed = onPosition ? equipmentOnPosition.Add(itemType) : equipmentOnPosition.Remove(itemType);
        if (changed)
            EquipmentPlacementChanged?.Invoke(itemType, onPosition);
    }

    public void SetMode(ExperimentMode mode)
    {
        if (CurrentMode == mode)
            return;
        CurrentMode = mode;
        ModeChanged?.Invoke(mode);
    }

    public void CycleMode()
    {
        int next = ((int)CurrentMode + 1) % 3;
        SetMode((ExperimentMode)next);
    }

    public bool IsEquipmentOnPosition(LabItemType itemType)
    {
        return equipmentOnPosition.Contains(itemType);
    }

    public bool AreRequiredEquipmentReady()
    {
        if (CurrentDefinition == null || CurrentDefinition.requiredEquipment.Length == 0)
            return false;
        foreach (LabItemType itemType in CurrentDefinition.requiredEquipment)
        {
            if (!equipmentOnPosition.Contains(itemType))
                return false;
        }
        return true;
    }

    public void MarkPrepared()
    {
        if (CurrentDefinition == null || !AreRequiredEquipmentReady())
            return;
        prepared = true;
        RecalculatePhase();
    }

    public void SetStepCompleted(string stepId, bool completed)
    {
        if (!prepared || (completed && CurrentStep != null && CurrentStep.stepId != stepId))
        {
            Debug.LogWarning("Step '" + stepId + "' was rejected because the experiment is not ready or steps are out of order.");
            return;
        }
        SetStepCompletedFromLegacy(stepId, completed);
    }

    // Explicit bridge for the original collision-driven flow, which already
    // contains its own step guards. New experiment logic should use SetStepCompleted.
    public void SetStepCompletedFromLegacy(string stepId, bool completed)
    {
        if (CurrentDefinition == null)
            return;
        ExperimentStepDefinition step = CurrentDefinition.GetStep(stepId);
        if (step == null)
        {
            Debug.LogWarning("Unknown step '" + stepId + "' in " + CurrentDefinition.experimentId);
            return;
        }

        bool changed = completed ? completedSteps.Add(stepId) : completedSteps.Remove(stepId);
        if (changed)
        {
            StepChanged?.Invoke(step, completed);
            RecalculatePhase();
        }
    }

    public bool IsStepCompleted(string stepId)
    {
        return completedSteps.Contains(stepId);
    }

    public bool AreAllStepsCompleted()
    {
        if (CurrentDefinition == null || CurrentDefinition.steps.Length == 0)
            return false;
        foreach (ExperimentStepDefinition step in CurrentDefinition.steps)
        {
            if (!completedSteps.Contains(step.stepId))
                return false;
        }
        return true;
    }

    public void SetCleanupCompleted(string cleanupId, bool completed)
    {
        if (!prepared)
        {
            Debug.LogWarning("Cleanup '" + cleanupId + "' was rejected before preparation completed.");
            return;
        }
        SetCleanupCompletedFromLegacy(cleanupId, completed);
    }

    public void SetCleanupCompletedFromLegacy(string cleanupId, bool completed)
    {
        if (CurrentDefinition == null)
            return;
        CleanupCriterion criterion = CurrentDefinition.GetCleanup(cleanupId);
        if (criterion == null)
        {
            Debug.LogWarning("Unknown cleanup criterion '" + cleanupId + "' in " + CurrentDefinition.experimentId);
            return;
        }

        bool changed = completed ? completedCleanup.Add(cleanupId) : completedCleanup.Remove(cleanupId);
        if (changed)
        {
            CleanupChanged?.Invoke(criterion, completed);
            RecalculatePhase();
        }
    }

    public bool IsCleanupCompleted(string cleanupId)
    {
        return completedCleanup.Contains(cleanupId);
    }

    public bool AreAllCleanupCompleted()
    {
        if (CurrentDefinition == null)
            return false;
        foreach (CleanupCriterion criterion in CurrentDefinition.cleanupCriteria)
        {
            if (!completedCleanup.Contains(criterion.cleanupId))
                return false;
        }
        return true;
    }

    public float GetScore()
    {
        if (CurrentDefinition == null)
            return 0F;

        float earned = 0F;
        foreach (ExperimentStepDefinition step in CurrentDefinition.steps)
        {
            if (completedSteps.Contains(step.stepId))
                earned += step.scoreWeight;
        }

        if (AreAllStepsCompleted())
        {
            foreach (CleanupCriterion criterion in CurrentDefinition.cleanupCriteria)
            {
                if (completedCleanup.Contains(criterion.cleanupId))
                    earned += criterion.scoreWeight;
            }
        }

        float total = CurrentDefinition.GetTotalWeight();
        return total > 0F ? Mathf.Clamp(earned / total * 100F, 0F, 100F) : 0F;
    }

    public string GetScoreText()
    {
        if (CurrentDefinition == null)
            return string.Empty;
        if (CurrentMode == ExperimentMode.Assessment && Phase != ExperimentPhase.Completed)
            return CurrentDefinition.GetName() + LocalizationManager.Get("｜考核中", " | Assessment in progress");
        return CurrentDefinition.GetName() + LocalizationManager.Get("得分", " score") + ": " + GetScore().ToString("0.#") + " /100";
    }

    public void ResetCurrentExperiment()
    {
        resetOxygenNextSelection = true;
        LaboratoryResetHandler?.ResetLaboratory();
        if (CurrentProject != 0)
            SelectExperiment(CurrentProject);
        else
        {
            ShouldResetOxygen = true;
            ExperimentReset?.Invoke();
        }
        resetOxygenNextSelection = false;
    }

    public void ResetLaboratory()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void RecalculatePhase()
    {
        ExperimentPhase newPhase;
        if (CurrentDefinition == null)
            newPhase = ExperimentPhase.None;
        else if (!prepared)
            newPhase = ExperimentPhase.Preparing;
        else if (!AreAllStepsCompleted())
            newPhase = ExperimentPhase.InProgress;
        else if (!AreAllCleanupCompleted())
            newPhase = ExperimentPhase.AwaitingCleanup;
        else
            newPhase = ExperimentPhase.Completed;

        if (Phase == newPhase)
            return;
        Phase = newPhase;
        PhaseChanged?.Invoke(Phase);
    }

    private void ValidateDefinitions()
    {
        ValidateDefinition(makeO2Definition);
        ValidateDefinition(matchReFireDefinition);
    }

    private static void ValidateDefinition(ExperimentDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogError("An ExperimentDefinition asset is missing from Resources/Experiments");
            return;
        }
        foreach (string error in definition.ValidateConfiguration())
            Debug.LogError(definition.name + ": " + error);
    }
}
