using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = System.Random;
using UnityEngine.VFX;
using TMPro;

public class VisualEffectGraph : MonoBehaviour
{
    VisualEffect Fire;
    VisualEffect ReFire;
    VisualEffect Smoke;

    VFXEventAttribute eventAttribute;
    private GameObject MatchHead;
    private GameObject Mars;
    private GameObject MHead;
    private GameObject stick;
    private GameObject StickTirgger;
    private GameObject DisslpateBody;
    private GameObject Match;
    private Collider Stick_ContainerCollider;


    MeshRenderer My_RStick;
    MeshRenderer My_RDisslpate;

    private GameObject T1;
    private GameObject T2;
    private GameObject T3;
    private GameObject Match_ReFire_Step2;
    private GameObject Match_ReFire_Step3;
    //private GameObject Text_Hints;

    private Rigidbody STP;

    private static int Can_refire;
    private static int Is_fire;
    private static int Is_refire;
    private static int Flag;
    private static bool DissAdd;
    private static bool DestroyDiss;

    private static float aliveParticleCount;
    private static float O2ParticleCount;
    private static float MatchLength;
    private float Y;
    private float Off;
    private static float Dissposition;


    CreateMatch IF_CreateMatch = new CreateMatch();
    Score Match_ReFire_Score = new Score();
    private void Awake()
    {
        //InvokeRepeating("Disslpate", 0, (float)2);
    }

    void Start()
    {
        Match = GameObject.Find("Match");
        MatchHead = GameObject.Find("MatchHead");
        Mars = GameObject.Find("Mars");
        MHead = GameObject.Find("MHead");
        stick = GameObject.Find("stick");
        StickTirgger = GameObject.Find("StickTirgger");
        STP = GameObject.Find("Match").GetComponent<Rigidbody>();
        My_RStick = GameObject.Find("stick").GetComponent<MeshRenderer>();
        DisslpateBody = GameObject.Find("DisslpateBody");
        My_RDisslpate = GameObject.Find("Disslpate").GetComponent<MeshRenderer>();
        Fire = GameObject.Find("Fire").GetComponent<VisualEffect>();
        ReFire = GameObject.Find("ReFire").GetComponent<VisualEffect>();
        Smoke = GameObject.Find("Smoke").GetComponent<VisualEffect>();
        Stick_ContainerCollider = GameObject.Find("stick").GetComponent<BoxCollider>();

        T1 = GameObject.Find("ResponseRatio");
        T2 = GameObject.Find("Prompt");
        T3 = GameObject.Find("ParticleCount");
        Match_ReFire_Step2 = GameObject.Find("Match_ReFire_Step2");
        Match_ReFire_Step3 = GameObject.Find("Match_ReFire_Step3");
        //Text_Hints = GameObject.Find("Hints");

        Can_refire = 0;
        Is_refire = 0;
        Is_fire = 0;
        Flag = 0;
        DissAdd = false;
        DestroyDiss = false;

        aliveParticleCount = 0;
        O2ParticleCount = 0;

        Y = 0.2F;
        Off = 0.500001F;
        Dissposition = 0.600001F;

        Fire.Stop();
        ReFire.Stop();
        Smoke.Stop();

        Mars.GetComponent<Renderer>().enabled = false;
        DisslpateBody.GetComponent<Renderer>().enabled = false;
        My_RDisslpate.GetComponent<Renderer>().enabled = false;
    }

    void Update()
    {
        if (IF_CreateMatch.Have_Created_Match())
        {
            IF_CreateMatch.Have_New_Match();

            Match = GameObject.Find("Match");
            MatchHead = GameObject.Find("MatchHead");
            Mars = GameObject.Find("Mars");
            MHead = GameObject.Find("MHead");
            stick = GameObject.Find("stick");
            StickTirgger = GameObject.Find("StickTirgger");
            STP = GameObject.Find("Match").GetComponent<Rigidbody>();
            My_RStick = GameObject.Find("stick").GetComponent<MeshRenderer>();
            DisslpateBody = GameObject.Find("DisslpateBody");
            My_RDisslpate = GameObject.Find("Disslpate").GetComponent<MeshRenderer>();
            Fire = GameObject.Find("Fire").GetComponent<VisualEffect>();
            ReFire = GameObject.Find("ReFire").GetComponent<VisualEffect>();
            Smoke = GameObject.Find("Smoke").GetComponent<VisualEffect>();
            Stick_ContainerCollider = GameObject.Find("stick").GetComponent<BoxCollider>();


            Can_refire = 0;
            Is_refire = 0;
            Is_fire = 0;
            Flag = 0;
            DissAdd = false;
            DestroyDiss = false;

            aliveParticleCount = 0;

            Y = 0.2F;
            Off = 0.500001F;
            Dissposition = 0.600001F;

            Fire.Stop();
            ReFire.Stop();
            Smoke.Stop();

            Mars.GetComponent<Renderer>().enabled = false;
            DisslpateBody.GetComponent<Renderer>().enabled = false;
            My_RDisslpate.GetComponent<Renderer>().enabled = false;
        }

        if (IF_CreateMatch.Have_Match())
        {
            Particle_Number();
            Short_Stick();
            DisslpateCount();
            if (DestroyDiss)
                DestroyDisslpate();
            OutSpeed();
        }
    }

