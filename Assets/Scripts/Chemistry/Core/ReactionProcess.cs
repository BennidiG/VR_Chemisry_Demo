using System;
using System.Collections.Generic;

public enum ReactionRunStatus
{
    NotStarted = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public sealed class ReactionTickResult
{
    public ReactionRunStatus Status;
    public float Progress;
    public float DeltaProgress;
    public string FailureReason;
    public ReactionResult Template;
    public readonly Dictionary<string, float> Consumed = new Dictionary<string, float>();
    public readonly Dictionary<string, float> Produced = new Dictionary<string, float>();
}

/// <summary>
/// A stateful reaction with an explicit progress/commit lifecycle. Evaluate()
/// remains a pure stoichiometry query; this class owns time-based application.
/// </summary>
public sealed class ReactionProcess
{
    public ReactionDefinition Definition { get; }
    public ReactionContext Context { get; }
    public ReactionResult BaseResult { get; private set; }
    public ReactionRunStatus Status { get; private set; } = ReactionRunStatus.NotStarted;
    public float Progress { get; private set; }
    public float ElapsedSeconds { get; private set; }
    public string FailureReason { get; private set; }

    private MixtureState initialMixture;

    public ReactionProcess(ReactionDefinition definition, ReactionContext context)
    {
        Definition = definition;
        Context = context;
    }

    public ReactionResult Start()
    {
        if (Status == ReactionRunStatus.Running || Status == ReactionRunStatus.Paused)
            return ReactionResult.Failure("Reaction process is already running.");

        Progress = 0F;
        ElapsedSeconds = 0F;
        FailureReason = null;
        BaseResult = null;
        initialMixture = Context != null && Context.Mixture != null ? Context.Mixture.Clone() : null;
        BaseResult = ReactionEngine.Evaluate(Definition, Context);
        if (!BaseResult.Succeeded)
        {
            Status = ReactionRunStatus.Failed;
            FailureReason = BaseResult.FailureReason;
            return BaseResult;
        }

        Status = ReactionRunStatus.Running;
        return BaseResult;
    }

    public ReactionTickResult Tick(float deltaTime)
    {
        ReactionTickResult tick = NewTick();
        if (Status == ReactionRunStatus.NotStarted || Status == ReactionRunStatus.Completed || Status == ReactionRunStatus.Failed || Status == ReactionRunStatus.Cancelled)
            return tick;

        if (!IsFinite(deltaTime) || deltaTime < 0F)
            return FailTick(tick, "Delta time must be finite and non-negative.");
        if (deltaTime == 0F)
            return tick;

        ReactionCondition conditions = Definition.Conditions ?? new ReactionCondition();
        string conditionError = conditions.GetFailureReason(Context);
        if (!string.IsNullOrEmpty(conditionError))
        {
            Status = ReactionRunStatus.Paused;
            FailureReason = conditionError;
            tick.Status = Status;
            tick.FailureReason = FailureReason;
            return tick;
        }

        Status = ReactionRunStatus.Running;
        FailureReason = null;
        float deltaProgress = Definition.ProgressPerSecond * deltaTime;
        if (!IsFinite(deltaProgress) || deltaProgress < 0F)
            return FailTick(tick, "Reaction progress delta is invalid.");

        deltaProgress = Math.Min(deltaProgress, 1F - Progress);
        if (deltaProgress <= 0F)
        {
            tick.Status = Status;
            tick.Progress = Progress;
            tick.FailureReason = FailureReason;
            return tick;
        }

        float activeDeltaTime = deltaProgress / Definition.ProgressPerSecond;
        float nextElapsed = ElapsedSeconds + activeDeltaTime;
        if (!IsFinite(activeDeltaTime) || !IsFinite(nextElapsed))
            return FailTick(tick, "Reaction elapsed time is invalid.");
        if (!CanApply(deltaProgress, out string applyError))
            return FailTick(tick, applyError);

        MixtureState beforeTick = Context.Mixture.Clone();
        if (!Apply(deltaProgress, tick, out string commitError))
        {
            Context.Mixture.CopyFrom(beforeTick);
            tick.Consumed.Clear();
            tick.Produced.Clear();
            return FailTick(tick, commitError);
        }

        Progress += deltaProgress;
        ElapsedSeconds = nextElapsed;
        if (Progress >= 1F)
        {
            Progress = 1F;
            Status = ReactionRunStatus.Completed;
        }
        else
        {
            Status = ReactionRunStatus.Running;
        }

        tick.Status = Status;
        tick.Progress = Progress;
        tick.DeltaProgress = deltaProgress;
        tick.FailureReason = null;
        return tick;
    }

    public void Cancel()
    {
        if (Status == ReactionRunStatus.Running || Status == ReactionRunStatus.Paused)
            Status = ReactionRunStatus.Cancelled;
    }

