using UnityEngine;

[CreateAssetMenu(menuName = "VR Chemistry/Substance")]
public class SubstanceDefinitionAsset : ScriptableObject
{
    public string id;
    public string displayName;
    public string formula;
    public float molarMass;
    public SubstanceState state;
    public HazardLevel hazard;
    [TextArea] public string description;
    public Color displayColor = Color.white;

    public ChemicalSubstance ToSubstance()
    {
        return new ChemicalSubstance
        {
            Id = id,
            DisplayName = displayName,
            Formula = formula,
            MolarMass = molarMass,
            State = state,
            Hazard = hazard,
            Description = description
        };
    }
}