    void Particle_Number()
    {
        Random rd = new Random();
        if (Flag == 1)
        {
            if (aliveParticleCount < 450)
                aliveParticleCount += rd.Next(-1, 4);
            else if (aliveParticleCount > 820)
                aliveParticleCount -= rd.Next(-1, 3);
            else if (aliveParticleCount < 790)
                aliveParticleCount += rd.Next(-1, 3);
            else
                aliveParticleCount += rd.Next(-2, 2);
        }
        else
        {
            if (aliveParticleCount > 5)
                aliveParticleCount -= rd.Next(-1, 3);
            else
                aliveParticleCount = 0;
        }

        if (Is_refire == 0)
            T1.GetComponent<TMP_Text>().text = "Response Ratio :" + (aliveParticleCount / 40).ToString().PadRight(13) + "\t%";
        else
        {
            T1.GetComponent<TMP_Text>().text = "Response Ratio :" + (aliveParticleCount / 10).ToString().PadRight(13) + "\t%";
            if (O2ParticleCount > 1)
                O2ParticleCount--;
            else if (O2ParticleCount == 1)
            {
                O2_Empty();
                Can_refire = 0;
                O2ParticleCount--;
                T2.GetComponent<TMP_Text>().text = "Run out O2 ,cannot re start fier";
            }
        }
        T3.GetComponent<TMP_Text>().text = "O2 Particle Count :" + O2ParticleCount.ToString();


        //T.GetComponent<Text>().text = "ParticleCount: " + (Fire.aliveParticleCount + ReFire.aliveParticleCount).ToString();
    }

    void Short_Stick()
    {
        //Y -= 0.0001F;
        if (aliveParticleCount > 600 && Off > 0.2)
        {
            {
                if (Is_refire == 0)
                    Off -= 0.0001F;
                else
                    Off -= 0.0002F;
                //My_RStick.material.SetFloat("_Offest", Off);
                //stick.transform.localScale = new Vector3(0.01F, Y, 0.01F);   //³¤¶È
                //Mars.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, new Vector3(0, Y, 0), step);
                //Match_ContainerCollider.transform.localScale = new Vector3(0.005F, MatchLength, 0.005F);
                Stick_ContainerCollider.transform.localScale = new Vector3(1, 2 * Off, 1);
                Mars.transform.localPosition = new Vector3(0, Off, 0);
                MHead.transform.localPosition = new Vector3(0, Off, 0);
                Fire.transform.localPosition = new Vector3(0, Off, 0);
                ReFire.transform.localPosition = new Vector3(0, Off, 0);
                Smoke.transform.localPosition = new Vector3(0, Off, 0);
                StickTirgger.transform.localPosition = new Vector3(0, Off+0.02F, 0);

                DisslpateBody.transform.localPosition = new Vector3(0, Off, 0);
                My_RDisslpate.transform.localPosition = new Vector3(0, Off, 0);
            }
        }
        if (Off <= 0.2)
        {
            stopfire();
            T2.GetComponent<TMP_Text>().text = "Match is too short";
        }
    }

    void DisslpateCount()
    {
        if (Is_fire == 1)
        {
            if (DissAdd)
            {
                Dissposition += 0.00005F;
                if (Dissposition > 0.6)
                    DissAdd = false;
            }
            else
            {
                Dissposition -= 0.00005F;
                if (Dissposition < 0.5)
                    DissAdd = true;
            }
            My_RDisslpate.material.SetFloat("Offest", Dissposition - 0.2F);
            My_RDisslpate.material.SetFloat("Disslpate", Dissposition);
        }
    }


    public void startfire()
    {
        Destroy(MatchHead);
        Fire.Play();
        ReFire.Stop();
        Smoke.Play();
        Mars.GetComponent<Renderer>().enabled = true;
        DisslpateBody.GetComponent<Renderer>().enabled = true;
        My_RDisslpate.GetComponent<Renderer>().enabled = true;

        T2.GetComponent<TMP_Text>().text = "Start Fire : O2 + C = CO2 ";
        Can_refire = 1;
        Is_fire = 1;
        Is_refire = 0;
        Flag = 1;
        //T.GetComponent<Text>().text = "Play ParticleCount: " + (Fire.aliveParticleCount + ReFire.aliveParticleCount).ToString();
        //Debug.Log("Start Fire" + "  " + "The number of particle: " + Fire.aliveParticleCount);

    }

