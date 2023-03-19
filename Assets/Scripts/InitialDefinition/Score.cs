using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    private GameObject Text_Score;

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

    private float Make_O2_StepNum;
    private float Match_ReFire_StepNum;

    private static int Project;

    /*private GameObject Text_Make_O2_Step1;
    private GameObject Text_Make_O2_Step2;
    private GameObject Text_Make_O2_Step3;
    private GameObject Text_Make_O2_Step4;
    private GameObject Text_Make_O2_Step5;
    private GameObject Text_Make_O2_Step6;
    private GameObject Text_Make_O2_Step7;

    private GameObject Text_Match_ReFire_Step1;
    private GameObject Text_Match_ReFire_Step2;
    private GameObject Text_Match_ReFire_Step3;
    private GameObject Text_Match_ReFire_Step4;*/

    Xr_Ray Hit = new Xr_Ray();
    // Start is called before the first frame update
    void Start()
    {
        Text_Score = GameObject.Find("Score");

        Make_O2_Ready = false;
        Match_ReFire_Ready = false;

        Make_O2_Step1 = false;
        Make_O2_Step2 = false;
        Make_O2_Step3 = false;
        Make_O2_Step4 = false;
        Make_O2_Step5 = false;
        Make_O2_Step6 = false;
        Make_O2_Step7 = false;
        Make_O2_Finish = false;
        Spoon_On_Position = true;
        DrugBottle_On_Position = true;
        DrugBottleCap_On_Position = true;
        TestTube1_On_Position = true;
        TestTube2_On_Position = true;
        TestTube3_On_Position = true;
        TestTube4_On_Position = true;
        TestTube5_On_Position = true;
        TestTube6_On_Position = true;
        TestTube7_On_Position = true;
        TestTube8_On_Position = true;
        TestTube9_On_Position = true;
        TestTube10_On_Position = true;
        TestTube11_On_Position = true;
        TestTube12_On_Position = true;
        TubeClamp_On_Position = true;
        Basin_On_Position = true;
        Catheter_On_Position = true;
        Mistake_AlcoholLamp = true;


        Match_ReFire_Step1 = false;
        Match_ReFire_Step2 = false;
        Match_ReFire_Step3 = false;
        Match_ReFire_Finish = false;
        Match_Destory = false;
        AlcoholLamp_On_Position = true;
        AlcoholLampCap_On_Position = true;
        GlassBottle_On_Position = true;
        GlassBottleCap_On_Position = true;

        Make_O2_StepNum = 42;
        Match_ReFire_StepNum = 20;

        Project = 0;

        
        /*Text_Make_O2_Step1 = GameObject.Find("Make_O2_Step1");
        Text_Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
        Text_Make_O2_Step3 = GameObject.Find("Make_O2_Step3");
        Text_Make_O2_Step4 = GameObject.Find("Make_O2_Step4");
        Text_Make_O2_Step5 = GameObject.Find("Make_O2_Step5");
        Text_Make_O2_Step6 = GameObject.Find("Make_O2_Step6");
        Text_Make_O2_Step7 = GameObject.Find("Make_O2_Step7");

        Text_Match_ReFire_Step1 = GameObject.Find("Match_ReFire_Step1");
        Text_Match_ReFire_Step2 = GameObject.Find("Match_ReFire_Step2");
        Text_Match_ReFire_Step3 = GameObject.Find("Match_ReFire_Step3");
        Text_Match_ReFire_Step4 = GameObject.Find("Match_ReFire_Step4");*/
    }

    // Update is called once per frame
    void Update()
    {
        CalculateScore();
        If_Make_O2_Finish();
        If_Match_ReFire_Finish();
    }

    public bool If_Make_O2_Ready()
    {
        Make_O2_Ready = Hit.If_Make_O2_Ready();
        return Make_O2_Ready;
    }

    public bool If_Match_ReFire_Ready()
    {
        Match_ReFire_Ready = Hit.If_Match_ReFire_Ready();
        return Match_ReFire_Ready;
    }

    public void Yes_Make_O2_Step1()
    {
        Make_O2_Step1 = true;
        //Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step1()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step1 = false;
            //Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }

    public void Yes_Make_O2_Step2()
    {
        Make_O2_Step2 = true;
        //Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step2()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step2 = false;
            //Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }
    public void Make_O2_Step2_To_Step3()
    {
        if (Project == 1 && Make_O2_Step2)
            Make_O2_Step3 = true;
    }
    public void Yes_Make_O2_Step3()
    {
        Make_O2_Step3 = true;
        //Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step3()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step3 = false;
            //Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }
    public void Make_O2_Step3_To_Step4()
    {
        if (Project == 1 && Make_O2_Step3)
            Make_O2_Step4 = true;
    }
    public void Yes_Make_O2_Step4()
    {
        Make_O2_Step4 = true;
        //Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step4()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step4 = false;
            //Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }
    public void Make_O2_Step4_To_Step5()
    {
        if (Project == 1 && Make_O2_Step4)
            Make_O2_Step5 = true;
    }
    public void Yes_Make_O2_Step5()
    {
        Make_O2_Step5 = true;
        //Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step5()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step5 = false;
            //Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }
    public void Make_O2_Step5_To_Step6()
    {
        if (Project == 1 && Make_O2_Step5)
            Make_O2_Step6 = true;
    }
    public void Yes_Make_O2_Step6()
    {
        Make_O2_Step6 = true;
        //Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step6()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step6 = false;
            //Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }

    public void Yes_Make_O2_Step7()
    {
        Make_O2_Step7 = true;
        //Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void No_Make_O2_Step7()
    {
        if (!Make_O2_Finish)
        {
            Make_O2_Step7 = false;
            //Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        }
    }

    public void If_Make_O2_Finish()
    {
        if (Make_O2_Step1 && Make_O2_Step2 && Make_O2_Step3 && Make_O2_Step4 && Make_O2_Step5 && Make_O2_Step6 && Make_O2_Step7)
            Make_O2_Finish = true;
        else
            Make_O2_Finish = false;
    }

    //Position
    public void Yes_Spoon_On_Position()
    {
        Spoon_On_Position = true;
    }
    public void No_Spoon_On_Position()
    {
        Spoon_On_Position = false;
    }

    public void Yes_DrugBottle_On_Position()
    {
        DrugBottle_On_Position = true;
    }
    public void No_DrugBottle_On_Position()
    {
        DrugBottle_On_Position = false;
    }

    public void Yes_DrugBottleCap_On_Position()
    {
        DrugBottleCap_On_Position = true;
    }
    public void No_DrugBottleCap_On_Position()
    {
        DrugBottleCap_On_Position = false;
    }

    public void Yes_TestTube1_On_Position()
    {
        TestTube1_On_Position = true;
    }
    public void No_TestTube1_On_Position()
    {
        TestTube1_On_Position = false;
    }

    public void Yes_TestTube2_On_Position()
    {
        TestTube2_On_Position = true;
    }
    public void No_TestTube2_On_Position()
    {
        TestTube2_On_Position = false;
    }

    public void Yes_TestTube3_On_Position()
    {
        TestTube3_On_Position = true;
    }
    public void No_TestTube3_On_Position()
    {
        TestTube3_On_Position = false;
    }

    public void Yes_TestTube4_On_Position()
    {
        TestTube4_On_Position = true;
    }
    public void No_TestTube4_On_Position()
    {
        TestTube4_On_Position = false;
    }

    public void Yes_TestTube5_On_Position()
    {
        TestTube5_On_Position = true;
    }
    public void No_TestTube5_On_Position()
    {
        TestTube5_On_Position = false;
    }

    public void Yes_TestTube6_On_Position()
    {
        TestTube6_On_Position = true;
    }
    public void No_TestTube6_On_Position()
    {
        TestTube6_On_Position = false;
    }

    public void Yes_TestTube7_On_Position()
    {
        TestTube7_On_Position = true;
    }
    public void No_TestTube7_On_Position()
    {
        TestTube7_On_Position = false;
    }

    public void Yes_TestTube8_On_Position()
    {
        TestTube8_On_Position = true;
    }
    public void No_TestTube8_On_Position()
    {
        TestTube8_On_Position = false;
    }

    public void Yes_TestTube9_On_Position()
    {
        TestTube9_On_Position = true;
    }
    public void No_TestTube9_On_Position()
    {
        TestTube9_On_Position = false;
    }

    public void Yes_TestTube10_On_Position()
    {
        TestTube10_On_Position = true;
    }
    public void No_TestTube10_On_Position()
    {
        TestTube10_On_Position = false;
    }

    public void Yes_TestTube11_On_Position()
    {
        TestTube11_On_Position = true;
    }
    public void No_TestTube11_On_Position()
    {
        TestTube11_On_Position = false;
    }

    public void Yes_TestTube12_On_Position()
    {
        TestTube12_On_Position = true;
    }
    public void No_TestTube12_On_Position()
    {
        TestTube12_On_Position = false;
    }

    public bool AllTestTube_On_Position()
    {
        if (TestTube1_On_Position && TestTube2_On_Position && TestTube3_On_Position && TestTube4_On_Position && TestTube5_On_Position && TestTube6_On_Position && TestTube7_On_Position && TestTube8_On_Position && TestTube9_On_Position && TestTube10_On_Position && TestTube11_On_Position && TestTube12_On_Position)
            return true;
        return false;
    }

    public void Yes_TubeClamp_On_Position()
    {
        TubeClamp_On_Position = true;
    }
    public void No_TubeClamp_On_Position()
    {
        TubeClamp_On_Position = false;
    }

    public void Yes_Basin_On_Position()
    {
        Basin_On_Position = true;
    }
    public void No_Basin_On_Position()
    {
        Basin_On_Position = false;
    }

    public void Yes_Catheter_On_Position()
    {
        Catheter_On_Position = true;
    }
    public void No_Catheter_On_Position()
    {
        Catheter_On_Position = false;
    }

    public bool If_Make_O2_Step1()
    {
        return Make_O2_Step1;
    }
    public bool If_Make_O2_Step2()
    {
        return Make_O2_Step2;
    }
    public bool If_Make_O2_Step3()
    {
        return Make_O2_Step3;
    }
    public bool If_Make_O2_Step4()
    {
        return Make_O2_Step4;
    }
    public bool If_Make_O2_Step5()
    {
        return Make_O2_Step5;
    }
    public bool If_Make_O2_Step6()
    {
        return Make_O2_Step6;
    }
    public bool If_Make_O2_Step7()
    {
        return Make_O2_Step7;
    }
    public bool IF_Make_O2_Step8()
    {
        return Make_O2_Finish;
    }



    public void Yes_Match_ReFire_Step1()
    {
        Match_ReFire_Step1 = true;
    }
    public void No_Match_ReFire_Step1()
    {
        if (!Match_ReFire_Finish)
            Match_ReFire_Step1 = false;
    }

    public void Yes_Match_ReFire_Step2()
    {
        Match_ReFire_Step2 = true;
    }
    public void No_Match_ReFire_Step2()
    {
        if (!Match_ReFire_Finish)
            Match_ReFire_Step2 = false;
    }

    public void Yes_Match_ReFire_Step3()
    {
        Match_ReFire_Step3 = true;
    }
    public void No_Match_ReFire_Step3()
    {
        if (!Match_ReFire_Finish)
            Match_ReFire_Step3 = false;
    }

    public void If_Match_ReFire_Finish()
    {
        if (Match_ReFire_Step1 && Match_ReFire_Step2 && Match_ReFire_Step3)
            Match_ReFire_Finish = true;
        else
            Match_ReFire_Finish = false;
    }

    public void Yes_Match_Destory()
    {
        Match_Destory = true;
    }
    public void No_Match_Destory()
    {
        Match_Destory = false;
    }

    public void Yes_AlcoholLamp_On_Position()
    {
        AlcoholLamp_On_Position = true;
    }
    public void No_AlcoholLamp_On_Position()
    {
        AlcoholLamp_On_Position = false;
    }

    public void Yes_AlcoholLampCap_On_Position()
    {
        AlcoholLampCap_On_Position = true;
    }
    public void No_AlcoholLampCap_On_Position()
    {
        AlcoholLampCap_On_Position = false;
    }

    public void Yes_GlassBottle_On_Position()
    {
        GlassBottle_On_Position = true;
    }
    public void No_GlassBottle_On_Position()
    {
        GlassBottle_On_Position = false;
    }

    public void Yes_GlassBottleCap_On_Position()
    {
        GlassBottleCap_On_Position = true;
    }
    public void No_GlassBottleCap_On_Position()
    {
        GlassBottleCap_On_Position = false;
    }

    public bool If_Match_ReFire_Step1()
    {
        return Match_ReFire_Step1;
    }
    public bool If_Match_ReFire_Step2()
    {
        return Match_ReFire_Step2;
    }
    public bool If_Match_ReFire_Step3()
    {
        return Match_ReFire_Step3;
    }
    public bool If_Match_ReFire_Step4()
    {
        return Match_ReFire_Finish;
    }





    public void Nope_Mistake_AlcoholLamp()
    {
        Mistake_AlcoholLamp = true;
    }
    public void Have_Mistake_AlcoholLamp()
    {
        Mistake_AlcoholLamp = false;
    }





    public void Choose_Project1()
    {
        Project = 1;

        Make_O2_Ready = false;

        Make_O2_Step1 = false;
        Make_O2_Step2 = false;
        Make_O2_Step3 = false;
        Make_O2_Step4 = false;
        Make_O2_Step5 = false;
        Make_O2_Step6 = false;
        Make_O2_Step7 = false;
        Make_O2_Finish = false;
        Invoke("Project1", 0.1F);
    }
    public void Project1()
    {
        Project = 1;
    }
    public void Choose_Project2()
    {
        Project = 2;

        Match_ReFire_Ready = false;

        Match_ReFire_Step1 = false;
        Match_ReFire_Step2 = false;
        Match_ReFire_Step3 = false;
        Match_ReFire_Finish = false;
        /*Match_Destory = false;
        AlcoholLamp_On_Position = false;
        AlcoholLampCap_On_Position = false;
        GlassBottle_On_Position = false;
        GlassBottleCap_On_Position = false;*/
    }

    public int Project_Num()
    {
        return Project;
    }

    void CalculateScore()
    {
        float Make_O2_Score = 0;
        float Make_O2_Steps_Divide = 100 / Make_O2_StepNum;

        float Match_ReFire_Score = 0;
        float Match_ReFire_Steps_Divide = 100 / Match_ReFire_StepNum;

        if (Project == 1)
        {
            if (Make_O2_Step1)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;
            if (Make_O2_Step2)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;
            if (Make_O2_Step3)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;
            if (Make_O2_Step4)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;
            if (Make_O2_Step5)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;
            if (Make_O2_Step6)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;
            if (Make_O2_Step7)
                Make_O2_Score += 5 * Make_O2_Steps_Divide;

            if (Make_O2_Finish && AllTestTube_On_Position())
                Make_O2_Score += Make_O2_Steps_Divide;
            if (Make_O2_Finish && AllTestTube_On_Position()) 
                Make_O2_Score += Make_O2_Steps_Divide;
            if (Make_O2_Finish && DrugBottle_On_Position)
                Make_O2_Score += Make_O2_Steps_Divide;
            if (Make_O2_Finish && DrugBottleCap_On_Position)
                Make_O2_Score += Make_O2_Steps_Divide;
            if (Make_O2_Finish && TubeClamp_On_Position)
                Make_O2_Score += Make_O2_Steps_Divide;
            if (Make_O2_Finish && Basin_On_Position)
                Make_O2_Score += Make_O2_Steps_Divide;
            if (Make_O2_Finish && Catheter_On_Position)
                Make_O2_Score += Make_O2_Steps_Divide;
            Text_Score.GetComponent<TMP_Text>().text = "制作氧气实验得分: " + Make_O2_Score + " /100";
        }

        else if (Project == 2)
        {
            if (Match_ReFire_Step1)
                Match_ReFire_Score += 5 * Match_ReFire_Steps_Divide;
            if (Match_ReFire_Step2)
                Match_ReFire_Score += 5 * Match_ReFire_Steps_Divide;
            if (Match_ReFire_Step3)
                Match_ReFire_Score += 5 * Match_ReFire_Steps_Divide;
            /*if (Match_ReFire_Step5)
                Match_ReFire_Score += 2 * Match_ReFire_Steps_Divide;*/
            if (Match_ReFire_Finish && Match_Destory) 
                Match_ReFire_Score += Match_ReFire_Steps_Divide;
            if (Match_ReFire_Finish && AlcoholLamp_On_Position)
                Match_ReFire_Score += Match_ReFire_Steps_Divide;
            if (Match_ReFire_Finish && AlcoholLampCap_On_Position)
                Match_ReFire_Score += Match_ReFire_Steps_Divide;
            if (Match_ReFire_Finish && GlassBottle_On_Position)
                Match_ReFire_Score += Match_ReFire_Steps_Divide;
            if (Match_ReFire_Finish && GlassBottleCap_On_Position)
                Match_ReFire_Score += Match_ReFire_Steps_Divide;
            Text_Score.GetComponent<TMP_Text>().text = "火柴复燃实验得分: " + Match_ReFire_Score + " /100";
        }

    }
}
