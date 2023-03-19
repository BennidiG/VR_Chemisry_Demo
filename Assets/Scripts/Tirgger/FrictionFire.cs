using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class FrictionFire : MonoBehaviour
{
    private GameObject T2;
    private Rigidbody STP;

    VisualEffectGraph A_Fire = new VisualEffectGraph();
    // Start is called before the first frame update
    void Start()
    {
        STP = GameObject.Find("Match").GetComponent<Rigidbody>();
        T2 = GameObject.Find("Prompt");
    }

    // Update is called once per frame
    void Update()
    {
        T2.GetComponent<TMP_Text>().text = STP.velocity.magnitude.ToString();

    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Cube")
        {
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "Cube")
        {
            A_Fire.startfire();
            if (STP.velocity.magnitude > 0.5)
                A_Fire.startfire();
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "Cube")
        {
        }
    }
}
