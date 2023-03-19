using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MakeO2 : MonoBehaviour
{
    private GameObject T2;
    private GameObject T3;

    private ParticleSystem AirBubbles;

    private static float O2ParticleCount;
    private static bool Lamp_Is_Fire;
    private static bool Tube_On_Fire;
    private static bool Hot;


    O2IntoBottle If_O2Into_Bottle = new O2IntoBottle();
    CanSee_AirBubbles If_CanSee_AB = new CanSee_AirBubbles();
    GetPowder If_Get_Powder = new GetPowder();
    // Start is called before the first frame update
    void Start()
    {
        O2ParticleCount = 0;
        Lamp_Is_Fire = false;
        Tube_On_Fire = false;
        Hot = false;

        AirBubbles = GameObject.Find("Bubbles").GetComponent<ParticleSystem>();
        T2 = GameObject.Find("Prompt");
        T3 = GameObject.Find("ParticleCount");


        AirBubbles.GetComponent<Renderer>().enabled = false;
        AirBubbles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        See_AB();
        HotEnough();
    }

    public void LampIsFire()
    {
        Lamp_Is_Fire = true;
        //T2.GetComponent<Text>().text = "Lamp_Is_Fire";
        //Debug.Log("Lamp_Is_Fire");
    }

    public void LampIsNotFire()
    {
        Lamp_Is_Fire = false;
        //T2.GetComponent<Text>().text = "Lamp_Is_Not__Fire";
        //Debug.Log("Lamp_Is_Not__Fire");
    }

    public bool IfLampIsFire()
    {
        return Lamp_Is_Fire;
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "OnFire")
        {
            if (Lamp_Is_Fire == true)
            {
                Tube_On_Fire = true;
                T2.GetComponent<TMP_Text>().text = "Tube Is On Fire";
                Invoke("DelayStartAirBubbles", 5F);
            }
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "OnFire")
        {
            if (Lamp_Is_Fire == true)
            {
                Tube_On_Fire = true;
                T2.GetComponent<TMP_Text>().text = "Tube Is On Fire";
                Invoke("DelayStartAirBubbles", 5F);
            }
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "OnFire" || Lamp_Is_Fire == false) 
        {
            Tube_On_Fire = false;
            T2.GetComponent<TMP_Text>().text = "Tube Is Not On Fire";
            Invoke("DelayStopAirBubbles", 5F);
        }
    }

    public void DelayStartAirBubbles()
    {
        if (Tube_On_Fire == true)
        {
            AirBubbles.Play();
            Hot = true;
            if (If_Get_Powder.Can_Burn())
                T2.GetComponent<TMP_Text>().text = "Chemical reaction in progress";
        }
    }

    public void DelayStopAirBubbles()
    {
        if (Tube_On_Fire == false)
        {
            AirBubbles.Stop();
            Hot = false;
            T2.GetComponent<TMP_Text>().text = "Chemical reaction not in progress";
        }
    }

    public void HotEnough()
    {
        if (Hot == true && If_Get_Powder.Can_Burn())
        {
            If_O2Into_Bottle.MakeingO2True();
        }
        else
            If_O2Into_Bottle.MakeingO2False();
    }

    public void See_AB()
    {
        if (If_CanSee_AB.Can_See_AB())
            AirBubbles.GetComponent<Renderer>().enabled = true;
        else
            AirBubbles.GetComponent<Renderer>().enabled = false;
    }

    public void StartAirBubbles()
    {
        AirBubbles.Play();
    }

    public void StopAirBubbles()
    {
        AirBubbles.Stop();
    }
}
