using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class InTrigger : MonoBehaviour
{
    VisualEffect Fire;
    VisualEffect ReFire;
    VisualEffect Smoke;
    VisualEffect LampFire;

    private Rigidbody STP;

    private GameObject Match_ReFire_Step1;
    //private GameObject Text_Hints;

    VisualEffectGraph A_Fire = new VisualEffectGraph();
    Score Match_ReFire_Score = new Score();
    MakeO2 IfLampIsFire = new MakeO2();

    // Start is called before the first frame update
    void Start()
    {
        Fire = GameObject.Find("Fire").GetComponent<VisualEffect>();
        ReFire = GameObject.Find("ReFire").GetComponent<VisualEffect>();
        Smoke = GameObject.Find("Smoke").GetComponent<VisualEffect>();
        STP = GameObject.Find("Match").GetComponent<Rigidbody>();

        A_Fire = GameObject.Find("Match").GetComponent<VisualEffectGraph>();

        Match_ReFire_Step1 = GameObject.Find("Match_ReFire_Step1");
        LampFire = GameObject.Find("LampFire").GetComponent<VisualEffect>();
        LampFire.Stop();
        //Text_Hints = GameObject.Find("Hints");
        Fire.Stop();
        ReFire.Stop();
        Smoke.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        Match_ReFire_Step1 = GameObject.Find("Match_ReFire_Step1");
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "LampFireTrigger")
        {
            if (A_Fire.IfFire() && !IfLampIsFire.IfLampIsFire())
            {
                LampFire.Play();
                IfLampIsFire.LampIsFire();
                if (Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Ready())
                {
                    Match_ReFire_Score.Yes_Match_ReFire_Step1();
                    Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                }
            }
        }

        if (collider.name == "LampFireTrigger")
        {
            if (!A_Fire.IfFire() && IfLampIsFire.IfLampIsFire())
                A_Fire.startfire();
        }

        if (collider.name == "O2")
        {
            A_Fire.refire();
        }


        if (collider.name == "Plane")
        {
            //A_Fire.stopfire();
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "CubeColl" && STP.velocity.magnitude > 1)
        {
            A_Fire.startfire();
        }

        if (collider.name == "LampFireTrigger")
        {
            if (A_Fire.IfFire() && !IfLampIsFire.IfLampIsFire())
            {
                LampFire.Play();
                IfLampIsFire.LampIsFire();
                if (Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Ready())
                {
                    Match_ReFire_Score.Yes_Match_ReFire_Step1();
                    Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                }
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "O2")
        {
            if (A_Fire.C_refire())
            {
                A_Fire.exit_O2();
            }
        }
    }
}
