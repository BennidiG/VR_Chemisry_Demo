using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class ReadInput : MonoBehaviour
{
    // Assigned to the right-hand controller.
    public XRController controller = null;

    private InputDevice device;
    private TMP_Text prompt;
    private bool wasPressed;

    void Start()
    {
        if (controller != null)
            device = InputDevices.GetDeviceAtXRNode(controller.controllerNode);

        GameObject promptObject = SceneObjectFinder.Find("Prompt");
        if (promptObject != null)
            prompt = promptObject.GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (controller == null)
            return;

        // XR devices may not be connected yet when Start runs.
        if (!device.isValid)
            device = InputDevices.GetDeviceAtXRNode(controller.controllerNode);

        if (!device.TryGetFeatureValue(CommonUsages.primaryButton, out bool buttonValue))
            return;

        if (buttonValue && !wasPressed)
        {
            Debug.Log(" pressing a");
            if (prompt != null)
                prompt.text = " pressing a";
        }

        wasPressed = buttonValue;
    }
}
