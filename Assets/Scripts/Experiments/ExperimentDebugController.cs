using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Keyboard-only verification of configuration, scoring and teaching UI.
/// It does not simulate collisions, VFX or the chemistry itself.
/// </summary>
public class ExperimentDebugController : MonoBehaviour
{
    private StartPrepare startPrepare;
    private Score score;
    private Xr_Ray preparation;
    private ReG reset;

    private void Start()
    {
        startPrepare = FindObjectOfType<StartPrepare>();
        score = FindObjectOfType<Score>();
        preparation = FindObjectOfType<Xr_Ray>();
        reset = FindObjectOfType<ReG>();
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Keyboard.current == null || startPrepare == null || score == null || preparation == null)
            return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            SelectProject(1);
        else if (Keyboard.current.f2Key.wasPressedThisFrame)
            SelectProject(2);
        else if (Keyboard.current.f3Key.wasPressedThisFrame)
            PrepareAll();
        else if (Keyboard.current.f4Key.wasPressedThisFrame)
            score.DebugCompleteNextStep();
        else if (Keyboard.current.f5Key.wasPressedThisFrame)
            score.DebugCompleteNextCleanup();
        else if (Keyboard.current.mKey.wasPressedThisFrame)
            ExperimentRunner.GetOrCreate().CycleMode();
        else if (Keyboard.current.lKey.wasPressedThisFrame)
            LocalizationManager.ToggleLanguage();
        else if (Keyboard.current.rKey.wasPressedThisFrame && reset != null)
            reset.Click();
#endif
    }

    private void SelectProject(int projectNumber)
    {
        if (projectNumber == 1)
        {
            startPrepare.Make_O2();
            score.Choose_Project1();
            preparation.Make_O2();
        }
        else
        {
            startPrepare.Match_ReFire();
            score.Choose_Project2();
            preparation.Make_O2_ReFire();
        }
    }

    private void PrepareAll()
    {
        if (preparation == null || score == null)
            return;

        if (score.Project_Num() == 1)
        {
            preparation.Yes_Prepare_Match_On_Position();
            preparation.Yes_Prepare_Spoon_On_Position();
            preparation.Yes_Prepare_DrugBottle_On_Position();
            preparation.Yes_Prepare_GlassBottle_On_Position();
            preparation.Yes_Prepare_AlcoholLamp_On_Position();
            preparation.Yes_Prepare_TestTube_On_Position();
            preparation.Yes_Prepare_TubeClamp_On_Position();
            preparation.Yes_Prepare_Basin_On_Position();
            preparation.Yes_Prepare_Catheter_On_Position();
            preparation.Project1_Ready();
        }
        else if (score.Project_Num() == 2)
        {
            preparation.Yes_Prepare_Match_On_Position();
            preparation.Yes_Prepare_GlassBottle_On_Position();
            preparation.Yes_Prepare_AlcoholLamp_On_Position();
            preparation.Project2_Ready();
        }
        score.DebugClearCleanup();
    }
}
