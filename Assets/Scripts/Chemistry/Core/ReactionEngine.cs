using System;
using System.Collections.Generic;

public static class ReactionEngine
{
    public static ReactionResult Evaluate(ReactionDefinition definition, ReactionContext context)
    {
        if (definition == null)
            return ReactionResult.Failure("Reaction definition is missing.");
        if (context == null || context.Mixture == null)
            return ReactionResult.Failure("Reaction context or mixture is missing.");

        foreach (string error in definition.ValidateConfiguration())
            return ReactionResult.Failure(error);

        ReactionCondition conditions = definition.Conditions ?? new ReactionCondition();
        string conditionError = conditions.GetFailureReason(context);
        if (!string.IsNullOrEmpty(conditionError))
            return ReactionResult.Failure(conditionError);

        Dictionary<string, ReactionParticipant> reactants = AggregateParticipants(definition.Reactants, false);
        Dictionary<string, ReactionParticipant> products = AggregateParticipants(definition.Products, true);
        if (reactants == null || products == null)
            return ReactionResult.Failure("Reaction participant phases are inconsistent.");

        float extent = float.PositiveInfinity;
        string limitingReagent = null;
        foreach (ReactionParticipant participant in reactants.Values)
        {
            float amount = context.Mixture.GetAmount(participant.SubstanceId);
            if (!IsFinite(amount) || amount <= 0F)
                return ReactionResult.Failure("Missing reagent: " + participant.SubstanceId);

            float candidate = amount / participant.Coefficient;
            if (!IsFinite(candidate) || candidate <= 0F)
                return ReactionResult.Failure("Invalid reagent amount: " + participant.SubstanceId);
            if (candidate < extent)
            {
                extent = candidate;
                limitingReagent = participant.SubstanceId;
            }
        }

        if (!IsFinite(extent) || extent <= 0F)
            return ReactionResult.Failure("No reactant is available for this reaction.");

        ReactionResult result = new ReactionResult
        {
            Succeeded = true,
            Message = definition.Equation,
            ExtentMol = extent,
            LimitingReagentId = limitingReagent,
            ProgressPerSecond = definition.ProgressPerSecond,
            HeatReleased = definition.Exothermic,
            SafetyNote = definition.SafetyNote
        };

        foreach (ReactionParticipant participant in reactants.Values)
        {
            float consumed = participant.Coefficient * extent;
            if (!AddAmount(result.ConsumedReagents, participant.SubstanceId, consumed))
                return ReactionResult.Failure("Non-finite reagent consumption.");
            if (participant.Phase == SubstanceState.Gas && !AddAmount(result.GasConsumed, participant.SubstanceId, consumed))
                return ReactionResult.Failure("Non-finite gas consumption.");
        }

        foreach (ReactionParticipant participant in products.Values)
        {
            float amount = participant.Coefficient * extent;
            switch (participant.Phase)
            {
                case SubstanceState.Gas:
                    if (!AddAmount(result.GasProduced, participant.SubstanceId, amount))
                        return ReactionResult.Failure("Non-finite gas production.");
                    break;
                case SubstanceState.Aqueous:
                    if (!AddAmount(result.AqueousProduced, participant.SubstanceId, amount))
                        return ReactionResult.Failure("Non-finite aqueous production.");
                    break;
                case SubstanceState.Liquid:
                    if (!AddAmount(result.LiquidProduced, participant.SubstanceId, amount))
                        return ReactionResult.Failure("Non-finite liquid production.");
                    break;
                case SubstanceState.Solid:
                    if (!AddAmount(result.SolidProduced, participant.SubstanceId, amount))
                        return ReactionResult.Failure("Non-finite solid production.");
                    break;
            }
        }
        return result;
    }

    private static Dictionary<string, ReactionParticipant> AggregateParticipants(IEnumerable<ReactionParticipant> participants, bool includeCatalysts)
    {
        Dictionary<string, ReactionParticipant> aggregated = new Dictionary<string, ReactionParticipant>();
        if (participants == null)
            return aggregated;

        foreach (ReactionParticipant participant in participants)
        {
            if (participant == null || (!includeCatalysts && participant.Role == ReactionParticipantRole.Catalyst))
                continue;
            if (string.IsNullOrWhiteSpace(participant.SubstanceId) || !IsFinite(participant.Coefficient) || participant.Coefficient <= 0F)
                continue;
            if (aggregated.TryGetValue(participant.SubstanceId, out ReactionParticipant existing))
            {
                if (existing.Phase != participant.Phase)
                    return null;
                existing.Coefficient += participant.Coefficient;
            }
            else
            {
                aggregated.Add(participant.SubstanceId, new ReactionParticipant(participant.SubstanceId, participant.Coefficient, participant.Role, participant.Phase));
            }
        }
        return aggregated;
    }

    private static bool AddAmount(Dictionary<string, float> amounts, string substanceId, float amount)
    {
        if (!IsFinite(amount))
            return false;
        float current = amounts.TryGetValue(substanceId, out float value) ? value : 0F;
        float total = current + amount;
        if (!IsFinite(total))
            return false;
        amounts[substanceId] = total;
        return true;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
