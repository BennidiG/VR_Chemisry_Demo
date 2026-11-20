using System;
using System.Collections.Generic;

[Serializable]
public sealed class ReactionContext
{
    public MixtureState Mixture = new MixtureState();
    public readonly HashSet<string> Catalysts = new HashSet<string>();
    public bool IsHeating;
    public bool IsIgnited;
    public bool HasHotEmber;
    public bool HasOxygen;
    public float MixingAmount = 1F;
    public bool HasWater = true;
    public bool IsElectrolysis;
    public bool GasCollectionConnected = true;
}
