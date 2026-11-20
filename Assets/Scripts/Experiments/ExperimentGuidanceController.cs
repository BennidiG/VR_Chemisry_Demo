using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class ExperimentGuidanceController : MonoBehaviour
{
    private static readonly Color PreparationColor = new Color(1F, 0.72F, 0.08F);
    private static readonly Color StepColor = new Color(0.08F, 0.82F, 1F);
    private static readonly Color CleanupColor = new Color(1F, 0.36F, 0.08F);

    private ExperimentRunner runner;
    private GameObject hints;
    private GameObject makeO2Steps;
    private GameObject matchReFireSteps;
    private TMP_Text titleText;
    private TMP_Text contentText;
    private readonly Dictionary<LabItem, Color> highlightedItems = new Dictionary<LabItem, Color>();
    private readonly Dictionary<string, TMP_Text> stepTextCache = new Dictionary<string, TMP_Text>();
    private readonly Dictionary<LabItemType, Color> highlightTargets = new Dictionary<LabItemType, Color>();
    private readonly List<LabItem> itemsToRemove = new List<LabItem>();
    private readonly List<string> confirmedItems = new List<string>();
    private readonly List<string> missingItems = new List<string>();
    private readonly List<string> pendingItems = new List<string>();
    private readonly StringBuilder contentBuilder = new StringBuilder();
    private float nextRefresh;
    private bool guidanceDirty = true;

    private void Start()
    {
        runner = ExperimentRunner.GetOrCreate();
        hints = SceneObjectFinder.Find("Hints");
        makeO2Steps = SceneObjectFinder.Find("Make_O2_Steps");
        matchReFireSteps = SceneObjectFinder.Find("Match_ReFire_Steps");
        titleText = FindText("Hints/Title");
        contentText = FindText("Hints/Content");

        CacheStepText("Make_O2_Step", 7);
        CacheStepText("Match_ReFire_Step", 3);

        if (contentText != null)
        {
            contentText.enableAutoSizing = true;
            contentText.fontSizeMin = 16F;
            contentText.fontSizeMax = 30F;
        }

        runner.ExperimentSelected += OnExperimentChanged;
        runner.ModeChanged += OnModeChanged;
        runner.PhaseChanged += OnPhaseChanged;
        runner.StepChanged += OnStepChanged;
        runner.CleanupChanged += OnCleanupChanged;
        runner.EquipmentPlacementChanged += OnEquipmentChanged;
        LocalizationManager.LanguageChanged += OnLanguageChanged;
        RefreshGuidance();
    }

    private void OnDestroy()
    {
        if (runner != null)
        {
            runner.ExperimentSelected -= OnExperimentChanged;
            runner.ModeChanged -= OnModeChanged;
            runner.PhaseChanged -= OnPhaseChanged;
            runner.StepChanged -= OnStepChanged;
            runner.CleanupChanged -= OnCleanupChanged;
            runner.EquipmentPlacementChanged -= OnEquipmentChanged;
        }
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
        ClearHighlights();
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextRefresh)
        {
            nextRefresh = Time.unscaledTime + 0.2F;
            if (runner.CurrentDefinition != null && !guidanceDirty)
                UpdateHighlights(runner.CurrentDefinition);
            if (guidanceDirty || runner.Phase == ExperimentPhase.Preparing)
                RefreshGuidance();
        }
    }

    private static TMP_Text FindText(string path)
    {
        GameObject textObject = SceneObjectFinder.Find(path);
        return textObject != null ? textObject.GetComponent<TMP_Text>() : null;
    }

    private void CacheStepText(string prefix, int count)
    {
        for (int index = 1; index <= count; index++)
        {
            TMP_Text text = FindText(prefix + index);
            if (text != null)
                stepTextCache[prefix + index] = text;
        }
    }

    private void OnExperimentChanged(ExperimentDefinition definition) { guidanceDirty = true; RefreshGuidance(); }
    private void OnModeChanged(ExperimentMode mode) { guidanceDirty = true; RefreshGuidance(); }
    private void OnPhaseChanged(ExperimentPhase phase) { guidanceDirty = true; RefreshGuidance(); }
    private void OnStepChanged(ExperimentStepDefinition step, bool completed) { guidanceDirty = true; RefreshGuidance(); }
    private void OnCleanupChanged(CleanupCriterion criterion, bool completed) { guidanceDirty = true; RefreshGuidance(); }
    private void OnEquipmentChanged(LabItemType itemType, bool onPosition) { guidanceDirty = true; RefreshGuidance(); }
    private void OnLanguageChanged(Language language) { guidanceDirty = true; RefreshGuidance(); }

    public void RefreshGuidance()
    {
        guidanceDirty = false;
        if (runner == null || hints == null)
            return;

        ExperimentDefinition definition = runner.CurrentDefinition;
        if (definition == null)
        {
            hints.SetActive(false);
            ClearHighlights();
            return;
        }

        hints.SetActive(true);
        UpdateChecklistVisibility(definition);
        UpdateStepChecklist(definition);
        UpdateHighlights(definition);

        if (runner.Phase != ExperimentPhase.Completed && runner.CurrentMode == ExperimentMode.FreePractice)
        {
            ShowFreePractice(definition);
            return;
        }
        if (runner.Phase != ExperimentPhase.Completed && runner.CurrentMode == ExperimentMode.Assessment)
        {
            ShowAssessment(definition);
            return;
        }

        switch (runner.Phase)
        {
            case ExperimentPhase.Preparing:
                ShowPreparation(definition);
                break;
            case ExperimentPhase.InProgress:
                ShowCurrentStep(definition);
                break;
            case ExperimentPhase.AwaitingCleanup:
                ShowCleanup(definition);
                break;
            case ExperimentPhase.Completed:
                ShowCompleted(definition);
                break;
        }
    }

    private void ShowFreePractice(ExperimentDefinition definition)
    {
        SetTitle(definition.GetName() + LocalizationManager.Get("｜自由练习", " | Free practice"));
        SetContent(definition.GetDescription() + "\n" + LocalizationManager.Get("可自由操作现有器材，不显示步骤高亮和指导；步骤与得分仍会在后台记录。", "Operate the apparatus freely. Highlights and instructions are hidden, while steps and score are still recorded."));
    }

    private void ShowAssessment(ExperimentDefinition definition)
    {
        SetTitle(definition.GetName() + LocalizationManager.Get("｜考核模式", " | Assessment"));
        SetContent(definition.GetDescription() + "\n" + LocalizationManager.Get("请独立完成实验并归位器材。步骤、高亮和实时得分已隐藏，完成后显示结果。", "Complete the experiment and cleanup independently. Steps, highlights and live score are hidden until completion."));
    }

    private void UpdateChecklistVisibility(ExperimentDefinition definition)
    {
        bool showChecklist = runner.CurrentMode == ExperimentMode.Guided && runner.Phase != ExperimentPhase.Preparing;
        if (makeO2Steps != null)
            makeO2Steps.SetActive(showChecklist && definition.projectNumber == 1);
        if (matchReFireSteps != null)
            matchReFireSteps.SetActive(showChecklist && definition.projectNumber == 2);
    }

    private void ShowPreparation(ExperimentDefinition definition)
    {
        SetTitle(definition.GetName() + LocalizationManager.Get("｜器材准备", " | Preparation"));
        List<string> confirmed = confirmedItems;
        confirmed.Clear();
        List<string> missing = missingItems;
        missing.Clear();
        foreach (LabItemType itemType in definition.requiredEquipment)
        {
            if (runner.IsEquipmentOnPosition(itemType))
                confirmed.Add(LocalizationManager.GetItemName(itemType));
            else
                missing.Add(LocalizationManager.GetItemName(itemType));
        }

        StringBuilder content = contentBuilder;
        content.Clear();
        content.AppendLine(definition.GetDescription());
        content.AppendLine(LocalizationManager.Get("将所需器材移动到左侧实验台。", "Move the required items to the left laboratory desk."));
        content.AppendLine(LocalizationManager.Get("已确认：", "Confirmed: ") + (confirmed.Count > 0 ? string.Join("、", confirmed) : LocalizationManager.Get("无", "None")));
        content.AppendLine(LocalizationManager.Get("还缺少：", "Missing: ") + (missing.Count > 0 ? string.Join("、", missing) : LocalizationManager.Get("无", "None")));
        if (missing.Count == 0)
            content.AppendLine(LocalizationManager.Get("器材已备齐，即将开始实验。", "All items are ready. The experiment will begin shortly."));
        else
            content.AppendLine(LocalizationManager.Get("请按黄色高亮找到缺少的器材。", "Find the missing items highlighted in yellow."));
        SetContent(content.ToString());
    }

    private void ShowCurrentStep(ExperimentDefinition definition)
    {
        ExperimentStepDefinition step = runner.CurrentStep;
        if (step == null)
            return;

        int completed = CountCompletedSteps(definition);
        SetTitle(definition.GetName() + LocalizationManager.Get("｜进行中 ", " | In progress ") + completed + "/" + definition.steps.Length);
        StringBuilder content = contentBuilder;
        content.Clear();
        content.AppendLine(step.GetTitle());
        content.AppendLine(step.GetInstruction());
        string safetyNote = step.GetSafetyNote();
        if (!string.IsNullOrWhiteSpace(safetyNote))
            content.AppendLine(LocalizationManager.Get("安全提示：", "Safety: ") + safetyNote);
        SetContent(content.ToString());
    }

    private void ShowCleanup(ExperimentDefinition definition)
    {
        SetTitle(definition.GetName() + LocalizationManager.Get("｜器材归位", " | Cleanup"));
        List<string> pending = pendingItems;
        pending.Clear();
        foreach (CleanupCriterion criterion in definition.cleanupCriteria)
        {
            if (!runner.IsCleanupCompleted(criterion.cleanupId))
                pending.Add(criterion.GetName());
        }

        StringBuilder content = contentBuilder;
        content.Clear();
        content.AppendLine(LocalizationManager.Get("主要步骤已完成，请将器材归位。", "The main procedure is complete. Return all items to their places."));
        content.AppendLine(LocalizationManager.Get("待归位：", "Remaining: ") + (pending.Count > 0 ? string.Join("、", pending) : LocalizationManager.Get("无", "None")));
        content.AppendLine(LocalizationManager.Get("橙色高亮表示仍需处理的器材。", "Items highlighted in orange still need attention."));
        SetContent(content.ToString());
    }

    private void ShowCompleted(ExperimentDefinition definition)
    {
        SetTitle(definition.GetName() + LocalizationManager.Get("｜已完成", " | Completed"));
        SetContent(runner.GetScoreText() + "\n" + LocalizationManager.Get("所有步骤和器材归位均已完成。点击“重置实验室”可重新开始。", "All steps and cleanup tasks are complete. Select Reset Lab to start again."));
    }

    private void UpdateStepChecklist(ExperimentDefinition definition)
    {
        string prefix = definition.projectNumber == 1 ? "Make_O2_Step" : "Match_ReFire_Step";
        for (int index = 0; index < definition.steps.Length; index++)
        {
            if (!stepTextCache.TryGetValue(prefix + (index + 1), out TMP_Text stepText) || stepText == null)
                continue;
            ExperimentStepDefinition step = definition.steps[index];
            stepText.text = step.GetTitle();
            stepText.color = runner.IsStepCompleted(step.stepId) ? Color.green : Color.black;
        }
    }

    private int CountCompletedSteps(ExperimentDefinition definition)
    {
        int count = 0;
        foreach (ExperimentStepDefinition step in definition.steps)
        {
            if (runner.IsStepCompleted(step.stepId))
                count++;
        }
        return count;
    }

    private void UpdateHighlights(ExperimentDefinition definition)
    {
        Dictionary<LabItemType, Color> targets = highlightTargets;
        targets.Clear();
        if (runner.CurrentMode == ExperimentMode.Guided && runner.Phase == ExperimentPhase.Preparing)
        {
            foreach (LabItemType itemType in definition.requiredEquipment)
            {
                if (!runner.IsEquipmentOnPosition(itemType))
                    targets[itemType] = PreparationColor;
            }
        }
        else if (runner.CurrentMode == ExperimentMode.Guided && runner.Phase == ExperimentPhase.InProgress && runner.CurrentStep != null)
        {
            foreach (LabItemType itemType in runner.CurrentStep.highlightItems)
                targets[itemType] = StepColor;
        }
        else if (runner.CurrentMode == ExperimentMode.Guided && runner.Phase == ExperimentPhase.AwaitingCleanup)
        {
            foreach (CleanupCriterion criterion in definition.cleanupCriteria)
            {
                if (!runner.IsCleanupCompleted(criterion.cleanupId) && criterion.itemType != LabItemType.Match)
                {
                    targets[criterion.itemType] = CleanupColor;
                    if (criterion.itemType == LabItemType.TestTubeRack)
                        targets[LabItemType.TestTube] = CleanupColor;
                }
            }
        }

        LabItem[] activeItems = LabItemRegistry.ActiveItems;
        foreach (LabItem item in activeItems)
        {
            if (targets.TryGetValue(item.itemType, out Color color))
                highlightedItems[item] = color;
        }

        List<LabItem> toRemove = itemsToRemove;
        toRemove.Clear();
        foreach (KeyValuePair<LabItem, Color> pair in highlightedItems)
        {
            if (pair.Key == null || !pair.Key.isActiveAndEnabled || !targets.ContainsKey(pair.Key.itemType))
                toRemove.Add(pair.Key);
        }
        foreach (LabItem item in toRemove)
        {
            if (item != null)
                item.SetHighlight(null);
            highlightedItems.Remove(item);
        }

        foreach (KeyValuePair<LabItem, Color> pair in highlightedItems)
        {
            if (pair.Key != null)
                pair.Key.SetHighlight(pair.Value);
        }
    }

    private void ClearHighlights()
    {
        foreach (KeyValuePair<LabItem, Color> pair in highlightedItems)
        {
            if (pair.Key != null)
                pair.Key.SetHighlight(null);
        }
        highlightedItems.Clear();
    }

    private void SetTitle(string text)
    {
        if (titleText != null)
            titleText.text = text;
    }

    private void SetContent(string text)
    {
        if (contentText == null)
            return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        text += LocalizationManager.Get("\n调试：F1制氧 F2复燃 F3备齐 F4步骤 F5归位 M模式 L语言 R重置", "\nDebug: F1 O2  F2 ReFire  F3 Prepare  F4 Step  F5 Cleanup  M Mode  L Language  R Reset");
#endif
        contentText.text = text;
    }
}
