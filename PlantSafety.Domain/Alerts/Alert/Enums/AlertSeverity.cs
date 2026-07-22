// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertSeverity.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for "13. Alert Severity Levels" (Critical/High/Medium/Low) from the
//                  source doc. Carries a numeric priority rank and escalation-eligibility metadata
//                  used by routing/escalation logic — satisfying the Smart Enum requirement (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.Enums;

/// <summary>Classifies the operational priority of an <see cref="Aggregates.Alert"/>.</summary>
public sealed class AlertSeverity : IEquatable<AlertSeverity>, IComparable<AlertSeverity>
{
    /// <summary>Canonical string code (e.g. "Critical").</summary>
    public string Code { get; }

    /// <summary>Numeric priority rank; higher value = more severe.</summary>
    public int Rank { get; }

    /// <summary>Human-readable description matching the source catalog wording.</summary>
    public string Description { get; }

    /// <summary>Maximum time, in minutes, an unacknowledged alert of this severity may remain open before auto-escalation.</summary>
    public int AutoEscalationThresholdMinutes { get; }

    private AlertSeverity(string code, int rank, string description, int autoEscalationThresholdMinutes)
    {
        Code = code;
        Rank = rank;
        Description = description;
        AutoEscalationThresholdMinutes = autoEscalationThresholdMinutes;
    }

    /// <summary>Immediate danger to life. Overrides all other notifications.</summary>
    public static readonly AlertSeverity Critical = new("Critical", 4, "Immediate danger to life", 2);

    /// <summary>Severe hazard requiring rapid response.</summary>
    public static readonly AlertSeverity High = new("High", 3, "Severe hazard requiring rapid response", 10);

    /// <summary>Operational or compliance issue.</summary>
    public static readonly AlertSeverity Medium = new("Medium", 2, "Operational or compliance issue", 60);

    /// <summary>Advisory or informational.</summary>
    public static readonly AlertSeverity Low = new("Low", 1, "Advisory or informational", 480);

    /// <summary>All defined severities, from most to least severe.</summary>
    public static readonly IReadOnlyList<AlertSeverity> All = new[] { Critical, High, Medium, Low };

    /// <summary>Default severity assigned when none is specified.</summary>
    public static readonly AlertSeverity Default = Medium;

    /// <summary>Resolves an <see cref="AlertSeverity"/> from its canonical code. Throws when unknown.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known severity.</exception>
    public static AlertSeverity From(string code)
        => TryParse(code, out var severity)
            ? severity!
            : throw new ArgumentException($"Unknown AlertSeverity code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an <see cref="AlertSeverity"/> from its canonical code.</summary>
    public static bool TryParse(string? code, out AlertSeverity? severity)
    {
        severity = All.FirstOrDefault(s => string.Equals(s.Code, code, StringComparison.OrdinalIgnoreCase));
        return severity is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> matches a known severity.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public int CompareTo(AlertSeverity? other) => other is null ? 1 : Rank.CompareTo(other.Rank);
    /// <inheritdoc/>
    public bool Equals(AlertSeverity? other) => other is not null && Code == other.Code;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AlertSeverity);
    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    /// <inheritdoc/>
    public override string ToString() => Code;
}
