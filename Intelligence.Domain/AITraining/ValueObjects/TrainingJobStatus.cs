/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               TrainingJobStatus.cs
    DESCRIPTION:        Smart Enum for TrainingJob lifecycle status. (Rule 2.7)
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.AITraining.ValueObjects;

/// <summary>Smart Enum representing the lifecycle status of an ML training job.</summary>
public sealed class TrainingJobStatus : IEquatable<TrainingJobStatus>
{
    /// <summary>Job is queued, awaiting compute resources.</summary>
    public static readonly TrainingJobStatus Pending = new("Pending", 1, isTerminal: false, isRunning: false);

    /// <summary>Job is actively running on the compute cluster.</summary>
    public static readonly TrainingJobStatus Running = new("Running", 2, isTerminal: false, isRunning: true);

    /// <summary>Job is computing evaluation metrics.</summary>
    public static readonly TrainingJobStatus Evaluating = new("Evaluating", 3, isTerminal: false, isRunning: true);

    /// <summary>Job completed successfully; model artifact is available.</summary>
    public static readonly TrainingJobStatus Completed = new("Completed", 4, isTerminal: true, isRunning: false);

    /// <summary>Job failed due to a training error.</summary>
    public static readonly TrainingJobStatus Failed = new("Failed", 5, isTerminal: true, isRunning: false);

    /// <summary>Job was cancelled before or during execution.</summary>
    public static readonly TrainingJobStatus Cancelled = new("Cancelled", 6, isTerminal: true, isRunning: false);

    private static readonly Dictionary<string, TrainingJobStatus> _all = new(StringComparer.OrdinalIgnoreCase)
    {
        [Pending.Value] = Pending,
        [Running.Value] = Running,
        [Evaluating.Value] = Evaluating,
        [Completed.Value] = Completed,
        [Failed.Value] = Failed,
        [Cancelled.Value] = Cancelled
    };

    private TrainingJobStatus(string value, int order, bool isTerminal, bool isRunning)
    {
        Value = value;
        Order = order;
        IsTerminal = isTerminal;
        IsRunning = isRunning;
    }

    /// <summary>The string value of this status.</summary>
    public string Value { get; }

    /// <summary>Ordinal position in the lifecycle sequence.</summary>
    public int Order { get; }

    /// <summary>Whether this is a terminal (non-transitionable) state.</summary>
    public bool IsTerminal { get; }

    /// <summary>Whether compute resources are actively allocated in this state.</summary>
    public bool IsRunning { get; }

    /// <summary>Default status for a newly submitted job.</summary>
    public static TrainingJobStatus Default => Pending;

    /// <summary>All defined statuses.</summary>
    public static IReadOnlyCollection<TrainingJobStatus> All => _all.Values;

    /// <summary>Returns the status for the given string value.</summary>
    /// <exception cref="ArgumentException">Thrown when value is not a known status.</exception>
    public static TrainingJobStatus From(string value)
    {
        if (!_all.TryGetValue(value, out var status))
            throw new ArgumentException($"'{value}' is not a valid TrainingJobStatus.", nameof(value));
        return status;
    }

    /// <summary>Attempts to parse the given string into a <see cref="TrainingJobStatus"/>.</summary>
    public static bool TryParse(string value, out TrainingJobStatus? result) =>
        _all.TryGetValue(value, out result!);

    /// <inheritdoc />
    public bool Equals(TrainingJobStatus? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TrainingJobStatus other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator.</summary>
    public static bool operator ==(TrainingJobStatus? left, TrainingJobStatus? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(TrainingJobStatus? left, TrainingJobStatus? right) =>
        !(left == right);
}
