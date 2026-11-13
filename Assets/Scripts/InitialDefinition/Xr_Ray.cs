using UnityEngine;

/// <summary>
/// Socket-based equipment preparation. Localized preparation UI is rendered by
/// ExperimentGuidanceController using the same ExperimentRunner state.
/// </summary>
public class Xr_Ray : MonoBehaviour
{
    // Kept for compatibility with the existing serialized scene component.
    public GameObject CubeCanvas;
    public GameObject Up;
    public GameObject Down;
    public GameObject Forward;
    public GameObject Back;

    private static bool Prepare_Match_On_Position;
    private static bool Prepare_Spoon_On_Position;
    private static bool Prepare_DrugBottle_On_Position;
    private static bool Prepare_GlassBottle_On_Position;
    private static bool Prepare_AlcoholLamp_On_Position;
    private static bool Prepare_TestTube_On_Position;
    private static bool Prepare_TubeClamp_On_Position;
    private static bool Prepare_Basin_On_Position;
    private static bool Prepare_Catheter_On_Position;
    private static bool Make_O2_Ready;
    private static bool Match_ReFire_Ready;
    private static int Project;

    private GameObject Hints;
    private GameObject Text_Make_O2_Steps;
    private GameObject Text_Match_ReFire_Steps;
    private ExperimentRunner runner;

    private void Awake()
    {
        runner = ExperimentRunner.GetOrCreate();
        runner.ExperimentReset += OnExperimentReset;
        Hints = SceneObjectFinder.Find("Hints");
        Text_Make_O2_Steps = SceneObjectFinder.Find("Make_O2_Steps");
        Text_Match_ReFire_Steps = SceneObjectFinder.Find("Match_ReFire_Steps");

        Prepare_Match_On_Position = false;
        Prepare_Spoon_On_Position = false;
        Prepare_DrugBottle_On_Position = false;
        Prepare_GlassBottle_On_Position = false;
        Prepare_AlcoholLamp_On_Position = false;
        Prepare_TestTube_On_Position = false;
        Prepare_TubeClamp_On_Position = false;
        Prepare_Basin_On_Position = false;
        Prepare_Catheter_On_Position = false;
        Make_O2_Ready = false;
        Match_ReFire_Ready = false;
        Project = 0;
    }

    private void OnExperimentReset()
    {
        CancelInvoke("Project1_Ready");
        CancelInvoke("Project2_Ready");
        Prepare_Match_On_Position = false;
        Prepare_Spoon_On_Position = false;
        Prepare_DrugBottle_On_Position = false;
        Prepare_GlassBottle_On_Position = false;
        Prepare_AlcoholLamp_On_Position = false;
        Prepare_TestTube_On_Position = false;
        Prepare_TubeClamp_On_Position = false;
        Prepare_Basin_On_Position = false;
        Prepare_Catheter_On_Position = false;
        Make_O2_Ready = false;
        Match_ReFire_Ready = false;
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.ExperimentReset -= OnExperimentReset;
    }

    public void Yes_Prepare_Match_On_Position() { SetPreparation(LabItemType.Match, true); }
    public void No_Prepare_Match_On_Position() { SetPreparation(LabItemType.Match, false); }
    public void Yes_Prepare_Spoon_On_Position() { SetPreparation(LabItemType.Spoon, true); }
    public void No_Prepare_Spoon_On_Position() { SetPreparation(LabItemType.Spoon, false); }
    public void Yes_Prepare_DrugBottle_On_Position() { SetPreparation(LabItemType.DrugBottle, true); }
    public void No_Prepare_DrugBottle_On_Position() { SetPreparation(LabItemType.DrugBottle, false); }
    public void Yes_Prepare_GlassBottle_On_Position() { SetPreparation(LabItemType.GlassBottle, true); }
    public void No_Prepare_GlassBottle_On_Position() { SetPreparation(LabItemType.GlassBottle, false); }
    public void Yes_Prepare_AlcoholLamp_On_Position() { SetPreparation(LabItemType.AlcoholLamp, true); }
    public void No_Prepare_AlcoholLamp_On_Position() { SetPreparation(LabItemType.AlcoholLamp, false); }
    public void Yes_Prepare_TestTube_On_Position() { SetPreparation(LabItemType.TestTube, true); }
    public void No_Prepare_TestTube_On_Position() { SetPreparation(LabItemType.TestTube, false); }
    public void Yes_Prepare_TubeClamp_On_Position() { SetPreparation(LabItemType.TubeClamp, true); }
    public void No_Prepare_TubeClamp_On_Position() { SetPreparation(LabItemType.TubeClamp, false); }
    public void Yes_Prepare_Basin_On_Position() { SetPreparation(LabItemType.Basin, true); }
    public void No_Prepare_Basin_On_Position() { SetPreparation(LabItemType.Basin, false); }
    public void Yes_Prepare_Catheter_On_Position() { SetPreparation(LabItemType.Catheter, true); }
    public void No_Prepare_Catheter_On_Position() { SetPreparation(LabItemType.Catheter, false); }

