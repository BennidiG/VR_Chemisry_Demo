public static class ChemistryLegacyAdapter
{
    public const float GasUnitsPerMole = ChemistryUnits.GasUnitsPerMole;

    public static ReactionResult EvaluateMakeOxygen(bool hot, bool hasReagent, bool collectionConnected)
    {
        if (!hasReagent)
            return ReactionResult.Failure("No reagent in the test tube");

        ReactionContext context = new ReactionContext
        {
            IsHeating = hot,
            GasCollectionConnected = collectionConnected,
            HasOxygen = false,
            MixingAmount = 1F
        };
        context.Mixture.SetTemperature(hot ? 250F : 25F);
        context.Mixture.AddReagent(new ReagentPortion("kclo3", 0.0001F));
        context.Catalysts.Add("mno2");
        return ChemistrySimulationService.GetOrCreate().Evaluate("make_oxygen", context);
    }

    public static ReactionResult EvaluateMatchRefire(bool canRefire, float oxygenUnits)
    {
        ReactionContext context = new ReactionContext
        {
            HasHotEmber = canRefire,
            HasOxygen = oxygenUnits > 0F,
            IsIgnited = true
        };
        context.Mixture.AddReagent(new ReagentPortion("carbon", 0.0001F));
        context.Mixture.AddReagent(new ReagentPortion("o2", oxygenUnits / GasUnitsPerMole));
        return ChemistrySimulationService.GetOrCreate().Evaluate("match_refire", context);
    }

    public static void AddProducedOxygen(OxygenSupply supply, float deltaTime, bool collectionConnected)
    {
        if (supply == null)
            return;

        ReactionResult result = EvaluateMakeOxygen(true, true, collectionConnected);
        if (result.Succeeded && result.GasProduced.TryGetValue("o2", out float oxygenMol))
            supply.AddOxygenMol(oxygenMol * result.ProgressPerSecond * deltaTime);
    }
}
