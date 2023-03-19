using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Hints : MonoBehaviour
{
    private GameObject Text_Make_O2_Step1;
    private GameObject Text_Make_O2_Step2;
    private GameObject Text_Make_O2_Step3;
    private GameObject Text_Make_O2_Step4;
    private GameObject Text_Make_O2_Step5;
    private GameObject Text_Make_O2_Step6;
    private GameObject Text_Make_O2_Step7;
    private GameObject Text_Make_O2_Finish;

    private GameObject Text_Match_ReFire_Step1;
    private GameObject Text_Match_ReFire_Step2;
    private GameObject Text_Match_ReFire_Step3;
    private GameObject Text_Match_ReFire_Finish;

    Score Match_ReFire_Score = new Score();
    // Start is called before the first frame update
    void Start()
    {
        Text_Make_O2_Step1 = GameObject.Find("Make_O2_Step1");
        Text_Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
        Text_Make_O2_Step3 = GameObject.Find("Make_O2_Step3");
        Text_Make_O2_Step4 = GameObject.Find("Make_O2_Step4");
        Text_Make_O2_Step5 = GameObject.Find("Make_O2_Step5");
        Text_Make_O2_Step6 = GameObject.Find("Make_O2_Step6");
        Text_Make_O2_Step7 = GameObject.Find("Make_O2_Step7");
        Text_Make_O2_Finish = GameObject.Find("Make_O2_Step7");

        Text_Match_ReFire_Step1 = GameObject.Find("Match_ReFire_Step1");
        Text_Match_ReFire_Step2 = GameObject.Find("Match_ReFire_Step2");
        Text_Match_ReFire_Step3 = GameObject.Find("Match_ReFire_Step3");
        Text_Match_ReFire_Finish = GameObject.Find("Match_ReFire_Finish");
    }

    // Update is called once per frame
    void Update()
    {
        Make_O2_Finish();
        Match_ReFire_Finish();
    }

    public void Make_O2_Check()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "检查器材准备, 用右手柄射线确认实验器材: 勺子 药瓶 氧气瓶 酒精灯 试管 试管夹 水盆 导管";
    }

    public void Make_O2_Step1()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "步骤一: 取少量药品到于药勺中";
        Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Make_O2_Step2()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "步骤二: 将药勺的药品放到试管中";
        Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Make_O2_Step3()
    {
        if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step2())
        {
            //Text_Hints.GetComponent<TMP_Text>().text = "步骤三: 将试管和塞子连接";
            Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
        }
    }
    public void Make_O2_Step3_Black()
    {
        Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step4()
    {
        if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step3())
        {
            //Text_Hints.GetComponent<TMP_Text>().text = "步骤四: 将试管口和导管链接";
            Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
        }
    }
    public void Make_O2_Step4_Black()
    {
        Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step5()
    {
        if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step4())
        {
            //Text_Hints.GetComponent<TMP_Text>().text = "步骤五: 将装配好的试管固定在架子上";
            Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
        }
    }
    public void Make_O2_Step5_Black()
    {
        Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step6()
    {
        if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step5())
        {
            //Text_Hints.GetComponent<TMP_Text>().text = "步骤六: 在试管底位置处上酒精灯 并用火柴点燃";
            Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
        }
    }
    public void Make_O2_Step6_Black()
    {
        Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step7()
    {
        if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step6())
        {
            //Text_Hints.GetComponent<TMP_Text>().text = "步骤七: 将空瓶子放在导管出口获取氧气";
            Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
        }
    }

    public void Make_O2_Finish()
    {
        if(Match_ReFire_Score.IF_Make_O2_Step8())
            Text_Make_O2_Finish.gameObject.SetActive(true);
        //Text_Hints.GetComponent<TMP_Text>().text = "完成制作氧气实验\t请将器材放回原位";
    }










    public void Match_ReFire_Check()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "检查器材准备, 用右手柄射线确认实验器材: 火柴 氧气瓶 酒精灯";
    }

    public void Match_ReFire_Step1()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "步骤一:点燃火柴和酒精灯";
        Text_Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Match_ReFire_Step2()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "步骤二:用力摇晃火柴,使火柴熄灭";
        Text_Match_ReFire_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Match_ReFire_Step3()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "步骤三:在5秒内将带火星的火柴放到氧气瓶中";
        Text_Match_ReFire_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Match_ReFire_Finish()
    {
        if (Match_ReFire_Score.If_Match_ReFire_Step4())
            Text_Match_ReFire_Finish.gameObject.SetActive(true);
    }
}