    private void SetPreparation(LabItemType itemType, bool onPosition)
    {
        switch (itemType)
        {
            case LabItemType.Match: Prepare_Match_On_Position = onPosition; break;
            case LabItemType.Spoon: Prepare_Spoon_On_Position = onPosition; break;
            case LabItemType.DrugBottle: Prepare_DrugBottle_On_Position = onPosition; break;
            case LabItemType.GlassBottle: Prepare_GlassBottle_On_Position = onPosition; break;
            case LabItemType.AlcoholLamp: Prepare_AlcoholLamp_On_Position = onPosition; break;
            case LabItemType.TestTube: Prepare_TestTube_On_Position = onPosition; break;
            case LabItemType.TubeClamp: Prepare_TubeClamp_On_Position = onPosition; break;
            case LabItemType.Basin: Prepare_Basin_On_Position = onPosition; break;
            case LabItemType.Catheter: Prepare_Catheter_On_Position = onPosition; break;
        }

        runner.SetEquipmentOnPosition(itemType, onPosition);
        Prepare_On_Position();
    }

    private void Prepare_On_Position()
    {
        if ((Project == 1 && Make_O2_Ready) || (Project == 2 && Match_ReFire_Ready) || Project == 0)
            return;
        if (runner.CurrentProject != Project)
            return;

        SyncEquipmentPlacement();
        string readyMethod = Project == 1 ? "Project1_Ready" : "Project2_Ready";
        if (runner.AreRequiredEquipmentReady())
        {
            if (!IsInvoking(readyMethod))
                Invoke(readyMethod, 2F);
        }
        else
        {
            CancelInvoke(readyMethod);
        }
    }

    private void SyncEquipmentPlacement()
    {
        runner.SetEquipmentOnPosition(LabItemType.Match, Prepare_Match_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.Spoon, Prepare_Spoon_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.DrugBottle, Prepare_DrugBottle_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.GlassBottle, Prepare_GlassBottle_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.AlcoholLamp, Prepare_AlcoholLamp_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.TestTube, Prepare_TestTube_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.TubeClamp, Prepare_TubeClamp_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.Basin, Prepare_Basin_On_Position);
        runner.SetEquipmentOnPosition(LabItemType.Catheter, Prepare_Catheter_On_Position);
    }

    public void Make_O2()
    {
        CancelInvoke("Project1_Ready");
        CancelInvoke("Project2_Ready");
        Make_O2_Ready = false;
        Project = 1;
        if (runner.CurrentProject != Project)
            runner.SelectExperiment(Project);
        Prepare_On_Position();
    }

    public void Make_O2_ReFire()
    {
        CancelInvoke("Project1_Ready");
        CancelInvoke("Project2_Ready");
        Match_ReFire_Ready = false;
        Project = 2;
        if (runner.CurrentProject != Project)
            runner.SelectExperiment(Project);
        Prepare_On_Position();
    }

    // Legacy entry points retained for scenes or tools that used the old ray path.
    public void Make_O2_ReadyStep() { Prepare_On_Position(); }
    public void Match_ReFire_ReadyStep() { Prepare_On_Position(); }

    public void Project1_Ready()
    {
        if (Project != 1)
            return;
        SyncEquipmentPlacement();
        if (!runner.AreRequiredEquipmentReady())
            return;

        Make_O2_Ready = true;
        Hints.SetActive(true);
        Text_Make_O2_Steps.SetActive(true);
        Text_Match_ReFire_Steps.SetActive(false);
        runner.MarkPrepared();
    }

    public void Project2_Ready()
    {
        if (Project != 2)
            return;
        SyncEquipmentPlacement();
        if (!runner.AreRequiredEquipmentReady())
            return;

        Match_ReFire_Ready = true;
        Hints.SetActive(true);
        Text_Match_ReFire_Steps.SetActive(true);
        Text_Make_O2_Steps.SetActive(false);
        runner.MarkPrepared();
    }

    public bool If_Make_O2_Ready()
    {
        return Make_O2_Ready;
    }

    public bool If_Match_ReFire_Ready()
    {
        return Match_ReFire_Ready;
    }
}
