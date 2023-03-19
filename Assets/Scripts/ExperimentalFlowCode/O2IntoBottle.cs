using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class O2IntoBottle : MonoBehaviour
{
    private static bool MakeingO2;

    private GameObject T2;
    private GameObject Make_O2_Step7;
    //private GameObject Text_Hints;

    VisualEffectGraph O2Count = new VisualEffectGraph();
    Score Match_ReFire_Score = new Score();
    // Start is called before the first frame update
    void Start()
    {
        MakeingO2 = false;
        T2 = GameObject.Find("Prompt");

        Make_O2_Step7 = GameObject.Find("Make_O2_Step7");
        //Text_Hints = GameObject.Find("Hints");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "O2IntoColl")
        {
            if (MakeingO2)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step6())
                {
                    Make_O2_Step7 = GameObject.Find("Make_O2_Step7");
                    //Text_Hints.GetComponent<TMP_Text>().text = "完成实验\n请将器材放回原位";
                    Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step7();
                }
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "O2IntoColl")
        {
            if (MakeingO2)
            {
                O2Count.AddO2Count();
                //T2.GetComponent<Text>().text = "O2 into bottle";
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "O2IntoColl")
        {

        }
    }

    public void MakeingO2True()
    {
        MakeingO2 = true;
    }

    public void MakeingO2False()
    {
        MakeingO2 = false;
    }
}
