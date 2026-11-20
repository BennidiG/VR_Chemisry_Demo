using UnityEngine;

[CreateAssetMenu(menuName = "VR Chemistry/Reaction")]
public class ReactionDefinitionAsset : ScriptableObject
{
    public string reactionId;
    public string displayName;
    public string equation;
    public ReactionParticipant[] reactants;
    public ReactionParticipant[] products;
    public ReactionCondition conditions = new ReactionCondition();
    public float progressPerSecond = 0.1F;
    public bool exothermic;
    [TextArea] public string safetyNote;

    public ReactionDefinition ToDefinition()
    {
        return new ReactionDefinition
        {
            Id = reactionId,
            DisplayName = displayName,
            Equation = equation,
            Reactants = reactants != null ? new System.Collections.Generic.List<ReactionParticipant>(reactants) : new System.Collections.Generic.List<ReactionParticipant>(),
            Products = products != null ? new System.Collections.Generic.List<ReactionParticipant>(products) : new System.Collections.Generic.List<ReactionParticipant>(),
            Conditions = conditions ?? new ReactionCondition(),
            ProgressPerSecond = progressPerSecond,
            Exothermic = exothermic,
            SafetyNote = safetyNote
        };
    }
}
