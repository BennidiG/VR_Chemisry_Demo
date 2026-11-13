using UnityEngine;
using TMPro;

/// <summary>
/// Compatibility adapter for the existing sockets and trigger scripts.
/// ExperimentRunner owns configuration-driven scoring; these fields preserve
/// the original public API while the collision-based flow is migrated gradually.
/// </summary>
public class Score : MonoBehaviour
{
    private GameObject Text_Score;
    private Xr_Ray Hit;
    private ExperimentRunner runner;

    private static bool Make_O2_Ready;
    private static bool Match_ReFire_Ready;
    private static bool Make_O2_Step1;
    private static bool Make_O2_Step2;
    private static bool Make_O2_Step3;
    private static bool Make_O2_Step4;
    private static bool Make_O2_Step5;
    private static bool Make_O2_Step6;
    private static bool Make_O2_Step7;
    private static bool Make_O2_Finish;
    private static bool Spoon_On_Position;
    private static bool DrugBottle_On_Position;
    private static bool DrugBottleCap_On_Position;
    private static bool TestTube1_On_Position;
    private static bool TestTube2_On_Position;
    private static bool TestTube3_On_Position;
    private static bool TestTube4_On_Position;
    private static bool TestTube5_On_Position;
    private static bool TestTube6_On_Position;
    private static bool TestTube7_On_Position;
    private static bool TestTube8_On_Position;
    private static bool TestTube9_On_Position;
    private static bool TestTube10_On_Position;
    private static bool TestTube11_On_Position;
    private static bool TestTube12_On_Position;
    private static bool TubeClamp_On_Position;
    private static bool Basin_On_Position;
    private static bool Catheter_On_Position;
    private static bool Mistake_AlcoholLamp;
    private static bool Match_ReFire_Step1;
    private static bool Match_ReFire_Step2;
    private static bool Match_ReFire_Step3;
    private static bool Match_ReFire_Finish;
    private static bool Match_Destory;
    private static bool AlcoholLamp_On_Position;
    private static bool AlcoholLampCap_On_Position;
    private static bool GlassBottle_On_Position;
    private static bool GlassBottleCap_On_Position;
    private static int Project;

    void Start()
    {
        runner = ExperimentRunner.GetOrCreate();
        runner.ExperimentReset += OnExperimentReset;
        Hit = FindObjectOfType<Xr_Ray>();
        Text_Score = GameObject.Find("Score");
        ResetLegacyState();
    }

    private void OnExperimentReset()
    {
        Make_O2_Ready = false;
        Match_ReFire_Ready = false;
        ResetMakeO2Steps();
        ResetMatchSteps();
        ClearCleanupProgress();
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.ExperimentReset -= OnExperimentReset;
    }

    private void ResetLegacyState()
    {
        Make_O2_Ready = false;
        Match_ReFire_Ready = false;
        ResetMakeO2Steps();
        ResetMatchSteps();
        Spoon_On_Position = true;
        DrugBottle_On_Position = true;
        DrugBottleCap_On_Position = true;
        SetAllTestTubes(true);
        TubeClamp_On_Position = true;
        Basin_On_Position = true;
        Catheter_On_Position = true;
        Mistake_AlcoholLamp = true;
        AlcoholLamp_On_Position = true;
        AlcoholLampCap_On_Position = true;
        GlassBottle_On_Position = true;
        GlassBottleCap_On_Position = true;
        Project = 0;
    }

    private int lastLegacyStateHash = int.MinValue;

    void Update()
    {
        If_Make_O2_Finish();
        If_Match_ReFire_Finish();
        SyncLegacyStateIfChanged();
        CalculateScore();
    }

    private void SyncLegacyStateIfChanged()
    {
        int stateHash = GetLegacyStateHash();
        if (stateHash == lastLegacyStateHash)
            return;
        lastLegacyStateHash = stateHash;
        SyncRunnerState();
    }