    public void Reset(bool restoreMixture = true)
    {
        if (restoreMixture && initialMixture != null && Context != null && Context.Mixture != null)
            Context.Mixture.CopyFrom(initialMixture);
        Status = ReactionRunStatus.NotStarted;
        Progress = 0F;
        ElapsedSeconds = 0F;
        FailureReason = null;
        BaseResult = null;
        initialMixture = null;
    }

    private bool CanApply(float deltaProgress, out string error)
    {
        foreach (KeyValuePair<string, float> pair in BaseResult.ConsumedReagents)
        {
            if (!Context.Mixture.CanConsume(pair.Key, pair.Value * deltaProgress))
            {
                error = "Insufficient reagent during reaction progress: " + pair.Key;
                return false;
            }
        }

        foreach (KeyValuePair<string, float> pair in BaseResult.GasProduced)
        {
            if (!Context.Mixture.CanAddProduct(pair.Key, SubstanceState.Gas, pair.Value * deltaProgress))
            {
                error = "Cannot add gas product: " + pair.Key;
                return false;
            }
        }
        foreach (KeyValuePair<string, float> pair in BaseResult.AqueousProduced)
        {
            if (!Context.Mixture.CanAddProduct(pair.Key, SubstanceState.Aqueous, pair.Value * deltaProgress))
            {
                error = "Cannot add aqueous product: " + pair.Key;
                return false;
            }
        }
        foreach (KeyValuePair<string, float> pair in BaseResult.LiquidProduced)
        {
            if (!Context.Mixture.CanAddProduct(pair.Key, SubstanceState.Liquid, pair.Value * deltaProgress))
            {
                error = "Cannot add liquid product: " + pair.Key;
                return false;
            }
        }
        foreach (KeyValuePair<string, float> pair in BaseResult.SolidProduced)
        {
            if (!Context.Mixture.CanAddProduct(pair.Key, SubstanceState.Solid, pair.Value * deltaProgress))
            {
                error = "Cannot add solid product: " + pair.Key;
                return false;
            }
        }
        float totalGas = Context.Mixture.GasAmountMol;
        foreach (KeyValuePair<string, float> pair in BaseResult.GasProduced)
            totalGas += pair.Value * deltaProgress;
        if (!IsFinite(totalGas))
        {
            error = "Total gas product amount is not finite.";
            return false;
        }

        float totalSolid = Context.Mixture.PrecipitateMol;
        foreach (KeyValuePair<string, float> pair in BaseResult.SolidProduced)
            totalSolid += pair.Value * deltaProgress;
        if (!IsFinite(totalSolid))
        {
            error = "Total solid product amount is not finite.";
            return false;
        }

        error = null;
        return true;
    }

    private bool Apply(float deltaProgress, ReactionTickResult tick, out string error)
    {
        error = null;
        foreach (KeyValuePair<string, float> pair in BaseResult.ConsumedReagents)
        {
            float amount = pair.Value * deltaProgress;
            float actual = Context.Mixture.Consume(pair.Key, amount);
            if (actual != amount && !NearlyEqual(actual, amount))
            {
                error = "Reagent consumption was not committed exactly: " + pair.Key;
                return false;
            }
            if (!AddAmount(tick.Consumed, pair.Key, actual))
            {
                error = "Reaction consumption accounting overflowed: " + pair.Key;
                return false;
            }
        }
        return ApplyProducts(BaseResult.GasProduced, SubstanceState.Gas, deltaProgress, tick, out error)
            && ApplyProducts(BaseResult.AqueousProduced, SubstanceState.Aqueous, deltaProgress, tick, out error)
            && ApplyProducts(BaseResult.LiquidProduced, SubstanceState.Liquid, deltaProgress, tick, out error)
            && ApplyProducts(BaseResult.SolidProduced, SubstanceState.Solid, deltaProgress, tick, out error);
    }

    private bool ApplyProducts(Dictionary<string, float> products, SubstanceState phase, float deltaProgress, ReactionTickResult tick, out string error)
    {
        error = null;
        foreach (KeyValuePair<string, float> pair in products)
        {
            float amount = pair.Value * deltaProgress;
            if (!Context.Mixture.AddProduct(pair.Key, phase, amount))
            {
                error = "Product could not be committed: " + pair.Key;
                return false;
            }
            if (!AddAmount(tick.Produced, pair.Key, amount))
            {
                error = "Reaction product accounting overflowed: " + pair.Key;
                return false;
            }
        }
        return true;
    }

    private ReactionTickResult NewTick()
    {
        return new ReactionTickResult
        {
            Status = Status,
            Progress = Progress,
            FailureReason = FailureReason,
            Template = BaseResult
        };
    }

    private ReactionTickResult FailTick(ReactionTickResult tick, string reason)
    {
        Status = ReactionRunStatus.Failed;
        FailureReason = reason;
        tick.Status = Status;
        tick.FailureReason = reason;
        return tick;
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
