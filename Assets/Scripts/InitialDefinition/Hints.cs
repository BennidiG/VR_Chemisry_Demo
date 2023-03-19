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
        //Text_Hints.GetComponent<TMP_Text>().text = "�������׼��, �����ֱ�����ȷ��ʵ������: ���� ҩƿ ����ƿ �ƾ��� �Թ� �Թܼ� ˮ�� ����";
    }

    public void Make_O2_Step1()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "����һ: ȡ����ҩƷ����ҩ����";
        Text_Make_O2_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Make_O2_Step2()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "�����: ��ҩ�׵�ҩƷ�ŵ��Թ���";
        Text_Make_O2_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Make_O2_Step3()
    {
        if (Match_ReFire_Score.Project_Num() == 1 && Match_ReFire_Score.If_Make_O2_Step2())
        {
            //Text_Hints.GetComponent<TMP_Text>().text = "������: ���Թܺ���������";
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
            //Text_Hints.GetComponent<TMP_Text>().text = "������: ���Թܿں͵�������";
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
            //Text_Hints.GetComponent<TMP_Text>().text = "������: ��װ��õ��Թ̶ܹ��ڼ�����";
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
            //Text_Hints.GetComponent<TMP_Text>().text = "������: ���Թܵ�λ�ô��Ͼƾ��� ���û���ȼ";
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
            //Text_Hints.GetComponent<TMP_Text>().text = "������: ����ƿ�ӷ��ڵ��ܳ��ڻ�ȡ����";
            Text_Make_O2_Step7.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
        }
    }

    public void Make_O2_Finish()
    {
        if(Match_ReFire_Score.IF_Make_O2_Step8())
            Text_Make_O2_Finish.gameObject.SetActive(true);
        //Text_Hints.GetComponent<TMP_Text>().text = "�����������ʵ��\t�뽫���ķŻ�ԭλ";
    }










    public void Match_ReFire_Check()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "�������׼��, �����ֱ�����ȷ��ʵ������: ��� ����ƿ �ƾ���";
    }

    public void Match_ReFire_Step1()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "����һ:��ȼ���;ƾ���";
        Text_Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Match_ReFire_Step2()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "�����:����ҡ�λ��,ʹ���Ϩ��";
        Text_Match_ReFire_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Match_ReFire_Step3()
    {
        //Text_Hints.GetComponent<TMP_Text>().text = "������:��5���ڽ������ǵĻ��ŵ�����ƿ��";
        Text_Match_ReFire_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
    }

    public void Match_ReFire_Finish()
    {
        if (Match_ReFire_Score.If_Match_ReFire_Step4())
            Text_Match_ReFire_Finish.gameObject.SetActive(true);
    }
}