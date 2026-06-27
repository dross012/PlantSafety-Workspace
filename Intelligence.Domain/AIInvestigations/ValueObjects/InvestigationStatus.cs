/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               InvestigationStatus.cs
    DESCRIPTION:        Smart Enum for Investigation lifecycle status. Drives workflow transitions on the
                        Investigation aggregate. Terminal statuses cannot transition further. (Rule 2.7)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace Intelligence.Domain.AIInvestigations.ValueObjects;

/// <summary>Smart Enum representing the lifecycle status of an AI investigation.</summary>
public sealed class InvestigationStatus : IEquatable<InvestigationStatus>
{
    /// <summary>Investigation has been opened and is awaiting assignment.</summary>
    public static readonly InvestigationStatus Open = new("Open", 1, isTerminal: false);

    /// <summary>Investigation is actively being worked.</summary>
    public static readonly InvestigationStatus InProgress = new("InProgress", 2, isTerminal: false);

    /// <summary>Investigation findings have been submitted for human review.</summary>
    public static readonly InvestigationStatus PendingReview = new("PendingReview", 3, isTerminal: false);

    /// <summary>Human reviewer has accepted the investigation findings.</summary>
    public static readonly InvestigationStatus Completed = new("Completed", 4, isTerminal: true);

    /// <summary>Investigation was closed without a completed review.</summary>
    public static readonly InvestigationStatus Closed = new("Closed", 5, isTerminal: true);

    /// <summary>Investigation has been archived for long-term retention.</summary>
    public static readonly InvestigationStatus Archived = new("Archived", 6, isTerminal: true);

    private static readonly Dictionary<string, InvestigationStatus> _all = new(StringComparer.OrdinalIgnoreCase)
    {
        [Open.Value] = Open,
        [InProgress.Value] = InProgress,
        [PendingReview.Value] = PendingReview,
        [Completed.Value] = Completed,
        [Closed.Value] = Closed,
        [Archived.Value] = Archived
    };

    private InvestigationStatus(string value, int order, bool isTerminal)
    {
        Value = value;
        Order = order;
        IsTerminal = isTerminal;
    }

    /// <summary>The string value of this status.</summary>
    public string Value { get; }

    /// <summary>Ordinal position in the lifecycle sequence.</summary>
    public int Order { get; }

    /// <summary>Whether this status is terminal (no further transitions permitted).</summary>
    public bool IsTerminal { get; }

    /// <summary>The default status for a newly opened investigation.</summary>
    public static InvestigationStatus Default => Open;

    /// <summary>All defined statuses.</summary>
    public static IReadOnlyCollection<InvestigationStatus> All => _all.Values;

    /// <summary>Returns the status for the given string value.</summary>
    /// <exception cref="ArgumentException">Thrown when value is not a known status.</exception>
    public static InvestigationStatus From(string value)
    {
        if (!_all.TryGetValue(value, out var status))
            throw new ArgumentException($"'{value}' is not a valid InvestigationStatus.", nameof(value));
        return status;
    }

    /// <summary>Attempts to parse the given string into an <see cref="InvestigationStatus"/>.</summary>
    public static bool TryParse(string value, out InvestigationStatus? result) =>
        _all.TryGetValue(value, out result!);

    /// <inheritdoc />
    public bool Equals(InvestigationStatus? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is InvestigationStatus other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator.</summary>
    public static bool operator ==(InvestigationStatus? left, InvestigationStatus? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(InvestigationStatus? left, InvestigationStatus? right) =>
        !(left == right);
}
