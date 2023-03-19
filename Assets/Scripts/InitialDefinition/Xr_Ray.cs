using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Xr_Ray : MonoBehaviour
{
    public GameObject CubeCanvas;

    public GameObject Up;
    public GameObject Down;
    public GameObject Forward;
    public GameObject Back;

    private GameObject Match;
    private GameObject Spoon;
    private GameObject DrugBottle;
    private GameObject GlassBottle;
    private GameObject AlcoholLamp;
    private GameObject TestTube1;
    private GameObject TubeClamp;
    private GameObject Basin;
    private GameObject Catheter; 

    private static bool Hit_Match;
    private static bool Hit_Spoon;
    private static bool Hit_DrugBottle;
    private static bool Hit_GlassBottle;
    private static bool Hit_AlcoholLamp;
    private static bool Hit_TestTube;
    private static bool Hit_TubeClamp;
    private static bool Hit_Basin;
    private static bool Hit_Catheter;

    private static bool Prepare_Match_On_Position;
    private static bool Prepare_Spoon_On_Position;
    private static bool Prepare_DrugBottle_On_Position;
    private static bool Prepare_GlassBottle_On_Position;
    private static bool Prepare_AlcoholLamp_On_Position;
    private static bool Prepare_TestTube_On_Position;
    private static bool Prepare_TubeClamp_On_Position;
    private static bool Prepare_Basin_On_Position;
    private static bool Prepare_Catheter_On_Position;

    private static bool Make_O2_Ready;
    private static bool Match_ReFire_Ready;

    private float RotateSpeed = 10f;
    private float MoveSpeed = 0.5f;

    private static int Project;

    private GameObject Clip;
    private GameObject MetalFixed;

    private GameObject Hints;
    private GameObject Title_Hints;
    private GameObject Text_Hints;
    private GameObject Text_Make_O2_Steps;
    private GameObject Text_Match_ReFire_Steps;

    //StartPrepare Project = new StartPrepare();
    private void Start()
    {
        Match = GameObject.Find("Match");
        Spoon = GameObject.Find("Spoon");
        DrugBottle = GameObject.Find("DrugBottle");
        GlassBottle = GameObject.Find("GlassBottle");
        AlcoholLamp = GameObject.Find("AlcoholLamp");
        TestTube1 = GameObject.Find("TestTube1");
        TubeClamp = GameObject.Find("TubeClamp");
        Basin = GameObject.Find("Basin");
        Catheter = GameObject.Find("Catheter");

        Clip = GameObject.Find("Clip");
        MetalFixed = GameObject.Find("MetalFixed");

        Hints = GameObject.Find("Hints");
        Title_Hints = GameObject.Find("Hints/Title");
        Text_Hints = GameObject.Find("Hints/Content");
        Text_Make_O2_Steps = GameObject.Find("Make_O2_Steps");
        Text_Match_ReFire_Steps = GameObject.Find("Match_ReFire_Steps");
        

        Hit_Match = false;
        Hit_Spoon = false;
        Hit_DrugBottle = false;
        Hit_GlassBottle = false;
        Hit_AlcoholLamp = false;
        Hit_TestTube = false;
        Hit_TubeClamp = false;
        Hit_Basin = false;
        Hit_Catheter = false;

        Prepare_Match_On_Position = false;
        Prepare_Spoon_On_Position = false;
        Prepare_DrugBottle_On_Position = false;
        Prepare_GlassBottle_On_Position = false;
        Prepare_AlcoholLamp_On_Position = false;
        Prepare_TestTube_On_Position = false;
        Prepare_TubeClamp_On_Position = false;
        Prepare_Basin_On_Position = false;
        Prepare_Catheter_On_Position = false;

        Make_O2_Ready = false;
        Match_ReFire_Ready = false;

        Project = 0;

        //先隐藏
        /*CubeCanvas = GameObject.Find("CubeCanvas");
        Up = GameObject.Find("Up");
        Down = GameObject.Find("Down");
        Forward = GameObject.Find("Forward");
        Back = GameObject.Find("Back");*/

        //CubeCanvas.gameObject.SetActive(false);
        /*Up.gameObject.SetActive(false);
        Down.gameObject.SetActive(false);
        Forward.gameObject.SetActive(false);
        Back.gameObject.SetActive(false);*/
    }

    private void Update()
    {
        //Ray();
    }

    public void Yes_Prepare_Match_On_Position()
    {
        Prepare_Match_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_Match_On_Position()
    {
        Prepare_Match_On_Position = false;
    }

    public void Yes_Prepare_Spoon_On_Position()
    {
        Prepare_Spoon_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_Spoon_On_Position()
    {
        Prepare_Spoon_On_Position = false;
    }

    public void Yes_Prepare_DrugBottle_On_Position()
    {
        Prepare_DrugBottle_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_DrugBottle_On_Position()
    {
        Prepare_DrugBottle_On_Position = false;
    }

    public void Yes_Prepare_GlassBottle_On_Position()
    {
        Prepare_GlassBottle_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_GlassBottle_On_Position()
    {
        Prepare_GlassBottle_On_Position = false;
    }

    public void Yes_Prepare_AlcoholLamp_On_Position()
    {
        Prepare_AlcoholLamp_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_AlcoholLamp_On_Position()
    {
        Prepare_AlcoholLamp_On_Position = false;
    }

    public void Yes_Prepare_TestTube_On_Position()
    {
        Prepare_TestTube_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_TestTube_On_Position()
    {
        Prepare_TestTube_On_Position = false;
    }

    public void Yes_Prepare_TubeClamp_On_Position()
    {
        Prepare_TubeClamp_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_TubeClamp_On_Position()
    {
        Prepare_TubeClamp_On_Position = false;
    }

    public void Yes_Prepare_Basin_On_Position()
    {
        Prepare_Basin_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_Basin_On_Position()
    {
        Prepare_Basin_On_Position = false;
    }

    public void Yes_Prepare_Catheter_On_Position()
    {
        Prepare_Catheter_On_Position = true;
        Prepare_On_Position();
    }
    public void No_Prepare_Catheter_On_Position()
    {
        Prepare_Catheter_On_Position = false;
    }

    void Prepare_On_Position()
    {
        if (Project == 1)
        {
            Text_Hints.GetComponent<TMP_Text>().text = "    已确认实验器材: ";
            if (Prepare_Match_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "火柴 ";
            if (Prepare_Spoon_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "勺子 ";
            if (Prepare_DrugBottle_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "药瓶 ";
            if (Prepare_GlassBottle_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
            if (Prepare_AlcoholLamp_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";
            if (Prepare_TestTube_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管 ";
            if (Prepare_TubeClamp_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管夹 ";
            if (Prepare_Basin_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "水盆 ";
            if (Prepare_Catheter_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "导管 ";

            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "\n还缺少:";

            if (!Prepare_Match_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "火柴 ";
            if (!Prepare_Spoon_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "勺子 ";
            if (!Prepare_DrugBottle_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "药瓶 ";
            if (!Prepare_GlassBottle_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
            if (!Prepare_AlcoholLamp_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";
            if (!Prepare_TestTube_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管 ";
            if (!Prepare_TubeClamp_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管夹 ";
            if (!Prepare_Basin_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "水盆 ";
            if (!Prepare_Catheter_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "导管 ";
            if (Prepare_Spoon_On_Position && Prepare_DrugBottle_On_Position && Prepare_GlassBottle_On_Position && Prepare_AlcoholLamp_On_Position && Prepare_TestTube_On_Position && Prepare_TubeClamp_On_Position && Prepare_Basin_On_Position && Prepare_Catheter_On_Position)
            {
                Text_Hints.GetComponent<TMP_Text>().text = "    实验准备阶段结束, 进入实验";
                Invoke("Project1_Ready", 2F);
            }
        }

        if (Project == 2)
        {
            Text_Hints.GetComponent<TMP_Text>().text = "已确认实验器材: ";
            if (Prepare_Match_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "火柴 ";
            if (Prepare_GlassBottle_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
            if (Prepare_AlcoholLamp_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";

            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "\n还缺少:";

            if (!Prepare_Match_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "火柴 ";
            if (!Prepare_GlassBottle_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
            if (!Prepare_AlcoholLamp_On_Position)
                Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";

            if (Prepare_Match_On_Position && Prepare_GlassBottle_On_Position && Prepare_AlcoholLamp_On_Position)
            {
                Text_Hints.GetComponent<TMP_Text>().text = "    实验准备阶段结束, 进入实验";
                Invoke("Project2_Ready", 2F);
            }


        }
    }


    void Ray()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        bool isLookedAt = Physics.Raycast(ray, out hit, Mathf.Infinity);

        //if (isLookedAt)
        //    Text_Hints.GetComponent<TMP_Text>().text = hit.transform.name;

        if (isLookedAt && Project == 1)     //如果isLookedAt为真，表示有物体与射线相碰撞
        {
            //hit.transform.GetComponent<Renderer>().material.color = Color.red;  //获取被碰撞物体身上的材质球，将其颜色置为红色
            //hit.rigidbody.useGravity = true;   //将Use Gravity后面的勾勾上，使其具有重力
            /*if (hit.transform.name == "Cube")
                CubeCanvas.gameObject.SetActive(true);
            else if (hit.transform.name == "Up")
            {
                Clip.transform.Translate(Vector3.up * MoveSpeed * Time.deltaTime);
                MetalFixed.transform.Translate(Vector3.up * MoveSpeed * Time.deltaTime);
            }
            else if (hit.transform.name == "Down")
            {
                Clip.transform.Translate(Vector3.down * MoveSpeed * Time.deltaTime);
                MetalFixed.transform.Translate(Vector3.down * MoveSpeed * Time.deltaTime);
            }
            else if (hit.transform.name == "Forward")
            {
                Clip.transform.Rotate(Vector3.forward * RotateSpeed * Time.deltaTime);
            }
            else if (hit.transform.name == "Back")
            {
                Clip.transform.Rotate(Vector3.back * RotateSpeed * Time.deltaTime);
            }*/

            if (hit.transform.name == "SpoonSocket")
            {
                if (!Hit_Spoon)
                {
                    Hit_Spoon = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "DrugBottleSocket")
            {
                if (!Hit_DrugBottle)
                {
                    Hit_DrugBottle = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "GlassBottleSocket")
            {
                if (!Hit_GlassBottle)
                {
                    Hit_GlassBottle = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "AlcoholLampSocket")
            {
                if (!Hit_AlcoholLamp)
                {
                    Hit_AlcoholLamp = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "TestTube")
            {
                if (!Hit_TestTube)
                {
                    Hit_TestTube = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "TubeClamp")
            {
                if (!Hit_TubeClamp)
                {
                    Hit_TubeClamp = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "Basin")
            {
                if (!Hit_Basin)
                {
                    Hit_Basin = true;
                    Make_O2_ReadyStep();
                }
            }
            else if (hit.transform.name == "CatheterTube")
            {
                if (!Hit_Catheter)
                {
                    Hit_Catheter = true;
                    Make_O2_ReadyStep();
                }
            }
        }

        if (isLookedAt && Project == 2)     //如果isLookedAt为真，表示有物体与射线相碰撞
        {
            if (hit.transform.name == "MatchSocket")
            {
                if (!Hit_Match)
                {
                    Hit_Match = true;
                    Match_ReFire_ReadyStep();
                }
            }
            else if (hit.transform.name == "GlassBottleSocket")
            {
                if (!Hit_GlassBottle)
                {
                    Hit_GlassBottle = true;
                    Match_ReFire_ReadyStep();
                }
            }
            else if (hit.transform.name == "AlcoholLampSocket")
            {
                if (!Hit_AlcoholLamp)
                {
                    Hit_AlcoholLamp = true;
                    Match_ReFire_ReadyStep();
                }
            }
        }
    }

    public void Make_O2()
    {
        Hit_Spoon = false;
        Hit_DrugBottle = false;
        Hit_GlassBottle = false;
        Hit_AlcoholLamp = false;
        Hit_TestTube = false;
        Hit_TubeClamp = false;
        Hit_Basin = false;
        Hit_Catheter = false;

        Make_O2_Ready = false;

        //Step.Make_O2_Step1();
        Project = 1;
        //Text_Hints.GetComponent<TMP_Text>().text = "检查器材准备，用右手柄射线确认实验器材：药瓶、试管、导管、酒精灯、试管夹、水盆、氧气瓶、勺子";
    }

    public void Make_O2_ReFire()
    {
        Hit_Match = false;
        Hit_GlassBottle = false;
        Hit_AlcoholLamp = false;

        Match_ReFire_Ready = false;

        Project = 2;
        //Text_Hints.GetComponent<TMP_Text>().text = "检查器材准备，用右手柄射线确认实验器材：火柴、酒精灯、氧气瓶";
    }

    public void Make_O2_ReadyStep()
    {
        Text_Hints.GetComponent<TMP_Text>().text = "已确认实验器材: ";
        if (Hit_Spoon)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "勺子 ";
        if (Hit_DrugBottle)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "药瓶 ";
        if (Hit_GlassBottle)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
        if (Hit_AlcoholLamp)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";
        if (Hit_TestTube)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管 ";
        if (Hit_TubeClamp)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管夹 ";
        if (Hit_Basin)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "水盆 ";
        if (Hit_Catheter)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "导管 ";

        Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "\n还缺少:";

        if (!Hit_Spoon)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "勺子 ";
        if (!Hit_DrugBottle)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "药瓶 ";
        if (!Hit_GlassBottle)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
        if (!Hit_AlcoholLamp)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";
        if (!Hit_TestTube)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管 ";
        if (!Hit_TubeClamp)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "试管夹 ";
        if (!Hit_Basin)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "水盆 ";
        if (!Hit_Catheter)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "导管 ";

        if (Hit_Spoon && Hit_DrugBottle && Hit_GlassBottle && Hit_AlcoholLamp && Hit_TestTube && Hit_TubeClamp && Hit_Basin && Hit_Catheter)
        {
            Text_Hints.GetComponent<TMP_Text>().text = "    实验准备阶段结束, 进入实验";

            Match.transform.position = new Vector3(0.3F, 1.024F, 6.75F);
            Spoon.transform.position = new Vector3(0.474F, 1.05F, 6.8F);
            DrugBottle.transform.position = new Vector3(0.064F, 1.08F, 6.86F);
            GlassBottle.transform.position = new Vector3(1.67F, 1.067F, 6.66F);
            AlcoholLamp.transform.position = new Vector3(0.85F, 1.052F, 6.7F);
            TestTube1.transform.position = new Vector3(0.747F, 1.042F, 6.8F);
            TubeClamp.transform.position = new Vector3(1F, 1.024F, 6.756F);
            Basin.transform.position = new Vector3(1.3F, 1.06F, 6.685F);
            Catheter.transform.position = new Vector3(0.675F, 1.024F, 6.7F);

            Invoke("Project1_Ready", 2F);
        }
    }

    public void Match_ReFire_ReadyStep()
    {
        Text_Hints.GetComponent<TMP_Text>().text = "已确认实验器材: ";
        if (Hit_Match)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "火柴 ";
        if (Hit_GlassBottle)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
        if (Hit_AlcoholLamp)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";

        Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "\n还缺少:";

        if (!Hit_Match)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "火柴 ";
        if (!Hit_GlassBottle)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "氧气瓶 ";
        if (!Hit_AlcoholLamp)
            Text_Hints.GetComponent<TMP_Text>().text = Text_Hints.GetComponent<TMP_Text>().text + "酒精灯 ";

        if (Hit_Match && Hit_GlassBottle && Hit_AlcoholLamp)
        {
            Text_Hints.GetComponent<TMP_Text>().text = "    实验准备阶段结束, 进入实验";

            Match.transform.position = new Vector3(0.3F, 1.024F, 6.75F);
            GlassBottle.transform.position = new Vector3(1.3F, 1.06F, 6.7F);
            AlcoholLamp.transform.position = new Vector3(0.85F, 1.052F, 6.7F);

            Invoke("Project2_Ready", 2F);
        }
    }

    public void Project1_Ready()
    {
        Make_O2_Ready = true;
        Hints.gameObject.SetActive(false);
        Text_Make_O2_Steps.gameObject.SetActive(true);
    }

    public void Project2_Ready()
    {
        Match_ReFire_Ready = true;
        Hints.gameObject.SetActive(false);
        Text_Match_ReFire_Steps.gameObject.SetActive(true);
    }

    public bool If_Make_O2_Ready()
    {
        return Make_O2_Ready;
    }

    public bool If_Match_ReFire_Ready()
    {
        return Match_ReFire_Ready;
    }
}