using System;
using UnityEngine;

/// <summary>
/// The only legacy-to-Runner translation point. Score keeps compatibility with
/// old UnityEvents, but this adapter owns all legacy state writes.
/// </summary>
public class LegacyExperimentAdapter : MonoBehaviour
{
    private Score score;
    private ExperimentRunner runner;

    private void Start()
    {
        score = FindObjectOfType<Score>();
        runner = ExperimentRunner.GetOrCreate();
        if (score != null)
        {
            score.LegacyStateChanged += Apply;
            Apply();
        }
    }

    private void OnDestroy()
    {
        if (score != null)
            score.LegacyStateChanged -= Apply;
    }

    public void Apply()
    {
        if (score == null || runner == null)
            return;
        int project = score.Project_Num();
        if (project == 0 || runner.CurrentProject != project)
            return;

        if (project == 1)
        {
            runner.SetStepCompletedFromLegacy("take_reagent", score.If_Make_O2_Step1());
            runner.SetStepCompletedFromLegacy("add_to_test_tube", score.If_Make_O2_Step2());
            runner.SetStepCompletedFromLegacy("attach_cork", score.If_Make_O2_Step3());
            runner.SetStepCompletedFromLegacy("connect_catheter", score.If_Make_O2_Step4());
            runner.SetStepCompletedFromLegacy("secure_test_tube", score.If_Make_O2_Step5());
            runner.SetStepCompletedFromLegacy("heat_test_tube", score.If_Make_O2_Step6());
            runner.SetStepCompletedFromLegacy("collect_oxygen", score.If_Make_O2_Step7());

            runner.SetCleanupCompletedFromLegacy("all_test_tubes", score.AllTestTube_On_Position());
            runner.SetCleanupCompletedFromLegacy("spoon", score.IsCleanupFlag("spoon"));
            runner.SetCleanupCompletedFromLegacy("drug_bottle", score.IsCleanupFlag("drug_bottle"));
            runner.SetCleanupCompletedFromLegacy("drug_bottle_cap", score.IsCleanupFlag("drug_bottle_cap"));
            runner.SetCleanupCompletedFromLegacy("tube_clamp", score.IsCleanupFlag("tube_clamp"));
            runner.SetCleanupCompletedFromLegacy("basin", score.IsCleanupFlag("basin"));
            runner.SetCleanupCompletedFromLegacy("catheter", score.IsCleanupFlag("catheter"));
            runner.SetCleanupCompletedFromLegacy("match_destroyed", score.IsCleanupFlag("match_destroyed"));
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp", score.IsCleanupFlag("alcohol_lamp"));
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp_cap", score.IsCleanupFlag("alcohol_lamp_cap"));
            runner.SetCleanupCompletedFromLegacy("glass_bottle", score.IsCleanupFlag("glass_bottle"));
            runner.SetCleanupCompletedFromLegacy("glass_bottle_cap", score.IsCleanupFlag("glass_bottle_cap"));
            runner.SetCleanupCompletedFromLegacy("cork", LabItemRegistry.IsAtHome(LabItemType.Cork));
        }
        else if (project == 2)
        {
            runner.SetStepCompletedFromLegacy("ignite_match_and_lamp", score.If_Match_ReFire_Step1());
            runner.SetStepCompletedFromLegacy("extinguish_match_keep_ember", score.If_Match_ReFire_Step2());
            runner.SetStepCompletedFromLegacy("refire_in_oxygen", score.If_Match_ReFire_Step3());
            runner.SetCleanupCompletedFromLegacy("match_destroyed", score.IsCleanupFlag("match_destroyed"));
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp", score.IsCleanupFlag("alcohol_lamp"));
            runner.SetCleanupCompletedFromLegacy("alcohol_lamp_cap", score.IsCleanupFlag("alcohol_lamp_cap"));
            runner.SetCleanupCompletedFromLegacy("glass_bottle", score.IsCleanupFlag("glass_bottle"));
            runner.SetCleanupCompletedFromLegacy("glass_bottle_cap", score.IsCleanupFlag("glass_bottle_cap"));
        }
    }
}
