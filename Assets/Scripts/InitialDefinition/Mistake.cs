using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Mistake : MonoBehaviour
{
    private GameObject T2;
    private GameObject AlcoholLamp;

    private Score Mistake_Score;
    // Start is called before the first frame update
    void Start()
    {
        Mistake_Score = FindObjectOfType<Score>();
        AlcoholLamp = GameObject.Find("AlcoholLamp");
        T2 = GameObject.Find("Prompt");
    }

    // Update is called once per frame
    void Update()
    {
        if (AlcoholLamp == null || Mistake_Score == null)
            return;

        if ((AlcoholLamp.transform.localEulerAngles.x > 50 && AlcoholLamp.transform.localEulerAngles.x < 310) || (AlcoholLamp.transform.localEulerAngles.z > 50 && AlcoholLamp.transform.localEulerAngles.z < 310))
        {
            Mistake_Score.Have_Mistake_AlcoholLamp();
            T2.GetComponent<TMP_Text>().text = "Too dangerous, AlcoholLamp rotation too big";
        }
        else
        {
            Mistake_Score.Nope_Mistake_AlcoholLamp();
        }
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "name")
        {
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "name")
        {
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "name")
        {
        }
    }
}
