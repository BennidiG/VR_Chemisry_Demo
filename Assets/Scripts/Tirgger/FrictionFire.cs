using UnityEngine;

public class FrictionFire : MonoBehaviour
{
    private Rigidbody STP;
    private VisualEffectGraph A_Fire;

    void Start()
    {
        STP = GameObject.Find("Match").GetComponent<Rigidbody>();
        A_Fire = GameObject.Find("Match").GetComponent<VisualEffectGraph>();
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "CubeColl" && STP != null && A_Fire != null && STP.velocity.magnitude > 0.5F)
            A_Fire.startfire();
    }
}