    private int GetLegacyStateHash()
    {
        int hash = 17;
        hash = hash * 31 + Project;
        AddState(ref hash, Make_O2_Ready);
        AddState(ref hash, Match_ReFire_Ready);
        AddState(ref hash, Make_O2_Step1);
        AddState(ref hash, Make_O2_Step2);
        AddState(ref hash, Make_O2_Step3);
        AddState(ref hash, Make_O2_Step4);
        AddState(ref hash, Make_O2_Step5);
        AddState(ref hash, Make_O2_Step6);
        AddState(ref hash, Make_O2_Step7);
        AddState(ref hash, Make_O2_Finish);
        AddState(ref hash, Match_ReFire_Step1);
        AddState(ref hash, Match_ReFire_Step2);
        AddState(ref hash, Match_ReFire_Step3);
        AddState(ref hash, Match_ReFire_Finish);
        AddState(ref hash, Match_Destory);
        AddState(ref hash, Spoon_On_Position);
        AddState(ref hash, DrugBottle_On_Position);
        AddState(ref hash, DrugBottleCap_On_Position);
        AddState(ref hash, TestTube1_On_Position);
        AddState(ref hash, TestTube2_On_Position);
        AddState(ref hash, TestTube3_On_Position);
        AddState(ref hash, TestTube4_On_Position);
        AddState(ref hash, TestTube5_On_Position);
        AddState(ref hash, TestTube6_On_Position);
        AddState(ref hash, TestTube7_On_Position);
        AddState(ref hash, TestTube8_On_Position);
        AddState(ref hash, TestTube9_On_Position);
        AddState(ref hash, TestTube10_On_Position);
        AddState(ref hash, TestTube11_On_Position);
        AddState(ref hash, TestTube12_On_Position);
        AddState(ref hash, TubeClamp_On_Position);
        AddState(ref hash, Basin_On_Position);
        AddState(ref hash, Catheter_On_Position);
        AddState(ref hash, AlcoholLamp_On_Position);
        AddState(ref hash, AlcoholLampCap_On_Position);
        AddState(ref hash, GlassBottle_On_Position);
        AddState(ref hash, GlassBottleCap_On_Position);
        AddState(ref hash, Mistake_AlcoholLamp);
        AddState(ref hash, LabItemRegistry.IsAtHome(LabItemType.Cork));
        return hash;
    }

    private static void AddState(ref int hash, bool value)
    {
        hash = hash * 31 + (value ? 1 : 0);
    }

    public bool If_Make_O2_Ready()
    {
        Make_O2_Ready = Hit != null && Hit.If_Make_O2_Ready();
        return Make_O2_Ready;
    }

    public bool If_Match_ReFire_Ready()
    {
        Match_ReFire_Ready = Hit != null && Hit.If_Match_ReFire_Ready();
        return Match_ReFire_Ready;
    }

    public void Yes_Make_O2_Step1() { Make_O2_Step1 = true; }
    public void No_Make_O2_Step1() { if (!Make_O2_Finish) Make_O2_Step1 = false; }
    public void Yes_Make_O2_Step2() { Make_O2_Step2 = true; }
    public void No_Make_O2_Step2() { if (!Make_O2_Finish) Make_O2_Step2 = false; }
    public void Make_O2_Step2_To_Step3() { if (Project == 1 && Make_O2_Step2) Make_O2_Step3 = true; }
    public void Yes_Make_O2_Step3() { Make_O2_Step3 = true; }
    public void No_Make_O2_Step3() { if (!Make_O2_Finish) Make_O2_Step3 = false; }
    public void Make_O2_Step3_To_Step4() { if (Project == 1 && Make_O2_Step3) Make_O2_Step4 = true; }
    public void Yes_Make_O2_Step4() { Make_O2_Step4 = true; }
    public void No_Make_O2_Step4() { if (!Make_O2_Finish) Make_O2_Step4 = false; }
    public void Make_O2_Step4_To_Step5() { if (Project == 1 && Make_O2_Step4) Make_O2_Step5 = true; }
    public void Yes_Make_O2_Step5() { Make_O2_Step5 = true; }
    public void No_Make_O2_Step5() { if (!Make_O2_Finish) Make_O2_Step5 = false; }
    public void Make_O2_Step5_To_Step6() { if (Project == 1 && Make_O2_Step5) Make_O2_Step6 = true; }
    public void Yes_Make_O2_Step6() { Make_O2_Step6 = true; }
    public void No_Make_O2_Step6() { if (!Make_O2_Finish) Make_O2_Step6 = false; }
    public void Yes_Make_O2_Step7() { Make_O2_Step7 = true; }
    public void No_Make_O2_Step7() { if (!Make_O2_Finish) Make_O2_Step7 = false; }

