/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               RiskLevel.cs
    DESCRIPTION:        SAFETY-CRITICAL Smart Enum representing the assessed risk level of a safety prediction.
                        Drives alerting thresholds and human escalation policies. Critical and High risk levels
                        require dual validation before any automated action. (Rule 2.7, Safety Rules 4.4–4.6)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Severity values must never be reordered or lowered.
==========================================================================================================================*/

namespace Intelligence.Domain.PredictiveSafety.ValueObjects;

/// <summary>
/// SAFETY-CRITICAL: Smart Enum representing the assessed risk level of a safety prediction.
/// <see cref="Critical"/> and <see cref="High"/> risk predictions require dual validation
/// before any automated action is permitted (Safety Rule 4.4).
/// </summary>
public sealed class RiskLevel : IEquatable<RiskLevel>
{
    /// <summary>Negligible risk — informational only. No automated action.</summary>
    public static readonly RiskLevel Low = new("Low", 1, requiresDualValidation: false, triggersAlert: false);

    /// <summary>Elevated risk — monitoring recommended. No automated action without confidence floor.</summary>
    public static readonly RiskLevel Medium = new("Medium", 2, requiresDualValidation: false, triggersAlert: false);

    /// <summary>High risk — immediate review required. Automated alert triggered.</summary>
    public static readonly RiskLevel High = new("High", 3, requiresDualValidation: true, triggersAlert: true);

    /// <summary>Critical risk — mandatory human escalation. Dual validation required. Automated alert triggered.</summary>
    public static readonly RiskLevel Critical = new("Critical", 4, requiresDualValidation: true, triggersAlert: true);

    private static readonly Dictionary<string, RiskLevel> _all = new(StringComparer.OrdinalIgnoreCase)
    {
        [Low.Value] = Low,
        [Medium.Value] = Medium,
        [High.Value] = High,
        [Critical.Value] = Critical
    };

    private RiskLevel(string value, int severity, bool requiresDualValidation, bool triggersAlert)
    {
        Value = value;
        Severity = severity;
        RequiresDualValidation = requiresDualValidation;
        TriggersAlert = triggersAlert;
    }

    /// <summary>The string value of this risk level.</summary>
    public string Value { get; }

    /// <summary>Ordinal severity (1 = lowest, 4 = highest). Used for comparison.</summary>
    public int Severity { get; }

    /// <summary>
    /// SAFETY-CRITICAL: Whether predictions at this risk level require dual validation
    /// before any automated safety action (Safety Rule 4.4).
    /// </summary>
    public bool RequiresDualValidation { get; }

    /// <summary>Whether predictions at this risk level should trigger an automated safety alert.</summary>
    public bool TriggersAlert { get; }

    /// <summary>All defined risk levels, ordered from lowest to highest severity.</summary>
    public static IReadOnlyCollection<RiskLevel> All => _all.Values;

    /// <summary>The default risk level for new unclassified predictions.</summary>
    public static RiskLevel Default => Low;

    /// <summary>Returns the <see cref="RiskLevel"/> for the given string value.</summary>
    /// <exception cref="ArgumentException">Thrown when value is not a known risk level.</exception>
    public static RiskLevel From(string value)
    {
        if (!_all.TryGetValue(value, out var level))
            throw new ArgumentException($"'{value}' is not a valid RiskLevel.", nameof(value));
        return level;
    }

    /// <summary>Attempts to parse the given string into a <see cref="RiskLevel"/>.</summary>
    public static bool TryParse(string value, out RiskLevel? result) =>
        _all.TryGetValue(value, out result!);

    /// <summary>Returns true when this risk level is at least as severe as the other.</summary>
    public bool IsAtLeastAsSevereAs(RiskLevel other) =>
        Severity >= (other?.Severity ?? 0);

    /// <inheritdoc />
    public bool Equals(RiskLevel? other) =>
        other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is RiskLevel other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>Equality operator.</summary>
    public static bool operator ==(RiskLevel? left, RiskLevel? right) =>
        left?.Equals(right) ?? right is null;

    /// <summary>Inequality operator.</summary>
    public static bool operator !=(RiskLevel? left, RiskLevel? right) =>
        !(left == right);
}
