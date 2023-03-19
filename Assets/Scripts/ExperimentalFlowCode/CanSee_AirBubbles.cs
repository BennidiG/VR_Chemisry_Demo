using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanSee_AirBubbles : MonoBehaviour
{
    private static bool AirBubbles_Can_See;

    // Start is called before the first frame update
    void Start()
    {
        AirBubbles_Can_See = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Water")
        {
            AirBubbles_Can_See = true;
        }
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "Water")
        {
            AirBubbles_Can_See = true;
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name == "Water")
        {
            AirBubbles_Can_See = false;
        }
    }

    public bool Can_See_AB()
    {
        if (AirBubbles_Can_See == true)
            return true;
        return false;
    }
}
