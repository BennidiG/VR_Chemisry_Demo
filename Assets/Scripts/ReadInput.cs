using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.IO;
using UnityEngine.UI;

public class ReadInput : MonoBehaviour
{
    private GameObject T2;
    //Assigned to the RightHand Controller
    public XRController controller = null;
    InputDevice device;
    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controller.controllerNode);
        T2 = GameObject.Find("Prompt");
    }
    void Update()
    {
        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool buttonValue) && buttonValue)
        {
            Debug.Log(" pressing a");
            T2.GetComponent<Text>().text = " pressing a";
        }

    }
}
