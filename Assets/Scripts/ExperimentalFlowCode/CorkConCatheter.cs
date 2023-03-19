using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CorkConCatheter : MonoBehaviour
{
    private GameObject Text_Make_O2_Step4;

    Score Match_ReFire_Score = new Score();
    // Start is called before the first frame update
    void Start()
    {
        Text_Make_O2_Step4 = GameObject.Find("Make_O2_Step4");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "CatheterTube")
        {
            if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step3())
            {
                Text_Make_O2_Step4 = GameObject.Find("Make_O2_Step4");
                Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                Match_ReFire_Score.Make_O2_Step3_To_Step4();
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {

    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "CatheterTube")
        {
            Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
            Match_ReFire_Score.No_Make_O2_Step4();
        }
    }
}
