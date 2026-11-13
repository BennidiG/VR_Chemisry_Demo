using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class TestActionController : MonoBehaviour
{
    private ActionBasedController controller;

    void Start()
    {
        controller = GetComponent<ActionBasedController>();
        if (controller != null && controller.selectAction.action != null)
            controller.selectAction.action.performed += Action_performed;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.selectAction.action != null)
            controller.selectAction.action.performed -= Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("选择Button成功！");
    }
}