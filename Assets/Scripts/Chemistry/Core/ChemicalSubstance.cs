using System;

[Serializable]
public enum SubstanceState
{
    Solid = 0,
    Liquid = 1,
    Gas = 2,
    Aqueous = 3
}

[Serializable]
public enum HazardLevel
{
    Low = 0,
    Irritant = 1,
    Oxidizer = 2,
    Flammable = 3,
    Corrosive = 4,
    Asphyxiant = 5
}

[Serializable]
public sealed class ChemicalSubstance
{
    public string Id;
    public string DisplayName;
    public string Formula;
    public float MolarMass;
    public SubstanceState State;
    public HazardLevel Hazard;
    public string Description;
}
