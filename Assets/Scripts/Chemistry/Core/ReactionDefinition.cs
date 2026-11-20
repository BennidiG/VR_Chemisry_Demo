using System;
using System.Collections.Generic;

[Serializable]
public enum ReactionParticipantRole
{
    Reactant = 0,
    Product = 1,
    Catalyst = 2
}

[Serializable]
public sealed class ReactionParticipant
{
    public string SubstanceId;
    public float Coefficient = 1F;
    public ReactionParticipantRole Role;
    public SubstanceState Phase;

    public ReactionParticipant() { }

    public ReactionParticipant(string substanceId, float coefficient, ReactionParticipantRole role, SubstanceState phase)
    {
        SubstanceId = substanceId;
        Coefficient = coefficient;
        Role = role;
        Phase = phase;
    }
}

[Serializable]
public sealed class ReactionDefinition
{
    public string Id;
    public string DisplayName;
    public string Equation;
    public List<ReactionParticipant> Reactants = new List<ReactionParticipant>();
    public List<ReactionParticipant> Products = new List<ReactionParticipant>();
    public ReactionCondition Conditions = new ReactionCondition();
    public float ProgressPerSecond = 0.1F;
    public bool Exothermic;
    public string SafetyNote;

    public IEnumerable<string> ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(Id))
            yield return "Reaction id is empty.";
        if (Reactants == null || Reactants.Count == 0)
            yield return Id + ": no reactants.";
        if (Products == null || Products.Count == 0)
            yield return Id + ": no products.";
        if (!IsFinite(ProgressPerSecond) || ProgressPerSecond <= 0F)
            yield return Id + ": progress rate must be finite and positive.";
        if (Conditions != null)
        {
            if (!IsFinite(Conditions.MinimumTemperatureC))
                yield return Id + ": minimum temperature must be finite.";
            if (!IsFinite(Conditions.MinimumMixing) || Conditions.MinimumMixing < 0F)
                yield return Id + ": minimum mixing must be finite and non-negative.";
        }

        foreach (string error in ValidateParticipants(Reactants, "reactant"))
            yield return error;
        foreach (string error in ValidateParticipants(Products, "product"))
            yield return error;
    }

    private IEnumerable<string> ValidateParticipants(IEnumerable<ReactionParticipant> participants, string label)
    {
        if (participants == null)
            yield break;
        Dictionary<string, SubstanceState> phases = new Dictionary<string, SubstanceState>();
        foreach (ReactionParticipant participant in participants)
        {
            if (participant == null)
            {
                yield return Id + ": " + label + " contains a null participant.";
                continue;
            }
            if (string.IsNullOrWhiteSpace(participant.SubstanceId))
            {
                yield return Id + ": " + label + " has no substance id.";
                continue;
            }
            if (!IsFinite(participant.Coefficient) || participant.Coefficient <= 0F)
                yield return Id + ": " + label + " coefficient must be finite and positive.";
            if (phases.TryGetValue(participant.SubstanceId, out SubstanceState phase) && phase != participant.Phase)
                yield return Id + ": conflicting phases for " + participant.SubstanceId;
            phases[participant.SubstanceId] = participant.Phase;
        }
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
