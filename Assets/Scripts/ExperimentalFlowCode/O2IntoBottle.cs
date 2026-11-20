using UnityEngine;
using TMPro;

public class O2IntoBottle : MonoBehaviour
{
    private static bool MakeingO2;

    private TMP_Text Make_O2_Step7;
    private OxygenSupply O2Count;
    private Score Match_ReFire_Score;
    private ExperimentRunner runner;

    [Min(0F)] public float minimumCollectionAmount = 120F;

    void Start()
    {
        MakeingO2 = false;
        runner = ExperimentRunner.GetOrCreate();
        runner.ExperimentReset += OnExperimentReset;

        GameObject stepObject = SceneObjectFinder.Find("Make_O2_Step7");
        if (stepObject != null)
            Make_O2_Step7 = stepObject.GetComponent<TMP_Text>();

        O2Count = FindObjectOfType<OxygenSupply>();
        Match_ReFire_Score = FindObjectOfType<Score>();
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.ExperimentReset -= OnExperimentReset;
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "O2IntoColl" && CanCollectOxygen())
            TryCompleteCollectionStep();
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name != "O2IntoColl" || !CanCollectOxygen())
            return;

        TryCompleteCollectionStep();
        ChemistryLegacyAdapter.AddProducedOxygen(O2Count, Time.fixedDeltaTime, Match_ReFire_Score != null && Match_ReFire_Score.If_Make_O2_Step4());
    }

    private bool CanCollectOxygen()
    {
        return MakeingO2 && Match_ReFire_Score != null && Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step4() && Match_ReFire_Score.If_Make_O2_Step6() && O2Count != null;
    }

    private void TryCompleteCollectionStep()
    {
        if (Match_ReFire_Score == null || Match_ReFire_Score.Project_Num() != 1)
            return;
        if (!Match_ReFire_Score.If_Make_O2_Step6() || Match_ReFire_Score.If_Make_O2_Step7())
            return;
        if (O2Count == null || O2Count.Amount < minimumCollectionAmount)
            return;

        if (Make_O2_Step7 != null)
            Make_O2_Step7.color = Color.green;
        Match_ReFire_Score.Yes_Make_O2_Step7();
    }

    public void MakeingO2True()
    {
        MakeingO2 = true;
    }

    public void MakeingO2False()
    {
        MakeingO2 = false;
    }

    public void ResetForExperiment()
    {
        MakeingO2 = false;
        if (O2Count != null)
            O2Count.ResetSupply();
    }

    private void OnExperimentReset()
    {
        MakeingO2 = false;
    }
}
