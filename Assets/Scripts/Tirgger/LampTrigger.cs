using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class LampTrigger : MonoBehaviour
{
    VisualEffect LampFire;


    private GameObject Match_ReFire_Step1;

    VisualEffectGraph IfMatchFire = new VisualEffectGraph();
    Score Match_ReFire_Score = new Score();
    MakeO2 IfLampIsFire = new MakeO2();

    //VisualEffectGraph A_Fire = new VisualEffectGraph();

    // Start is called before the first frame update
    void Start()
    {
        Match_ReFire_Step1 = GameObject.Find("Match_ReFire_Step1");
        LampFire = GameObject.Find("LampFire").GetComponent<VisualEffect>();
        LampFire.Stop();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "LampFireTrigger")
        {
            LampFire.Stop();
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "LampFireTrigger")
        {
            LampFire.Stop();
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        /*        if (collider.name == "StickFireColl")
        {
            if (IfMatchFire.IfFire() && !IfLampIsFire.IfLampIsFire())
            {
                LampFire.Play();
                IfLampIsFire.LampIsFire();
                if (Match_ReFire_Score.Project_Num() == 2 && Match_ReFire_Score.If_Match_ReFire_Ready())
                {
                    Match_ReFire_Score.Yes_Match_ReFire_Step1();
                    Match_ReFire_Step1.GetComponent<TMP_Text>().color = new Color(0, 1, 0, 1);
                }
            }
        }*/
    }
}
