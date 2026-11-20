using System;
using System.Collections.Generic;

[Serializable]
public sealed class ReactionResult
{
    public bool Succeeded;
    public string Message;
    public string FailureReason;
    public string LimitingReagentId;
    public float ExtentMol;
    public float ProgressPerSecond;
    public float InputMass;
    public float OutputMass;
    public float MassBalanceError;
    public bool HeatReleased;
    public string SafetyNote;
    public readonly Dictionary<string, float> ConsumedReagents = new Dictionary<string, float>();
    public readonly Dictionary<string, float> GasProduced = new Dictionary<string, float>();
    public readonly Dictionary<string, float> GasConsumed = new Dictionary<string, float>();
    public readonly Dictionary<string, float> AqueousProduced = new Dictionary<string, float>();
    public readonly Dictionary<string, float> LiquidProduced = new Dictionary<string, float>();
    public readonly Dictionary<string, float> SolidProduced = new Dictionary<string, float>();

    public static ReactionResult Failure(string reason)
    {
        return new ReactionResult { Succeeded = false, FailureReason = reason, Message = reason };
    }
}