    public void If_Make_O2_Finish()
    {
        Make_O2_Finish = Make_O2_Step1 && Make_O2_Step2 && Make_O2_Step3 && Make_O2_Step4 && Make_O2_Step5 && Make_O2_Step6 && Make_O2_Step7;
    }

    public void Yes_Spoon_On_Position() { Spoon_On_Position = true; }
    public void No_Spoon_On_Position() { Spoon_On_Position = false; }
    public void Yes_DrugBottle_On_Position() { DrugBottle_On_Position = true; }
    public void No_DrugBottle_On_Position() { DrugBottle_On_Position = false; }
    public void Yes_DrugBottleCap_On_Position() { DrugBottleCap_On_Position = true; }
    public void No_DrugBottleCap_On_Position() { DrugBottleCap_On_Position = false; }
    public void Yes_TestTube1_On_Position() { TestTube1_On_Position = true; }
    public void No_TestTube1_On_Position() { TestTube1_On_Position = false; }
    public void Yes_TestTube2_On_Position() { TestTube2_On_Position = true; }
    public void No_TestTube2_On_Position() { TestTube2_On_Position = false; }
    public void Yes_TestTube3_On_Position() { TestTube3_On_Position = true; }
    public void No_TestTube3_On_Position() { TestTube3_On_Position = false; }
    public void Yes_TestTube4_On_Position() { TestTube4_On_Position = true; }
    public void No_TestTube4_On_Position() { TestTube4_On_Position = false; }
    public void Yes_TestTube5_On_Position() { TestTube5_On_Position = true; }
    public void No_TestTube5_On_Position() { TestTube5_On_Position = false; }
    public void Yes_TestTube6_On_Position() { TestTube6_On_Position = true; }
    public void No_TestTube6_On_Position() { TestTube6_On_Position = false; }
    public void Yes_TestTube7_On_Position() { TestTube7_On_Position = true; }
    public void No_TestTube7_On_Position() { TestTube7_On_Position = false; }
    public void Yes_TestTube8_On_Position() { TestTube8_On_Position = true; }
    public void No_TestTube8_On_Position() { TestTube8_On_Position = false; }
    public void Yes_TestTube9_On_Position() { TestTube9_On_Position = true; }
    public void No_TestTube9_On_Position() { TestTube9_On_Position = false; }
    public void Yes_TestTube10_On_Position() { TestTube10_On_Position = true; }
    public void No_TestTube10_On_Position() { TestTube10_On_Position = false; }
    public void Yes_TestTube11_On_Position() { TestTube11_On_Position = true; }
    public void No_TestTube11_On_Position() { TestTube11_On_Position = false; }
    public void Yes_TestTube12_On_Position() { TestTube12_On_Position = true; }
    public void No_TestTube12_On_Position() { TestTube12_On_Position = false; }

    public bool AllTestTube_On_Position()
    {
        return TestTube1_On_Position && TestTube2_On_Position && TestTube3_On_Position && TestTube4_On_Position && TestTube5_On_Position && TestTube6_On_Position && TestTube7_On_Position && TestTube8_On_Position && TestTube9_On_Position && TestTube10_On_Position && TestTube11_On_Position && TestTube12_On_Position;
    }

