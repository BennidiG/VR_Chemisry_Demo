using System;
using UnityEngine;

/// <summary>
/// Oxygen belongs to the collection bottle, not to a match instance.
/// Initialization is one-shot per experiment, so menu clicks cannot refill it.
/// </summary>
public class OxygenSupply : MonoBehaviour
{
    public static OxygenSupply Instance { get; private set; }

    public const float MaxOxygen = 10000F;
    public float Amount { get; private set; }
    public bool Initialized { get; private set; }
    public event Action<float> AmountChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool InitializeForExperiment()
    {
        if (Initialized)
            return false;
        Amount = MaxOxygen;
        Initialized = true;
        AmountChanged?.Invoke(Amount);
        return true;
    }

    public void AddOxygen(float amount)
    {
        if (amount <= 0F)
            return;
        Amount = Mathf.Min(MaxOxygen, Amount + amount);
        Initialized = true;
        AmountChanged?.Invoke(Amount);
    }

    public float ConsumeOxygen(float amount)
    {
        float previous = Amount;
        Amount = Mathf.Max(0F, Amount - Mathf.Max(0F, amount));
        AmountChanged?.Invoke(Amount);
        return previous - Amount;
    }

    public void ResetSupply()
    {
        Amount = 0F;
        Initialized = false;
        AmountChanged?.Invoke(Amount);
    }
}
