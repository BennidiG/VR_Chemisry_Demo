using System;

[Serializable]
public sealed class ReactionCondition
{
    public bool HasMinimumTemperature;
    public float MinimumTemperatureC;
    public string CatalystSubstanceId;
    public bool RequiresIgnition;
    public bool RequiresHotEmber;
    public bool RequiresOxygen;
    public bool RequiresMixing;
    public float MinimumMixing = 0.25F;
    public bool RequiresWater;
    public bool RequiresElectrolysis;
    public bool RequiresGasCollection;

    public string GetFailureReason(ReactionContext context)
    {
        if (context == null || context.Mixture == null)
            return "Reaction context or mixture is missing.";
        if (!IsFinite(MinimumTemperatureC))
            return "Minimum temperature must be finite.";
        if (!IsFinite(MinimumMixing))
            return "Minimum mixing must be finite.";
        if ((HasMinimumTemperature || MinimumTemperatureC != 0F) && context.Mixture.TemperatureC < MinimumTemperatureC)
            return "Temperature is below the reaction requirement: " + MinimumTemperatureC + " C.";
        if (!string.IsNullOrWhiteSpace(CatalystSubstanceId) && (context.Catalysts == null || !context.Catalysts.Contains(CatalystSubstanceId)))
            return "Required catalyst is missing: " + CatalystSubstanceId;
        if (RequiresIgnition && !context.IsIgnited)
            return "An ignition source is required.";
        if (RequiresHotEmber && !context.HasHotEmber)
            return "A hot ember is required.";
        if (RequiresOxygen && !context.HasOxygen)
            return "Oxygen is required.";
        if (RequiresMixing && (!IsFinite(context.MixingAmount) || context.MixingAmount < MinimumMixing))
            return "Mix the reactants before evaluating the reaction.";
        if (RequiresWater && !context.HasWater)
            return "Water is required.";
        if (RequiresElectrolysis && !context.IsElectrolysis)
            return "An electrolysis power supply is required.";
        if (RequiresGasCollection && !context.GasCollectionConnected)
            return "Connect the gas collection apparatus first.";
        return null;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