    public void stopfire()
    {
        Fire.Stop();
        ReFire.Stop();
        //Smoke.Stop();
        Is_refire = 0;
        Is_fire = 0;
        Flag = 0;
        DisslpateBody.GetComponent<Renderer>().enabled = false;
        Invoke("SetDestroyDisslpate", 2F);
        Invoke("DelayStopFire", 5F);
        T2.GetComponent<TMP_Text>().text = "Stop Fire ,please put match in O2 bottle in 5s ";
        //T.GetComponent<Text>().text = "Stop ParticleCount: " + (Fire.aliveParticleCount + ReFire.aliveParticleCount).ToString();
        //Debug.Log("Stop Fire" + "  " + "The number of particle: " + Fire.aliveParticleCount);
    }

    public void refire()
    {
        if (Can_refire == 1)
        {
            if (Is_fire == 0)
            {
                if (Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Step2())
                {
                    Match_ReFire_Step3 = GameObject.Find("Match_ReFire_Step3");
                    Match_ReFire_Score.Yes_Match_ReFire_Step3();
                    Match_ReFire_Step3.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                }
            }

            Fire.Stop();
            ReFire.Play();
            ReFire.playRate = 2;
            Smoke.Play();
            Mars.GetComponent<Renderer>().enabled = true;
            DisslpateBody.GetComponent<Renderer>().enabled = true;
            My_RDisslpate.GetComponent<Renderer>().enabled = true;

            T2.GetComponent<TMP_Text>().text = "Restart Fire : O2 + C = CO2 ";
            Can_refire = 1;
            Is_fire = 1;
            Is_refire = 1;
            Flag = 1;
        }
        else
            T2.GetComponent<TMP_Text>().text = "Cannot restart fire";
        //T.GetComponent<Text>().text = "Re ParticleCount: " + (Fire.aliveParticleCount + ReFire.aliveParticleCount).ToString();
        //Debug.Log("ReFire" + "  " + "The number of particle: " + ReFire.aliveParticleCount);
    }

    public void exit_O2()
    {
        Fire.Play();
        ReFire.Stop();
        Smoke.Play();
        Can_refire = 1;
        Is_fire = 1;
        Is_refire = 0;
        Flag = 1;
        Mars.GetComponent<Renderer>().enabled = true;
        DisslpateBody.GetComponent<Renderer>().enabled = true;
        My_RDisslpate.GetComponent<Renderer>().enabled = true;

        T2.GetComponent<TMP_Text>().text = "Exit O2 ,fire become smaller";
        //T.GetComponent<Text>().text = "Re ParticleCount: " + (Fire.aliveParticleCount + ReFire.aliveParticleCount).ToString();
        //Debug.Log("ReFire" + "  " + "The number of particle: " + ReFire.aliveParticleCount);
    }

    public void DelayStopFire()
    {
        if (Flag == 0)
        {
            Smoke.Stop();
            Mars.GetComponent<Renderer>().enabled = false;
            T2.GetComponent<TMP_Text>().text = "Too late ,the fire cannot restart ";
            Can_refire = 0;
            Is_fire = 0;
            Is_refire = 0;
        }
    }

    public void SetDestroyDisslpate()
    {
        if (Flag == 0)
            DestroyDiss = true;
    }

    public void DestroyDisslpate()
    {
        if (Dissposition > 0.1)
        {
            Dissposition -= 0.001F;
            My_RDisslpate.material.SetFloat("Offest", Dissposition - 0.2F);
            My_RDisslpate.material.SetFloat("Disslpate", Dissposition);
        }
        else
        {
            My_RDisslpate.GetComponent<Renderer>().enabled = false;
            Dissposition = 0.600001F;
            //Debug.Log(Dissposition);
            DestroyDiss = false;
            DissAdd = false;
        }
    }

    public void OutSpeed()
    {
        if (Is_fire == 1)
        {
            if (STP.velocity.magnitude > 3.5)
            {
                stopfire();
                if (Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Step1())
                {
                    Match_ReFire_Step2 = GameObject.Find("Match_ReFire_Step2");
                    Match_ReFire_Score.Yes_Match_ReFire_Step2();
                    Match_ReFire_Step2.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                }
            }
        }
    }

    public void O2_Empty()
    {
        if (Flag == 1)
            startfire();
        else
            stopfire();
    }

    public bool C_refire()
    {
        if (Can_refire == 1)
            return true;
        return false;
    }

    public void AddO2Count()
    {
        O2ParticleCount++;
        T3.GetComponent<TMP_Text>().text = "Making O2: Particle Count :" + O2ParticleCount.ToString();
    }

    public void Start_ReFireStick()
    {
        O2ParticleCount=10000;
        T3.GetComponent<TMP_Text>().text = "Making O2: Particle Count :" + O2ParticleCount.ToString();
    }

    public bool IfFire()
    {
        if (Is_fire == 1)
            return true;
        return false;
    }
}