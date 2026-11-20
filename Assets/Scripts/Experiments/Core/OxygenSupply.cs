using System;
using UnityEngine;

/// <summary>
/// Oxygen belongs to the collection bottle, not to a match instance.
/// Initialization is one-shot per experiment run, so repeated button callbacks
/// cannot refill the bottle without starting a new run.
/// </summary>
public class OxygenSupply : MonoBehaviour
{
    public static OxygenSupply Instance { get; private set; }

    public const float MaxOxygen = 10000F;
    public const float MaxAmountMol = 0.01F;
    public float Amount { get; private set; }
    public float AmountMol => Amount / ChemistryUnits.GasUnitsPerMole;
    public bool Initialized { get; private set; }
    public event Action<float> AmountChanged;

    private int initializedRunId = -1;

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
        int runId = ExperimentRunner.Instance != null ? ExperimentRunner.Instance.ExperimentRunId : 0;
        bool fillBottle = ExperimentRunner.Instance == null || ExperimentRunner.Instance.CurrentProject != 1;
        return InitializeForExperiment(runId, fillBottle);
    }

    public bool InitializeForExperiment(int runId, bool fillBottle)
    {
        if (initializedRunId == runId)
            return false;

        initializedRunId = runId;
        Amount = fillBottle ? MaxOxygen : 0F;
        Initialized = true;
        AmountChanged?.Invoke(Amount);
        return true;
    }

    public void AddOxygen(float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0F)
            return;
        Amount = Mathf.Min(MaxOxygen, Amount + amount);
        Initialized = true;
        AmountChanged?.Invoke(Amount);
    }

    public float ConsumeOxygen(float amount)
    {
        if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0F)
            return 0F;
        float previous = Amount;
        Amount = Mathf.Max(0F, Amount - Mathf.Max(0F, amount));
        AmountChanged?.Invoke(Amount);
        return previous - Amount;
    }

    public void AddOxygenMol(float amountMol)
    {
        if (amountMol > 0F)
            AddOxygen(amountMol * ChemistryUnits.GasUnitsPerMole);
    }

    public float ConsumeOxygenMol(float amountMol)
    {
        return ConsumeOxygen(amountMol * ChemistryUnits.GasUnitsPerMole) / ChemistryUnits.GasUnitsPerMole;
    }

    public void ResetSupply()
    {
        Amount = 0F;
        Initialized = false;
        initializedRunId = -1;
        AmountChanged?.Invoke(Amount);
    }
}
