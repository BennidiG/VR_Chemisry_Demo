using System;
using UnityEngine;

public enum Language
{
    Chinese = 0,
    English = 1
}

public static class LocalizationManager
{
    private const string LanguagePreferenceKey = "VR_Chemistry_Language";
    private static Language currentLanguage = (Language)PlayerPrefs.GetInt(LanguagePreferenceKey, (int)Language.Chinese);

    public static Language CurrentLanguage => currentLanguage;
    public static event Action<Language> LanguageChanged;

    public static void SetLanguage(Language language)
    {
        if (currentLanguage == language)
            return;

        currentLanguage = language;
        PlayerPrefs.SetInt(LanguagePreferenceKey, (int)language);
        PlayerPrefs.Save();
        LanguageChanged?.Invoke(language);
    }

    public static void ToggleLanguage()
    {
        SetLanguage(currentLanguage == Language.Chinese ? Language.English : Language.Chinese);
    }

    public static string Get(string chinese, string english)
    {
        if (currentLanguage == Language.English && !string.IsNullOrWhiteSpace(english))
            return english;
        if (!string.IsNullOrWhiteSpace(chinese))
            return chinese;
        return english ?? string.Empty;
    }

    public static string GetItemName(LabItemType itemType)
    {
        switch (itemType)
        {
            case LabItemType.Match: return Get("火柴", "Match");
            case LabItemType.Spoon: return Get("药勺", "Spoon");
            case LabItemType.DrugBottle: return Get("药瓶", "Reagent bottle");
            case LabItemType.DrugBottleCap: return Get("药瓶盖", "Reagent bottle cap");
            case LabItemType.TestTube: return Get("试管", "Test tube");
            case LabItemType.TubeClamp: return Get("试管夹", "Test tube clamp");
            case LabItemType.Basin: return Get("水盆", "Water basin");
            case LabItemType.Catheter: return Get("导管", "Delivery tube");
            case LabItemType.AlcoholLamp: return Get("酒精灯", "Alcohol lamp");
            case LabItemType.AlcoholLampCap: return Get("酒精灯帽", "Alcohol lamp cap");
            case LabItemType.GlassBottle: return Get("氧气瓶", "Oxygen bottle");
            case LabItemType.GlassBottleCap: return Get("氧气瓶盖", "Oxygen bottle cap");
            case LabItemType.Cork: return Get("塞子", "Cork");
            case LabItemType.TestTubeRack: return Get("试管架及全部试管", "Test tube rack and all tubes");
            case LabItemType.Desk: return Get("实验台", "Laboratory desk");
            default: return Get("未知器材", "Unknown item");
        }
    }
}
