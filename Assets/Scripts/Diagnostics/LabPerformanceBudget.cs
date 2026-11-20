using System;
using UnityEngine;

/// <summary>
/// Optional Quest/QuestLow A/B profile switch. The default Quest profile is
/// preserved unless applyOnAndroid is explicitly enabled or F8 is pressed.
/// </summary>
public class LabPerformanceBudget : MonoBehaviour
{
    public bool applyOnAndroid;
    public string questProfile = "Quest";
    public string lowProfile = "QuestLow";

    private void Start()
    {
        if (applyOnAndroid && Application.platform == RuntimePlatform.Android)
            SetProfile(lowProfile);
    }

    public void ToggleProfile()
    {
        string current = QualitySettings.names[QualitySettings.GetQualityLevel()];
        SetProfile(current == lowProfile ? questProfile : lowProfile);
    }

    public void SetProfile(string profileName)
    {
        int index = Array.IndexOf(QualitySettings.names, profileName);
        if (index < 0)
        {
            Debug.LogWarning("Quality profile is not configured: " + profileName);
            return;
        }
        QualitySettings.SetQualityLevel(index, true);
        Debug.Log("Quality profile changed to " + profileName);
    }
}
