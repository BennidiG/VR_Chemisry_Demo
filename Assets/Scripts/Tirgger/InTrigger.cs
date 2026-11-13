using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class InTrigger : MonoBehaviour
{
    private VisualEffect Fire;
    private VisualEffect ReFire;
    private VisualEffect Smoke;
    private VisualEffect LampFire;
    private Rigidbody STP;
    private GameObject Match_ReFire_Step1;

    private VisualEffectGraph A_Fire;
    private Score Match_ReFire_Score;
    private MakeO2 IfLampIsFire;
    private readonly HashSet<Collider> oxygenContacts = new HashSet<Collider>();

    void Start()
    {
        Match_ReFire_Score = FindObjectOfType<Score>();
        IfLampIsFire = FindObjectOfType<MakeO2>();

        GameObject match = SceneObjectFinder.Find("Match");
        Fire = transform.parent.Find("Fire").GetComponent<VisualEffect>();
        ReFire = transform.parent.Find("ReFire").GetComponent<VisualEffect>();
        Smoke = transform.parent.Find("Smoke").GetComponent<VisualEffect>();
        STP = match.GetComponent<Rigidbody>();
        A_Fire = match.GetComponent<VisualEffectGraph>();

        Match_ReFire_Step1 = SceneObjectFinder.Find("Match_ReFire_Step1");
        LampFire = SceneObjectFinder.Find("LampFire").GetComponent<VisualEffect>();
        LampFire.Stop();
        Fire.Stop();
        ReFire.Stop();
        Smoke.Stop();
    }

    public void OnTriggerEnter(Collider collider)
    {
        HandleLampTrigger(collider);
        AddOxygenContact(collider);
    }

    public void OnTriggerStay(Collider collider)
    {
        if (collider.name == "CubeColl" && STP != null && STP.velocity.magnitude > 1)
            A_Fire.startfire();

        HandleLampTrigger(collider);
        AddOxygenContact(collider);
    }

    public void OnTriggerExit(Collider collider)
    {
        if (collider.name != "O2")
            return;

        oxygenContacts.Remove(collider);
        if (oxygenContacts.Count == 0 && A_Fire.C_refire())
            A_Fire.exit_O2();
    }

    private void OnDisable()
    {
        oxygenContacts.Clear();
    }

    private void AddOxygenContact(Collider collider)
    {
        if (collider.name != "O2")
            return;

        // The O2 object has multiple colliders. Only the first contact starts
        // re-burning; the last exit is handled separately in OnTriggerExit.
        if (oxygenContacts.Add(collider) && oxygenContacts.Count == 1)
            A_Fire.refire();
    }

    private void HandleLampTrigger(Collider collider)
    {
        if (collider.name != "LampFireTrigger")
            return;

        if (A_Fire.IfFire())
        {
            if (!IfLampIsFire.IfLampIsFire())
            {
                LampFire.Play();
                IfLampIsFire.LampIsFire();
                CompleteFirstStepIfReady();
            }
        }
        else if (IfLampIsFire.IfLampIsFire())
        {
            A_Fire.startfire();
        }
    }

    private void CompleteFirstStepIfReady()
    {
        if (Match_ReFire_Score.Project_Num() != 2 || !Match_ReFire_Score.If_Match_ReFire_Ready())
            return;

        Match_ReFire_Score.Yes_Match_ReFire_Step1();
        if (Match_ReFire_Step1 != null)
            Match_ReFire_Step1.GetComponent<TMP_Text>().color = Color.green;
    }
}
