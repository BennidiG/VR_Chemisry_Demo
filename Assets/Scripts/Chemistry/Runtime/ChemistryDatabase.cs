using System.Collections.Generic;
using UnityEngine;

public static class ChemistryDatabase
{
    private static bool loaded;
    private static readonly Dictionary<string, ReactionDefinition> reactions = new Dictionary<string, ReactionDefinition>();
    private static readonly Dictionary<string, ChemicalSubstance> substances = new Dictionary<string, ChemicalSubstance>();
    private static readonly List<string> loadErrors = new List<string>();

    public static IReadOnlyList<string> LoadErrors => loadErrors;

    public static void EnsureLoaded()
    {
        if (loaded)
            return;
        loaded = true;

        foreach (SubstanceDefinitionAsset asset in Resources.LoadAll<SubstanceDefinitionAsset>("Chemistry/Substances"))
        {
            ChemicalSubstance substance = asset.ToSubstance();
            if (string.IsNullOrWhiteSpace(substance.Id) || substance.MolarMass <= 0F || float.IsNaN(substance.MolarMass) || float.IsInfinity(substance.MolarMass))
            {
                loadErrors.Add(asset.name + ": invalid substance id or molar mass.");
                continue;
            }
            if (substances.ContainsKey(substance.Id))
            {
                loadErrors.Add("Duplicate substance id: " + substance.Id);
                continue;
            }
            substances.Add(substance.Id, substance);
        }

        foreach (ReactionDefinitionAsset asset in Resources.LoadAll<ReactionDefinitionAsset>("Chemistry/Reactions"))
        {
            ReactionDefinition definition = asset.ToDefinition();
            if (string.IsNullOrWhiteSpace(definition.Id))
            {
                loadErrors.Add(asset.name + ": reaction id is empty.");
                continue;
            }
            if (reactions.ContainsKey(definition.Id))
            {
                loadErrors.Add("Duplicate reaction id: " + definition.Id);
                continue;
            }
            bool valid = true;
            foreach (string error in definition.ValidateConfiguration())
            {
                loadErrors.Add(error);
                valid = false;
            }
            foreach (ReactionParticipant participant in definition.Reactants)
            {
                if (participant == null || string.IsNullOrWhiteSpace(participant.SubstanceId))
                {
                    valid = false;
                    continue;
                }
                if (!substances.ContainsKey(participant.SubstanceId))
                {
                    loadErrors.Add(definition.Id + ": unknown reactant substance " + participant.SubstanceId);
                    valid = false;
                }
            }
            foreach (ReactionParticipant participant in definition.Products)
            {
                if (participant == null || string.IsNullOrWhiteSpace(participant.SubstanceId))
                {
                    valid = false;
                    continue;
                }
                if (!substances.ContainsKey(participant.SubstanceId))
                {
                    loadErrors.Add(definition.Id + ": unknown product substance " + participant.SubstanceId);
                    valid = false;
                }
            }
            if (valid)
                reactions.Add(definition.Id, definition);
            else
                loadErrors.Add(definition.Id + ": invalid reaction was not registered.");
        }
    }

    public static ReactionDefinition GetReaction(string reactionId)
    {
        if (string.IsNullOrWhiteSpace(reactionId))
            return null;
        EnsureLoaded();
        return reactions.TryGetValue(reactionId, out ReactionDefinition definition) ? definition : null;
    }

    public static ChemicalSubstance GetSubstance(string substanceId)
    {
        if (string.IsNullOrWhiteSpace(substanceId))
            return null;
        EnsureLoaded();
        return substances.TryGetValue(substanceId, out ChemicalSubstance substance) ? substance : null;
    }
}
