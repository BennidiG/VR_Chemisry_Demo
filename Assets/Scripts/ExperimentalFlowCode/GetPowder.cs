using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GetPowder : MonoBehaviour
{
    private GameObject Powder;
    private GameObject Powder_Test;
    private GameObject Powder_Test1;
    private GameObject Powder_Test2;
    private GameObject Powder_Test3;
    private GameObject Powder_Test4;
    private GameObject Powder_Test5;
    private GameObject Powder_Test6;
    private GameObject Powder_Test7;
    private GameObject Powder_Test8;

    private GameObject Make_O2_Step1;
    private GameObject Make_O2_Step2;
    //private GameObject T2;

    private static bool Is_Powder;
    private static bool Is_Powder_Test;

    private static bool Is_Powder_Test1;
    private static bool Is_Powder_Test2;
    private static bool Is_Powder_Test3;
    private static bool Is_Powder_Test4;
    private static bool Is_Powder_Test5;
    private static bool Is_Powder_Test6;
    private static bool Is_Powder_Test7;
    private static bool Is_Powder_Test8;

    Score Match_ReFire_Score = new Score();

    //Hints Step = new Hints();
    // Start is called before the first frame update
    void Start()
    {
        Powder = GameObject.Find("Powder");
        Powder_Test1 = GameObject.Find("Powder_Test1");
        Powder_Test2 = GameObject.Find("Powder_Test2");
        Powder_Test3 = GameObject.Find("Powder_Test3");
        Powder_Test4 = GameObject.Find("Powder_Test4");
        Powder_Test5 = GameObject.Find("Powder_Test5");
        Powder_Test6 = GameObject.Find("Powder_Test6");
        Powder_Test7 = GameObject.Find("Powder_Test7");
        Powder_Test8 = GameObject.Find("Powder_Test8");

        Make_O2_Step1 = GameObject.Find("Make_O2_Step1");
        Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
        //T2 = GameObject.Find("Prompt");

        Powder.GetComponent<Renderer>().enabled = false;
        Powder_Test1.GetComponent<Renderer>().enabled = false;
        Powder_Test2.GetComponent<Renderer>().enabled = false;
        Powder_Test3.GetComponent<Renderer>().enabled = false;
        Powder_Test4.GetComponent<Renderer>().enabled = false;
        Powder_Test5.GetComponent<Renderer>().enabled = false;
        Powder_Test6.GetComponent<Renderer>().enabled = false;
        Powder_Test7.GetComponent<Renderer>().enabled = false;
        Powder_Test8.GetComponent<Renderer>().enabled = false;

        Is_Powder = false;
        Is_Powder_Test = false;
        Is_Powder_Test1 = false;
        Is_Powder_Test2 = false;
        Is_Powder_Test3 = false;
        Is_Powder_Test4 = false;
        Is_Powder_Test5 = false;
        Is_Powder_Test6 = false;
        Is_Powder_Test7 = false;
        Is_Powder_Test8 = false;
    }

    // Update is called once per frame
    void Update()
    {
        //T2.GetComponent<TMP_Text>().text = Match_ReFire_Score.Project_Num() + "  " + Match_ReFire_Score.If_Make_O2_Ready();
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Drug")
        {
            if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Ready())
            {
                Make_O2_Step1 = GameObject.Find("Make_O2_Step1");
                //Step.Make_O2_Step2();
                //Make_O2_Step1.GetComponent<TMP_Text>().text = "步骤二：将药勺的药品放到试管中";
                Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                Match_ReFire_Score.Yes_Make_O2_Step1();
            }
            Powder.GetComponent<Renderer>().enabled = true;
            Is_Powder = true;
        }

        if (collider.name == "Powder_TestColl1")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test1.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test1 = true;
            }
        }

        if (collider.name == "Powder_TestColl2")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test2.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test2 = true;
            }
        }

        if (collider.name == "Powder_TestColl3")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test3.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test3 = true;
            }
        }

        if (collider.name == "Powder_TestColl4")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test4.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test4 = true;
            }
        }

        if (collider.name == "Powder_TestColl5")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test5.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test5 = true;
            }
        }

        if (collider.name == "Powder_TestColl6")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test6.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test6 = true;
            }
        }

        if (collider.name == "Powder_TestColl7")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test7.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test7 = true;
            }
        }

        if (collider.name == "Powder_TestColl8")
        {
            if (Is_Powder == true)
            {
                if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step1())
                {
                    Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
                    //Step.Make_O2_Step3();
                    //Make_O2_Step2.GetComponent<TMP_Text>().text = "步骤三：将试管和塞子连接";
                    Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                    Match_ReFire_Score.Yes_Make_O2_Step2();
                }
                Powder.GetComponent<Renderer>().enabled = false;
                Is_Powder = false;

                Powder_Test8.GetComponent<Renderer>().enabled = true;
                Is_Powder_Test = true;
                Is_Powder_Test8 = true;
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "Drug")
        {
            Powder.GetComponent<Renderer>().enabled = true;
            Is_Powder = true;
        }
    }

    public void OnTriggerExit(Collider collider)
    {

    }

    public bool Can_Burn()
    {
        return Is_Powder_Test;
    }

    public bool IF_Powder_Test1()
    {
        return Is_Powder_Test1;
    }

    public bool IF_Powder_Test2()
    {
        return Is_Powder_Test2;
    }

    public bool IF_Powder_Test3()
    {
        return Is_Powder_Test3;
    }

    public bool IF_Powder_Test4()
    {
        return Is_Powder_Test4;
    }

    public bool IF_Powder_Test5()
    {
        return Is_Powder_Test5;
    }

    public bool IF_Powder_Test6()
    {
        return Is_Powder_Test6;
    }

    public bool IF_Powder_Test7()
    {
        return Is_Powder_Test7;
    }

    public bool IF_Powder_Test8()
    {
        return Is_Powder_Test8;
    }

}
