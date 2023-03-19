using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StartPrepare : MonoBehaviour
{
    private GameObject MakeO2Socket;
    private GameObject MatchReFireSocket;

    private GameObject Hints;
    private GameObject Text_Hints;
    private GameObject Text_Make_O2_Steps;
    private GameObject Text_Match_ReFire_Steps;

    private GameObject Text_Make_O2_Step1;
    private GameObject Text_Make_O2_Step2;
    private GameObject Text_Make_O2_Step3;
    private GameObject Text_Make_O2_Step4;
    private GameObject Text_Make_O2_Step5;
    private GameObject Text_Make_O2_Step6;
    private GameObject Text_Make_O2_Step7;

    private GameObject Text_Match_ReFire_Step1;
    private GameObject Text_Match_ReFire_Step2;
    private GameObject Text_Match_ReFire_Step3;

    private static int Project;

    // Start is called before the first frame update
    void Start()
    {
        MakeO2Socket = GameObject.Find("MakeO2Socket");
        MatchReFireSocket = GameObject.Find("MatchReFireSocket");

        Hints = GameObject.Find("Hints");
        Text_Hints = GameObject.Find("Hints/Content");
        Text_Make_O2_Steps = GameObject.Find("Make_O2_Steps");
        Text_Match_ReFire_Steps = GameObject.Find("Match_ReFire_Steps");

        Text_Make_O2_Step1 = GameObject.Find("Make_O2_Step1");
        Text_Make_O2_Step2 = GameObject.Find("Make_O2_Step2");
        Text_Make_O2_Step3 = GameObject.Find("Make_O2_Step3");
        Text_Make_O2_Step4 = GameObject.Find("Make_O2_Step4");
        Text_Make_O2_Step5 = GameObject.Find("Make_O2_Step5");
        Text_Make_O2_Step6 = GameObject.Find("Make_O2_Step6");
        Text_Make_O2_Step7 = GameObject.Find("Make_O2_Step7");

        Text_Match_ReFire_Step1 = GameObject.Find("Match_ReFire_Step1");
        Text_Match_ReFire_Step2 = GameObject.Find("Match_ReFire_Step2");
        Text_Match_ReFire_Step3 = GameObject.Find("Match_ReFire_Step3");

        Project = 0;

        MakeO2Socket.gameObject.SetActive(false);
        MatchReFireSocket.gameObject.SetActive(false);

        Hints.gameObject.SetActive(false);
        Text_Make_O2_Steps.gameObject.SetActive(false);
        Text_Match_ReFire_Steps.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Make_O2()
    {
        //Step.Make_O2_Step1();

        Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);

        MakeO2Socket.gameObject.SetActive(true);
        MatchReFireSocket.gameObject.SetActive(false);

        Hints.gameObject.SetActive(true);
        Text_Make_O2_Steps.gameObject.SetActive(false);
        Text_Match_ReFire_Steps.gameObject.SetActive(false);


        Project = 1;
        Text_Hints.GetComponent<TMP_Text>().text = "    实验器材: 火柴 药瓶 试管 导管 酒精灯 试管夹 水盆 氧气瓶 勺子\n    将用到的实验器材移动到左边的桌子上";
    }

    public void Match_ReFire()
    {
        Text_Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Match_ReFire_Step2.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
        Text_Match_ReFire_Step3.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);

        MakeO2Socket.gameObject.SetActive(false);
        MatchReFireSocket.gameObject.SetActive(true);

        Hints.gameObject.SetActive(true);
        Text_Make_O2_Steps.gameObject.SetActive(false);
        Text_Match_ReFire_Steps.gameObject.SetActive(false);
        Project = 2;
        Text_Hints.GetComponent<TMP_Text>().text = "    实验器材: 火柴 酒精灯 氧气瓶\n    将用到的实验器材移动到左边的桌子上";
    }

    public int Project_Num()
    {
        return Project;
    }

    public void Make_O2_Step1_Green()
    {
        Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step1_Black()
    {
        Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step2_Green()
    {
        Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step2_Black()
    {
        Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step3_Green()
    {
        Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step3_Black()
    {
        Text_Make_O2_Step3.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step4_Green()
    {
        Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step4_Black()
    {
        Text_Make_O2_Step4.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step5_Green()
    {
        Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step5_Black()
    {
        Text_Make_O2_Step5.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step6_Green()
    {
        Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step6_Black()
    {
        Text_Make_O2_Step6.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Make_O2_Step7_Green()
    {
        Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Make_O2_Step7_Black()
    {
        Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }




    public void Match_ReFire_Step1_Green()
    {
        Text_Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Match_ReFire_Step1_Black()
    {
        Text_Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Match_ReFire_Step2_Green()
    {
        Text_Match_ReFire_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Match_ReFire_Step2_Black()
    {
        Text_Match_ReFire_Step2.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }

    public void Match_ReFire_Step3_Green()
    {
        Text_Match_ReFire_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }
    public void Match_ReFire_Step3_Black()
    {
        Text_Match_ReFire_Step3.GetComponent<TMP_Text>().color = new Color(0, 0, 0, 1);
    }
}
