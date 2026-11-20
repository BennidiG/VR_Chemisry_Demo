using UnityEngine;

public class TrashTrigger : MonoBehaviour
{
    private CreateMatch IF_CreateMatch;
    private Score Match_ReFire_Score;

    void Start()
    {
        IF_CreateMatch = FindObjectOfType<CreateMatch>();
        Match_ReFire_Score = FindObjectOfType<Score>();
    }

    public void OnTriggerEnter(Collider collider)
    {
        GameObject target = collider.attachedRigidbody != null
            ? collider.attachedRigidbody.gameObject
            : collider.gameObject;

        // Never destroy the trash can or one of its own parents/children.
        if (target == gameObject || target.transform.IsChildOf(transform) || transform.IsChildOf(target.transform))
            return;

        if (target.name == "Match")
        {
            if (Match_ReFire_Score != null && ((Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Step4()) || (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.IF_Make_O2_Step8())))
                Match_ReFire_Score.Yes_Match_Destory();

            Destroy(target);
            if (IF_CreateMatch != null)
                IF_CreateMatch.Clear_Old_Match();
        }
        else
        {
            LabItem labItem = target.GetComponent<LabItem>();
            if (labItem != null)
            {
                ClearCleanupState(labItem.itemType);
                target.SetActive(false);
            }
            else
            {
                Destroy(target);
            }
        }
    }

    private void ClearCleanupState(LabItemType itemType)
    {
        if (Match_ReFire_Score == null)
            return;

        switch (itemType)
        {
            case LabItemType.Spoon: Match_ReFire_Score.No_Spoon_On_Position(); break;
            case LabItemType.DrugBottle: Match_ReFire_Score.No_DrugBottle_On_Position(); break;
            case LabItemType.DrugBottleCap: Match_ReFire_Score.No_DrugBottleCap_On_Position(); break;
            case LabItemType.TestTube: Match_ReFire_Score.No_TestTube1_On_Position(); break;
            case LabItemType.TubeClamp: Match_ReFire_Score.No_TubeClamp_On_Position(); break;
            case LabItemType.Basin: Match_ReFire_Score.No_Basin_On_Position(); break;
            case LabItemType.Catheter: Match_ReFire_Score.No_Catheter_On_Position(); break;
            case LabItemType.AlcoholLamp: Match_ReFire_Score.No_AlcoholLamp_On_Position(); break;
            case LabItemType.AlcoholLampCap: Match_ReFire_Score.No_AlcoholLampCap_On_Position(); break;
            case LabItemType.GlassBottle: Match_ReFire_Score.No_GlassBottle_On_Position(); break;
            case LabItemType.GlassBottleCap: Match_ReFire_Score.No_GlassBottleCap_On_Position(); break;
            case LabItemType.TestTubeRack:
                Match_ReFire_Score.No_TestTube1_On_Position();
                Match_ReFire_Score.No_TestTube2_On_Position();
                Match_ReFire_Score.No_TestTube3_On_Position();
                Match_ReFire_Score.No_TestTube4_On_Position();
                Match_ReFire_Score.No_TestTube5_On_Position();
                Match_ReFire_Score.No_TestTube6_On_Position();
                Match_ReFire_Score.No_TestTube7_On_Position();
                Match_ReFire_Score.No_TestTube8_On_Position();
                Match_ReFire_Score.No_TestTube9_On_Position();
                Match_ReFire_Score.No_TestTube10_On_Position();
                Match_ReFire_Score.No_TestTube11_On_Position();
                Match_ReFire_Score.No_TestTube12_On_Position();
                break;
        }
    }
}
