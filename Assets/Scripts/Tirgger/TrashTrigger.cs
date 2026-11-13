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
                target.SetActive(false);
            else
                Destroy(target);
        }
    }
}
