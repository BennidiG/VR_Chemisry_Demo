using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TestActionController : MonoBehaviour
{
    private ActionBasedController controller;

    void Start()
    {
        controller = GetComponent<ActionBasedController>();
        bool isPressed = controller.selectAction.action.ReadValue<bool>();
        controller.selectAction.action.performed += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("—°‘ÒButton≥…π¶£°");
    }
}