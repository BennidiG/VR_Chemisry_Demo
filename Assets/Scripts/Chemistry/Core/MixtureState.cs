using System;
using System.Collections.Generic;

[Serializable]
public sealed class ReagentPortion
{
    public string SubstanceId;
    public float AmountMol;
    public float VolumeMl;
    public float ConcentrationMolPerLiter;

    public ReagentPortion() { }

    public ReagentPortion(string substanceId, float amountMol, float volumeMl = 0F, float concentrationMolPerLiter = 0F)
    {
        SubstanceId = substanceId;
        AmountMol = amountMol;
        VolumeMl = volumeMl;
        ConcentrationMolPerLiter = concentrationMolPerLiter;
    }
}

[Serializable]
public sealed class MixtureState
{
    public float CapacityMl = 100F;
    public float VolumeMl;
    public float TemperatureC = 25F;
    public float PH = 7F;
    public float PrecipitateMol;
    public float GasAmountMol;
    public string ObservationColor = "colorless";
    public readonly List<ReagentPortion> Reagents = new List<ReagentPortion>();
    public readonly Dictionary<string, float> GasComponents = new Dictionary<string, float>();
    public readonly Dictionary<string, float> SolidComponents = new Dictionary<string, float>();

    public bool CanAddReagent(ReagentPortion portion)
    {
        if (portion == null || string.IsNullOrWhiteSpace(portion.SubstanceId) || !IsFinite(portion.AmountMol) || !IsFinite(portion.VolumeMl) || !IsFinite(portion.ConcentrationMolPerLiter))
            return false;
        if (portion.AmountMol <= 0F || portion.VolumeMl < 0F || !IsFinite(CapacityMl) || CapacityMl < 0F || !IsFinite(VolumeMl) || VolumeMl < 0F)
            return false;
        if (portion.VolumeMl > CapacityMl - VolumeMl || !IsFinite(VolumeMl + portion.VolumeMl))
            return false;

        for (int index = 0; index < Reagents.Count; index++)
        {
            if (Reagents[index] == null || Reagents[index].SubstanceId != portion.SubstanceId)
                continue;
            float newAmount = Reagents[index].AmountMol + portion.AmountMol;
            float newVolume = Reagents[index].VolumeMl + portion.VolumeMl;
            return IsFinite(newAmount) && IsFinite(newVolume) && IsFinite(CalculateConcentration(newAmount, newVolume));
        }
        return IsFinite(CalculateConcentration(portion.AmountMol, portion.VolumeMl));
    }

    public bool AddReagent(ReagentPortion portion)
    {
        if (!CanAddReagent(portion))
            return false;

        for (int index = 0; index < Reagents.Count; index++)
        {
            if (Reagents[index] == null || Reagents[index].SubstanceId != portion.SubstanceId)
                continue;
            Reagents[index].AmountMol += portion.AmountMol;
            Reagents[index].VolumeMl += portion.VolumeMl;
            Reagents[index].ConcentrationMolPerLiter = CalculateConcentration(Reagents[index].AmountMol, Reagents[index].VolumeMl);
            VolumeMl += portion.VolumeMl;
            return true;
        }

        Reagents.Add(new ReagentPortion(portion.SubstanceId, portion.AmountMol, portion.VolumeMl, CalculateConcentration(portion.AmountMol, portion.VolumeMl)));
        VolumeMl += portion.VolumeMl;
        return true;
    }

    public bool CanAddProduct(string substanceId, SubstanceState phase, float amountMol)
    {
        if (string.IsNullOrWhiteSpace(substanceId) || !IsFinite(amountMol) || amountMol <= 0F)
            return false;
        switch (phase)
        {
            case SubstanceState.Gas:
                float gas = GasComponents.TryGetValue(substanceId, out float existingGas) ? existingGas : 0F;
                return IsFinite(gas + amountMol) && IsFinite(GasAmountMol + amountMol);
            case SubstanceState.Solid:
                float solid = SolidComponents.TryGetValue(substanceId, out float existingSolid) ? existingSolid : 0F;
                return IsFinite(solid + amountMol) && IsFinite(PrecipitateMol + amountMol);
            default:
                return CanAddReagent(new ReagentPortion(substanceId, amountMol));
        }
    }

    public bool AddProduct(string substanceId, SubstanceState phase, float amountMol)
    {
        if (!CanAddProduct(substanceId, phase, amountMol))
            return false;

        switch (phase)
        {
            case SubstanceState.Gas:
                GasComponents[substanceId] = (GasComponents.TryGetValue(substanceId, out float gas) ? gas : 0F) + amountMol;
                GasAmountMol += amountMol;
                return true;
            case SubstanceState.Solid:
                SolidComponents[substanceId] = (SolidComponents.TryGetValue(substanceId, out float solid) ? solid : 0F) + amountMol;
                PrecipitateMol += amountMol;
                return true;
            default:
                return AddReagent(new ReagentPortion(substanceId, amountMol));
        }
    }

    public bool CanConsume(string substanceId, float amountMol)
    {
        if (string.IsNullOrWhiteSpace(substanceId) || !IsFinite(amountMol) || amountMol <= 0F)
            return false;
        float available = GetAmount(substanceId);
        return IsFinite(available) && (available >= amountMol || NearlyEqual(available, amountMol));
    }

