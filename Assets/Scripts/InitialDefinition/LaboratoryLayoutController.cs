using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LaboratoryLayoutController : MonoBehaviour, ILaboratoryResettable
{
    private sealed class ItemSnapshot
    {
        public LabItem item;
        public LabItemType itemType;
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool active;
        public bool grabEnabled;
        public Rigidbody body;
        public bool wasKinematic;
        public bool useGravity;
        public float drag;
        public float angularDrag;
    }

    private readonly List<ItemSnapshot> snapshots = new List<ItemSnapshot>();

    private void Awake()
    {
        ExperimentRunner.LaboratoryResetHandler = this;
        foreach (LabItem item in Resources.FindObjectsOfTypeAll<LabItem>())
        {
            if (!item.gameObject.scene.IsValid())
                continue;
            snapshots.Add(new ItemSnapshot
            {
                item = item,
                itemType = item.itemType,
                parent = item.transform.parent,
                localPosition = item.transform.localPosition,
                localRotation = item.transform.localRotation,
                localScale = item.transform.localScale,
                active = item.gameObject.activeSelf,
                grabEnabled = item.GetComponent<XRGrabInteractable>() == null || item.GetComponent<XRGrabInteractable>().enabled,
                body = item.GetComponent<Rigidbody>(),
                wasKinematic = item.GetComponent<Rigidbody>() == null || item.GetComponent<Rigidbody>().isKinematic,
                useGravity = item.GetComponent<Rigidbody>() == null || item.GetComponent<Rigidbody>().useGravity,
                drag = item.GetComponent<Rigidbody>() == null ? 0F : item.GetComponent<Rigidbody>().drag,
                angularDrag = item.GetComponent<Rigidbody>() == null ? 0F : item.GetComponent<Rigidbody>().angularDrag
            });
        }
    }

    private void OnDestroy()
    {
        if (ExperimentRunner.LaboratoryResetHandler == this)
            ExperimentRunner.LaboratoryResetHandler = null;
    }

    public void ResetLaboratory()
    {
        MakeO2 makeO2 = FindObjectOfType<MakeO2>();
        if (makeO2 != null)
            makeO2.ResetForExperiment();
        GetPowder powder = FindObjectOfType<GetPowder>();
        if (powder != null)
            powder.ResetForExperiment();
        O2IntoBottle collector = FindObjectOfType<O2IntoBottle>();
        if (collector != null)
            collector.ResetForExperiment();
        VisualEffectGraph match = FindObjectOfType<VisualEffectGraph>();
        if (match != null)
            match.ResetForExperiment();

        foreach (ItemSnapshot snapshot in snapshots)
        {
            if (snapshot.item == null)
            {
                if (snapshot.itemType == LabItemType.Match)
                {
                    GameObject newMatch = GameObject.Find("Match");
                    if (newMatch != null)
                    {
                        newMatch.transform.SetParent(snapshot.parent, false);
                        newMatch.transform.localPosition = snapshot.localPosition;
                        newMatch.transform.localRotation = snapshot.localRotation;
                        newMatch.transform.localScale = snapshot.localScale;
                        newMatch.SetActive(snapshot.active);
                        Rigidbody body = newMatch.GetComponent<Rigidbody>();
                        if (body != null)
                        {
                            body.velocity = Vector3.zero;
                            body.angularVelocity = Vector3.zero;
                        }
                    }
                }
                continue;
            }

            snapshot.item.SetHighlight(null);
            XRGrabInteractable grab = snapshot.item.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;

            snapshot.item.transform.SetParent(snapshot.parent, false);
            snapshot.item.transform.localPosition = snapshot.localPosition;
            snapshot.item.transform.localRotation = snapshot.localRotation;
            snapshot.item.transform.localScale = snapshot.localScale;
            snapshot.item.gameObject.SetActive(snapshot.active);

            if (snapshot.body != null)
            {
                snapshot.body.velocity = Vector3.zero;
                snapshot.body.angularVelocity = Vector3.zero;
                snapshot.body.isKinematic = snapshot.wasKinematic;
                snapshot.body.useGravity = snapshot.useGravity;
                snapshot.body.drag = snapshot.drag;
                snapshot.body.angularDrag = snapshot.angularDrag;
            }
            if (grab != null)
                grab.enabled = snapshot.grabEnabled;
        }

        Score score = FindObjectOfType<Score>();
        if (score != null)
            score.Nope_Mistake_AlcoholLamp();
    }
}
