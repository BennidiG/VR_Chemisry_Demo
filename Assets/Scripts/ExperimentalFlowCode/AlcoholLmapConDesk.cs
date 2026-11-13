using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlcoholLmapConDesk : MonoBehaviour
{
    private GameObject Text_Make_O2_Step6;

    private Score Match_ReFire_Score;
    private MakeO2 LampIsFire;
    // Start is called before the first frame update
    void Start()
    {
        Match_ReFire_Score = FindObjectOfType<Score>();
        LampIsFire = FindObjectOfType<MakeO2>();
        Text_Make_O2_Step6 = SceneObjectFinder.Find("Make_O2_Step6");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Cylinder001")
        {
            if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step5())
            {
                Text_Make_O2_Step6 = SceneObjectFinder.Find("Make_O2_Step6");
                if (LampIsFire.IfLampIsFire())
                {
                    LampIsFire.TryCompleteHeatingStep();
                }
                else {
                    Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
                    Match_ReFire_Score.No_Make_O2_Step6();
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "Cylinder001")
        {
            if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step5())
            {
                Text_Make_O2_Step6 = SceneObjectFinder.Find("Make_O2_Step6");
                if (LampIsFire.IfLampIsFire())
                {
                    LampIsFire.TryCompleteHeatingStep();
                }
                else
                {
                    Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
                    Match_ReFire_Score.No_Make_O2_Step6();
                }
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "Cylinder001")
        {
            Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
            Match_ReFire_Score.No_Make_O2_Step6();
        }
    }
}
