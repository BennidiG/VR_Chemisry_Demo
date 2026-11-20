using UnityEngine;

/// <summary>
/// Keeps UI and persistent UnityEvents independent from the lifetime of a
/// particular Match instance. The current match is resolved again after the
/// previous one has been destroyed.
/// </summary>
public class MatchActionRelay : MonoBehaviour
{
    private VisualEffectGraph currentMatch;

    public void ResetCurrentMatch()
    {
        currentMatch = null;
    }

    private VisualEffectGraph CurrentMatch
    {
        get
        {
            if (currentMatch == null)
                currentMatch = FindObjectOfType<VisualEffectGraph>();
            return currentMatch;
        }
    }

    public void startfire()
    {
        VisualEffectGraph match = CurrentMatch;
        if (match != null)
            match.startfire();
    }

    public void stopfire()
    {
        VisualEffectGraph match = CurrentMatch;
        if (match != null)
            match.stopfire();
    }

    public void refire()
    {
        VisualEffectGraph match = CurrentMatch;
        if (match != null)
            match.refire();
    }

    public void Start_ReFireStick()
    {
        VisualEffectGraph match = CurrentMatch;
        if (match != null)
            match.Start_ReFireStick();
    }
}