    public void Yes_TubeClamp_On_Position() { TubeClamp_On_Position = true; }
    public void No_TubeClamp_On_Position() { TubeClamp_On_Position = false; }
    public void Yes_Basin_On_Position() { Basin_On_Position = true; }
    public void No_Basin_On_Position() { Basin_On_Position = false; }
    public void Yes_Catheter_On_Position() { Catheter_On_Position = true; }
    public void No_Catheter_On_Position() { Catheter_On_Position = false; }

    public bool If_Make_O2_Step1() { return Make_O2_Step1; }
    public bool If_Make_O2_Step2() { return Make_O2_Step2; }
    public bool If_Make_O2_Step3() { return Make_O2_Step3; }
    public bool If_Make_O2_Step4() { return Make_O2_Step4; }
    public bool If_Make_O2_Step5() { return Make_O2_Step5; }
    public bool If_Make_O2_Step6() { return Make_O2_Step6; }
    public bool If_Make_O2_Step7() { return Make_O2_Step7; }

    public bool IF_Make_O2_Step8()
    {
        SyncLegacyStateIfChanged();
        If_Make_O2_Finish();
        return Make_O2_Finish;
    }

    public void Yes_Match_ReFire_Step1() { Match_ReFire_Step1 = true; }
    public void No_Match_ReFire_Step1() { if (!Match_ReFire_Finish) Match_ReFire_Step1 = false; }
    public void Yes_Match_ReFire_Step2() { Match_ReFire_Step2 = true; }
    public void No_Match_ReFire_Step2() { if (!Match_ReFire_Finish) Match_ReFire_Step2 = false; }
    public void Yes_Match_ReFire_Step3() { Match_ReFire_Step3 = true; }
    public void No_Match_ReFire_Step3() { if (!Match_ReFire_Finish) Match_ReFire_Step3 = false; }

    public void If_Match_ReFire_Finish()
    {
        Match_ReFire_Finish = Match_ReFire_Step1 && Match_ReFire_Step2 && Match_ReFire_Step3;
    }

    public void Yes_Match_Destory()
    {
        Match_Destory = true;
        SyncRunnerState();
    }
    public void No_Match_Destory() { Match_Destory = false; }
    public void Yes_AlcoholLamp_On_Position() { AlcoholLamp_On_Position = true; }
    public void No_AlcoholLamp_On_Position() { AlcoholLamp_On_Position = false; }
    public void Yes_AlcoholLampCap_On_Position() { AlcoholLampCap_On_Position = true; }
    public void No_AlcoholLampCap_On_Position() { AlcoholLampCap_On_Position = false; }
    public void Yes_GlassBottle_On_Position() { GlassBottle_On_Position = true; }
    public void No_GlassBottle_On_Position() { GlassBottle_On_Position = false; }
    public void Yes_GlassBottleCap_On_Position() { GlassBottleCap_On_Position = true; }
    public void No_GlassBottleCap_On_Position() { GlassBottleCap_On_Position = false; }

    public bool If_Match_ReFire_Step1() { return Match_ReFire_Step1; }
    public bool If_Match_ReFire_Step2() { return Match_ReFire_Step2; }
    public bool If_Match_ReFire_Step3() { return Match_ReFire_Step3; }

    public bool If_Match_ReFire_Step4()
    {
        SyncLegacyStateIfChanged();
        If_Match_ReFire_Finish();
        return Match_ReFire_Finish;
    }

    public void Nope_Mistake_AlcoholLamp() { Mistake_AlcoholLamp = true; }
    public void Have_Mistake_AlcoholLamp() { Mistake_AlcoholLamp = false; }

    public void Choose_Project1()
    {
        Project = 1;
        Make_O2_Ready = false;
        ResetMakeO2Steps();
        runner.SelectExperiment(1);
    }

