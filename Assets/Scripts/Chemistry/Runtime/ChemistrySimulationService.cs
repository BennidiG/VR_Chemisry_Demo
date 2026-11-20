using UnityEngine;

public class ChemistrySimulationService : MonoBehaviour
{
    public static ChemistrySimulationService Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        ChemistryDatabase.EnsureLoaded();
        foreach (string error in ChemistryDatabase.LoadErrors)
            Debug.LogError("Chemistry definition: " + error);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static ChemistrySimulationService GetOrCreate()
    {
        if (Instance != null)
            return Instance;
        ChemistrySimulationService existing = FindObjectOfType<ChemistrySimulationService>();
        if (existing != null)
        {
            Instance = existing;
            return Instance;
        }

        GameObject serviceObject = new GameObject("ChemistrySimulationService");
        Instance = serviceObject.AddComponent<ChemistrySimulationService>();
        return Instance;
    }

    public ReactionResult Evaluate(string reactionId, ReactionContext context)
    {
        ReactionDefinition definition = ChemistryDatabase.GetReaction(reactionId);
        if (definition == null)
            return ReactionResult.Failure("Unknown reaction: " + reactionId);
        ReactionResult result = ReactionEngine.Evaluate(definition, context);
        if (result.Succeeded)
            PopulateMassBalance(result);
        return result;
    }

    public ReactionResult Evaluate(ReactionDefinition definition, ReactionContext context)
    {
        ReactionResult result = ReactionEngine.Evaluate(definition, context);
        if (result.Succeeded)
            PopulateMassBalance(result);
        return result;
    }

    public ReactionProcess CreateProcess(string reactionId, ReactionContext context)
    {
        ReactionDefinition definition = ChemistryDatabase.GetReaction(reactionId);
        return definition == null ? null : new ReactionProcess(definition, context);
    }

    public ReactionProcess StartProcess(string reactionId, ReactionContext context, out ReactionResult initialResult)
    {
        ReactionProcess process = CreateProcess(reactionId, context);
        if (process == null)
        {
            initialResult = ReactionResult.Failure("Unknown reaction: " + reactionId);
            return null;
        }

        initialResult = process.Start();
        if (initialResult.Succeeded)
            PopulateMassBalance(initialResult);
        return process;
    }

    private static void PopulateMassBalance(ReactionResult result)
    {
        result.InputMass = CalculateMass(result.ConsumedReagents);
        result.OutputMass = CalculateMass(result.GasProduced) + CalculateMass(result.AqueousProduced) + CalculateMass(result.LiquidProduced) + CalculateMass(result.SolidProduced);
        result.MassBalanceError = result.OutputMass - result.InputMass;
    }

    private static float CalculateMass(System.Collections.Generic.Dictionary<string, float> amounts)
    {
        float mass = 0F;
        foreach (System.Collections.Generic.KeyValuePair<string, float> pair in amounts)
        {
            ChemicalSubstance substance = ChemistryDatabase.GetSubstance(pair.Key);
            if (substance == null)
            {
                // Keep the error visible to validation and tests rather than
                // silently hiding an unknown substance in the balance.
                return float.NaN;
            }
            mass += pair.Value * substance.MolarMass;
        }
        return mass;
    }

    public ChemicalSubstance GetSubstance(string substanceId)
    {
        return ChemistryDatabase.GetSubstance(substanceId);
    }
}
