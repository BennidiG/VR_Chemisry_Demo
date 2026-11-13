using UnityEngine;
using UnityEngine.VFX;

public class LampTrigger : MonoBehaviour
{
    private VisualEffect LampFire;
    private MakeO2 lampState;

    void Start()
    {
        lampState = FindObjectOfType<MakeO2>();
        LampFire = GameObject.Find("LampFire").GetComponent<VisualEffect>();
        LampFire.Stop();
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "LampFireTrigger")
            ExtinguishLamp();
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "LampFireTrigger")
            ExtinguishLamp();
    }

    private void ExtinguishLamp()
    {
        LampFire.Stop();
        if (lampState != null)
            lampState.LampIsNotFire();
    }
}