    public void Project1()
    {
        Project = 1;
    }

    public void Choose_Project2()
    {
        Project = 2;
        Match_ReFire_Ready = false;
        ResetMatchSteps();
        runner.SelectExperiment(2);
    }

    public int Project_Num()
    {
        return Project;
    }

    private void ResetMakeO2Steps()
    {
        Make_O2_Step1 = false;
        Make_O2_Step2 = false;
        Make_O2_Step3 = false;
        Make_O2_Step4 = false;
        Make_O2_Step5 = false;
        Make_O2_Step6 = false;
        Make_O2_Step7 = false;
        Make_O2_Finish = false;
        Match_Destory = false;
    }

    private void ResetMatchSteps()
    {
        Match_ReFire_Step1 = false;
        Match_ReFire_Step2 = false;
        Match_ReFire_Step3 = false;
        Match_ReFire_Finish = false;
        Match_Destory = false;
    }

    private void SetAllTestTubes(bool onPosition)
    {
        TestTube1_On_Position = onPosition;
        TestTube2_On_Position = onPosition;
        TestTube3_On_Position = onPosition;
        TestTube4_On_Position = onPosition;
        TestTube5_On_Position = onPosition;
        TestTube6_On_Position = onPosition;
        TestTube7_On_Position = onPosition;
        TestTube8_On_Position = onPosition;
        TestTube9_On_Position = onPosition;
        TestTube10_On_Position = onPosition;
        TestTube11_On_Position = onPosition;
        TestTube12_On_Position = onPosition;
    }

    private void CalculateScore()
    {
        if (Text_Score != null)
            Text_Score.GetComponent<TMP_Text>().text = runner.GetScoreText();
    }

    public void SyncRunnerState()
    {
        if (runner == null || Project == 0 || runner.CurrentProject != Project)
            return;

        if (Project == 1)
        {
            runner.SetStepCompletedFromLegacy("take_reagent", Make_O2_Step1);
            runner.SetStepCompletedFromLegacy("add_to_test_tube", Make_O2_Step2);
            runner.SetStepCompletedFromLegacy("attach_cork", Make_O2_Step3);
            runner.SetStepCompletedFromLegacy("connect_catheter", Make_O2_Step4);
            runner.SetStepCompletedFromLegacy("secure_test_tube", Make_O2_Step5);
            runner.SetStepCompletedFromLegacy("heat_test_tube", Make_O2_Step6);
            runner.SetStepCompletedFromLegacy("collect_oxygen", Make_O2_Step7);
            runner.SetCleanupCompletedFromLegacy("all_test_tubes", AllTestTube_On_Position());
            runner.SetCleanupCompletedFromLegacy("spoon", Spoon_On_Position);
            runner.SetCleanupCompletedFromLegacy("drug_bottle", DrugBottle_On_Position);
            runner.SetCleanupCompletedFromLegacy("drug_bottle_cap", DrugBottleCap_On_Position);
            runner.SetCleanupCompletedFromLegacy("tube_clamp", TubeClamp_On_Position);
            runner.SetCleanupCompletedFromLegacy("basin", Basin_On_Position);
            runner.SetCleanupCompletedFromLegacy("catheter", Catheter_On_Position);
            runner.SetCleanupCompletedFromLegacy("match_destroyed", Match_Destory);
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp", AlcoholLamp_On_Position);
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp_cap", AlcoholLampCap_On_Position);
            runner.SetCleanupCompletedFromLegacy("glass_bottle", GlassBottle_On_Position);
            runner.SetCleanupCompletedFromLegacy("glass_bottle_cap", GlassBottleCap_On_Position);
            runner.SetCleanupCompletedFromLegacy("cork", LabItemRegistry.IsAtHome(LabItemType.Cork));
        }
        else if (Project == 2)
        {
            runner.SetStepCompletedFromLegacy("ignite_match_and_lamp", Match_ReFire_Step1);
            runner.SetStepCompletedFromLegacy("extinguish_match_keep_ember", Match_ReFire_Step2);
            runner.SetStepCompletedFromLegacy("refire_in_oxygen", Match_ReFire_Step3);
            runner.SetCleanupCompletedFromLegacy("match_destroyed", Match_Destory);
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp", AlcoholLamp_On_Position);
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp_cap", AlcoholLampCap_On_Position);
            runner.SetCleanupCompletedFromLegacy("glass_bottle", GlassBottle_On_Position);
            runner.SetCleanupCompletedFromLegacy("glass_bottle_cap", GlassBottleCap_On_Position);
        }
    }

