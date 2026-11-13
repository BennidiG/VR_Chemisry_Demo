using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperimentLocalizationController : MonoBehaviour
{
    private ExperimentRunner runner;
    private TMP_Text languageToggleLabel;
    private Text languageToggleLegacyLabel;
    private TMP_Text modeToggleLabel;
    private Text modeToggleLegacyLabel;

    private void Start()
    {
        runner = ExperimentRunner.GetOrCreate();
        CreateToggle("LanguageToggle_UI", -60F, LocalizationManager.ToggleLanguage, out languageToggleLabel, out languageToggleLegacyLabel);
        CreateToggle("ModeToggle_UI", -100F, runner.CycleMode, out modeToggleLabel, out modeToggleLegacyLabel);
        LocalizationManager.LanguageChanged += OnLanguageChanged;
        runner.ModeChanged += OnModeChanged;
        UpdateLabels();
    }

    private void OnDestroy()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
        if (runner != null)
            runner.ModeChanged -= OnModeChanged;
    }

    private void CreateToggle(string objectName, float verticalPosition, UnityEngine.Events.UnityAction action, out TMP_Text label, out Text legacyLabel)
    {
        label = null;
        legacyLabel = null;
        GameObject template = SceneObjectFinder.Find("Match_ReFire");
        GameObject startUi = SceneObjectFinder.Find("Start_UI");
        if (template == null || startUi == null)
            return;

        GameObject toggle = Instantiate(template, startUi.transform);
        toggle.name = objectName;
        RectTransform rect = toggle.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0F, verticalPosition);

        Button button = toggle.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(action);
        label = toggle.GetComponentInChildren<TMP_Text>(true);
        legacyLabel = toggle.GetComponentInChildren<Text>(true);
    }

    private void OnLanguageChanged(Language language)
    {
        UpdateLabels();
    }

    private void OnModeChanged(ExperimentMode mode)
    {
        UpdateModeLabel();
    }

    private void UpdateLabels()
    {
        SetLabel(languageToggleLabel, languageToggleLegacyLabel, "中文 / EN");
        SetButtonLabel("Make_O2", runner.GetDefinition(1).GetName(), runner.GetDefinition(1).GetName());
        SetButtonLabel("Match_ReFire", runner.GetDefinition(2).GetName(), runner.GetDefinition(2).GetName());
        SetButtonLabel("CreateMatch_UI", "添加火柴", "Add Match");
        SetButtonLabel("DestoryMatch", "销毁火柴", "Discard Match");
        SetButtonLabel("ReG_UI", "重置实验室", "Reset Lab");
        SetButtonLabel("AdjustPosition", "调整位置", "Adjust Position");
        SetButtonLabel("AdjustAngle", "调整角度", "Adjust Angle");
        SetButtonLabel("Up", "上移", "Up");
        SetButtonLabel("Down", "下移", "Down");
        SetButtonLabel("Forward", "前移", "Forward");
        SetButtonLabel("Back", "后移", "Back");
        UpdateModeLabel();
    }

    private void UpdateModeLabel()
    {
        string text;
        switch (runner.CurrentMode)
        {
            case ExperimentMode.Guided:
                text = LocalizationManager.Get("模式：引导", "Mode: Guided");
                break;
            case ExperimentMode.FreePractice:
                text = LocalizationManager.Get("模式：自由练习", "Mode: Practice");
                break;
            default:
                text = LocalizationManager.Get("模式：考核", "Mode: Assessment");
                break;
        }
        SetLabel(modeToggleLabel, modeToggleLegacyLabel, text);
    }

    private static void SetLabel(TMP_Text label, Text legacyLabel, string text)
    {
        if (label != null)
            label.text = text;
        if (legacyLabel != null)
            legacyLabel.text = text;
    }

    private static void SetButtonLabel(string objectName, string chinese, string english)
    {
        GameObject buttonObject = SceneObjectFinder.Find(objectName);
        if (buttonObject == null)
            return;
        TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
        Text legacyLabel = buttonObject.GetComponentInChildren<Text>(true);
        SetLabel(label, legacyLabel, LocalizationManager.Get(chinese, english));
    }
}
