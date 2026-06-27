/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               ModelStatus.cs
    DESCRIPTION:        SAFETY-CRITICAL Smart Enum: governance status of a registered ML model.
                        Only Approved models may be deployed to production (Safety Rule 4.5). (Rule 2.7)
    SCHEMA VERSION:     2026.1  |  EVOLUTION: Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.ModelRegistry.ValueObjects;

/// <summary>
/// SAFETY-CRITICAL: Smart Enum representing governance status of a registered ML model.
/// Only <see cref="Approved"/> models are deployable (Safety Rule 4.5 — fail safe, not fail open).
/// </summary>
public sealed class ModelStatus : IEquatable<ModelStatus>
{
    /// <summary>Model registered but not yet submitted for approval.</summary>
    public static readonly ModelStatus Draft = new("Draft", 1, isTerminal: false, isDeployable: false);

    /// <summary>Model submitted and awaiting human approval.</summary>
    public static readonly ModelStatus PendingApproval = new("PendingApproval", 2, isTerminal: false, isDeployable: false);

    /// <summary>Model approved by a human reviewer. May be deployed.</summary>
    public static readonly ModelStatus Approved = new("Approved", 3, isTerminal: false, isDeployable: true);

    /// <summary>Model superseded by a newer version. Existing deployments may continue temporarily.</summary>
    public static readonly ModelStatus Deprecated = new("Deprecated", 4, isTerminal: false, isDeployable: false);

    /// <summary>Model permanently retired. No further deployments permitted.</summary>
    public static readonly ModelStatus Retired = new("Retired", 5, isTerminal: true, isDeployable: false);

    private static readonly Dictionary<string, ModelStatus> _all = new(StringComparer.OrdinalIgnoreCase)
    {
        [Draft.Value] = Draft,
        [PendingApproval.Value] = PendingApproval,
        [Approved.Value] = Approved,
        [Deprecated.Value] = Deprecated,
        [Retired.Value] = Retired
    };

    private ModelStatus(string value, int order, bool isTerminal, bool isDeployable)
    {
        Value = value;
        Order = order;
        IsTerminal = isTerminal;
        IsDeployable = isDeployable;
    }

    /// <summary>The string value of this status.</summary>
    public string Value { get; }

    /// <summary>Ordinal position in the governance lifecycle.</summary>
    public int Order { get; }

    /// <summary>Whether this is a terminal state (no further transitions).</summary>
    public bool IsTerminal { get; }

    /// <summary>SAFETY-CRITICAL: Whether models in this status may be deployed to production (Safety Rule 4.5).</summary>
    public bool IsDeployable { get; }

    /// <summary>Default status for a newly registered model.</summary>
    public static ModelStatus Default => Draft;

    /// <summary>All defined statuses.</summary>
    public static IReadOnlyCollection<ModelStatus> All => _all.Values;

    /// <summary>Returns the status for the given string value.</summary>
    /// <exception cref="ArgumentException">Thrown when value is not a known status.</exception>
    public static ModelStatus From(string value)
    {
        if (!_all.TryGetValue(value, out var status))
            throw new ArgumentException($"'{value}' is not a valid ModelStatus.", nameof(value));
        return status;
    }

    /// <summary>Attempts to parse the given string into a <see cref="ModelStatus"/>.</summary>
    public static bool TryParse(string value, out ModelStatus? result) =>
        _all.TryGetValue(value, out result!);

    /// <inheritdoc />
    public bool Equals(ModelStatus? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ModelStatus other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator.</summary>
    public static bool operator ==(ModelStatus? left, ModelStatus? right) => left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(ModelStatus? left, ModelStatus? right) => !(left == right);
}