    // Used only by the editor/development debug controller and EditMode tests.
    public void DebugCompleteNextStep()
    {
        ExperimentStepDefinition step = runner.CurrentStep;
        if (step == null)
            return;

        switch (step.stepId)
        {
            case "take_reagent": Yes_Make_O2_Step1(); break;
            case "add_to_test_tube": Yes_Make_O2_Step2(); break;
            case "attach_cork": Make_O2_Step2_To_Step3(); break;
            case "connect_catheter": Make_O2_Step3_To_Step4(); break;
            case "secure_test_tube": Make_O2_Step4_To_Step5(); break;
            case "heat_test_tube": Make_O2_Step5_To_Step6(); break;
            case "collect_oxygen": Yes_Make_O2_Step7(); break;
            case "ignite_match_and_lamp": Yes_Match_ReFire_Step1(); break;
            case "extinguish_match_keep_ember": Yes_Match_ReFire_Step2(); break;
            case "refire_in_oxygen": Yes_Match_ReFire_Step3(); break;
        }
        SyncRunnerState();
    }

    public void DebugClearCleanup()
    {
        ClearCleanupProgress();
        SyncRunnerState();
    }

    private void ClearCleanupProgress()
    {
        SetAllTestTubes(false);
        Spoon_On_Position = false;
        DrugBottle_On_Position = false;
        DrugBottleCap_On_Position = false;
        TubeClamp_On_Position = false;
        Basin_On_Position = false;
        Catheter_On_Position = false;
        Match_Destory = false;
        AlcoholLamp_On_Position = false;
        AlcoholLampCap_On_Position = false;
        GlassBottle_On_Position = false;
        GlassBottleCap_On_Position = false;
        SyncRunnerState();
    }

    public void DebugCompleteNextCleanup()
    {
        if (runner.CurrentDefinition == null)
            return;

        foreach (CleanupCriterion criterion in runner.CurrentDefinition.cleanupCriteria)
        {
            if (runner.IsCleanupCompleted(criterion.cleanupId))
                continue;
            DebugSetCleanup(criterion.cleanupId);
            break;
        }
        SyncRunnerState();
    }

    private void DebugSetCleanup(string cleanupId)
    {
        switch (cleanupId)
        {
            case "all_test_tubes": SetAllTestTubes(true); break;
            case "spoon": Yes_Spoon_On_Position(); break;
            case "drug_bottle": Yes_DrugBottle_On_Position(); break;
            case "drug_bottle_cap": Yes_DrugBottleCap_On_Position(); break;
            case "tube_clamp": Yes_TubeClamp_On_Position(); break;
            case "basin": Yes_Basin_On_Position(); break;
            case "catheter": Yes_Catheter_On_Position(); break;
            case "cork": LabItemRegistry.DebugReturnHome(LabItemType.Cork); break;
            case "match_destroyed": Yes_Match_Destory(); break;
            case "alcohol_lamp": Yes_AlcoholLamp_On_Position(); break;
            case "alcohol_lamp_cap": Yes_AlcoholLampCap_On_Position(); break;
            case "glass_bottle": Yes_GlassBottle_On_Position(); break;
            case "glass_bottle_cap": Yes_GlassBottleCap_On_Position(); break;
        }
    }
}