    public float GetAmount(string substanceId)
    {
        if (string.IsNullOrWhiteSpace(substanceId))
            return 0F;
        double amount = 0.0;
        foreach (ReagentPortion portion in Reagents)
        {
            if (portion != null && portion.SubstanceId == substanceId)
                amount += portion.AmountMol;
        }
        if (GasComponents.TryGetValue(substanceId, out float gas))
            amount += gas;
        if (SolidComponents.TryGetValue(substanceId, out float solid))
            amount += solid;
        if (double.IsNaN(amount))
            return float.NaN;
        return amount > float.MaxValue ? float.PositiveInfinity : (float)amount;
    }

    public float GetGasAmount(string substanceId)
    {
        return !string.IsNullOrWhiteSpace(substanceId) && GasComponents.TryGetValue(substanceId, out float amount) ? amount : 0F;
    }

    public float GetSolidAmount(string substanceId)
    {
        return !string.IsNullOrWhiteSpace(substanceId) && SolidComponents.TryGetValue(substanceId, out float amount) ? amount : 0F;
    }

    public float Consume(string substanceId, float amountMol)
    {
        if (string.IsNullOrWhiteSpace(substanceId) || !IsFinite(amountMol) || amountMol <= 0F)
            return 0F;

        float remaining = amountMol;
        float consumed = 0F;
        for (int index = Reagents.Count - 1; index >= 0 && remaining > 0F; index--)
        {
            ReagentPortion portion = Reagents[index];
            if (portion == null || portion.SubstanceId != substanceId)
                continue;
            if (!IsFinite(portion.AmountMol) || portion.AmountMol <= 0F)
            {
                Reagents.RemoveAt(index);
                continue;
            }

            float before = portion.AmountMol;
            float taken = Math.Min(remaining, before);
            float fraction = taken / before;
            remaining -= taken;
            consumed += taken;
            VolumeMl = Math.Max(0F, VolumeMl - portion.VolumeMl * fraction);
            portion.VolumeMl = Math.Max(0F, portion.VolumeMl * (1F - fraction));
            portion.AmountMol -= taken;
            portion.ConcentrationMolPerLiter = CalculateConcentration(portion.AmountMol, portion.VolumeMl);
            if (portion.AmountMol <= 0F)
                Reagents.RemoveAt(index);
        }

        if (remaining > 0F)
        {
            float taken = ConsumeComponent(GasComponents, substanceId, remaining);
            remaining -= taken;
            consumed += taken;
            GasAmountMol = Math.Max(0F, GasAmountMol - taken);
        }
        if (remaining > 0F)
        {
            float taken = ConsumeComponent(SolidComponents, substanceId, remaining);
            consumed += taken;
            PrecipitateMol = Math.Max(0F, PrecipitateMol - taken);
        }
        return consumed;
    }

    public MixtureState Clone()
    {
        MixtureState clone = new MixtureState();
        clone.CopyFrom(this);
        return clone;
    }

    public void CopyFrom(MixtureState other)
    {
        if (other == null || ReferenceEquals(this, other))
            return;
        CapacityMl = other.CapacityMl;
        VolumeMl = other.VolumeMl;
        TemperatureC = other.TemperatureC;
        PH = other.PH;
        PrecipitateMol = other.PrecipitateMol;
        GasAmountMol = other.GasAmountMol;
        ObservationColor = other.ObservationColor;
        Reagents.Clear();
        foreach (ReagentPortion portion in other.Reagents)
        {
            if (portion != null)
                Reagents.Add(new ReagentPortion(portion.SubstanceId, portion.AmountMol, portion.VolumeMl, portion.ConcentrationMolPerLiter));
        }
        GasComponents.Clear();
        foreach (KeyValuePair<string, float> pair in other.GasComponents)
            GasComponents.Add(pair.Key, pair.Value);
        SolidComponents.Clear();
        foreach (KeyValuePair<string, float> pair in other.SolidComponents)
            SolidComponents.Add(pair.Key, pair.Value);
    }

    public void SetTemperature(float temperatureC)
    {
        if (!IsFinite(temperatureC))
            return;
        TemperatureC = Math.Max(-273.15F, temperatureC);
    }

    private static float ConsumeComponent(Dictionary<string, float> components, string substanceId, float requested)
    {
        if (!components.TryGetValue(substanceId, out float available) || !IsFinite(available) || available <= 0F)
            return 0F;
        float taken = Math.Min(available, requested);
        float remaining = available - taken;
        if (remaining <= 0F)
            components.Remove(substanceId);
        else
            components[substanceId] = remaining;
        return taken;
    }

    private static float CalculateConcentration(float amountMol, float volumeMl)
    {
        return volumeMl > 0F ? amountMol * 1000F / volumeMl : 0F;
    }

    private static bool NearlyEqual(float left, float right)
    {
        if (!IsFinite(left) || !IsFinite(right))
            return false;
        float scale = Math.Max(Math.Abs(left), Math.Abs(right));
        return Math.Abs(left - right) <= scale * 0.000001F;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
