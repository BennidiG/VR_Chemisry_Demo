using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CreateMatch : MonoBehaviour
{
    public GameObject Match;

    private static bool IF_CreateM;
    private static bool Have_M;
    private XRSocketInteractor[] matchSockets = new XRSocketInteractor[0];
    private int lastResetFrame = -1;
    private ExperimentRunner runner;
    private Transform initialParent;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;
    private bool hasInitialTransform;

    void Start()
    {
        IF_CreateM = false;
        Have_M = true;
        CacheMatchSockets();
        runner = ExperimentRunner.GetOrCreate();
        runner.ExperimentReset += OnExperimentReset;

        GameObject initialMatch = GameObject.Find("Match");
        if (initialMatch != null)
        {
            initialParent = initialMatch.transform.parent;
            initialPosition = initialMatch.transform.localPosition;
            initialRotation = initialMatch.transform.localRotation;
            initialScale = initialMatch.transform.localScale;
            hasInitialTransform = true;
        }
    }

    private void CacheMatchSockets()
    {
        List<XRSocketInteractor> sockets = new List<XRSocketInteractor>();
        XRSocketInteractor[] allSockets = Resources.FindObjectsOfTypeAll<XRSocketInteractor>();
        foreach (XRSocketInteractor socket in allSockets)
        {
            if (!socket.gameObject.scene.IsValid() || socket.startingSelectedInteractable == null)
                continue;

            // Cache references while the original scene Match still exists.
            if (socket.startingSelectedInteractable.gameObject.name == "Match")
                sockets.Add(socket);
        }
        matchSockets = sockets.ToArray();
    }

    private void OnDestroy()
    {
        if (runner != null)
            runner.ExperimentReset -= OnExperimentReset;
    }

    private void OnExperimentReset()
    {
        ResetMatch();
    }

    public void Creat_New_Match()
    {
        if (Match == null)
        {
            Debug.LogError("Match prefab is not assigned.");
            return;
        }

        if (Have_M)
            return;

        GameObject Match_Copy = Instantiate(Match, new Vector3(2.77F, 1.024F, 5.87F), Quaternion.identity);
        Match_Copy.transform.localScale = new Vector3(0.005F, 0.15F, 0.005F);
        Match_Copy.transform.Rotate(new Vector3(90, 0, -90));
        Match_Copy.name = "Match";

        // Do not force a socket selection: the original design spawns the new
        // match on the equipment table for the player to pick up and assemble.
        XRGrabInteractable grabInteractable = Match_Copy.GetComponent<XRGrabInteractable>();
        foreach (XRSocketInteractor socket in matchSockets)
        {
            if (socket != null)
                socket.startingSelectedInteractable = grabInteractable;
        }

        IF_CreateM = true;
        Have_M = true;
        RefreshMatchRelay();
    }

    public void ResetMatch()
    {
        if (lastResetFrame == Time.frameCount && Have_M && GameObject.Find("Match") != null)
            return;
        lastResetFrame = Time.frameCount;

        GameObject current = GameObject.Find("Match");
        if (current != null)
            Destroy(current);
        Clear_Old_Match();

        GameObject matchCopy;
        if (hasInitialTransform)
        {
            matchCopy = Instantiate(Match);
            matchCopy.transform.SetParent(initialParent, false);
            matchCopy.transform.localPosition = initialPosition;
            matchCopy.transform.localRotation = initialRotation;
            matchCopy.transform.localScale = initialScale;
        }
        else
        {
            Creat_New_Match();
            return;
        }

        matchCopy.name = "Match";
        XRGrabInteractable grabInteractable = matchCopy.GetComponent<XRGrabInteractable>();
        foreach (XRSocketInteractor socket in matchSockets)
        {
            if (socket != null)
                socket.startingSelectedInteractable = grabInteractable;
        }
        IF_CreateM = true;
        Have_M = true;
        RefreshMatchRelay();
        StartCoroutine(SelectMatchAfterReset(matchCopy));
    }

    private void RefreshMatchRelay()
    {
        MatchActionRelay relay = FindObjectOfType<MatchActionRelay>();
        if (relay != null)
            relay.ResetCurrentMatch();
    }

    private IEnumerator SelectMatchAfterReset(GameObject match)
    {
        yield return null;
        if (match == null)
            yield break;

        XRGrabInteractable grab = match.GetComponent<XRGrabInteractable>();
        if (grab == null)
            yield break;

        foreach (XRSocketInteractor socket in matchSockets)
        {
            if (socket == null)
                continue;
            XRInteractionManager manager = socket.interactionManager;
            if (manager != null && socket.CanSelect(grab))
            {
                manager.SelectEnter(socket, grab);
                break;
            }
        }
    }
    public bool Have_Match()
    {
        return Have_M;
    }

    public bool Have_Created_Match()
    {
        return IF_CreateM;
    }

    public void Clear_Old_Match()
    {
        IF_CreateM = false;
        Have_M = false;
    }

    public void Have_New_Match()
    {
        IF_CreateM = false;
        Have_M = true;
    }
}
