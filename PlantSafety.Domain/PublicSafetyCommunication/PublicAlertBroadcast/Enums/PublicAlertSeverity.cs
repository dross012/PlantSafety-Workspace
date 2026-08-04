// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.PublicAlertBroadcast
//  Layer:          Domain
//  File:           PublicAlertSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart enum modelling public alert severity levels, aligned to the Common
//                  Alerting Protocol (CAP) severity vocabulary used by EAS/WEA/IPAWS. Carries
//                  workflow metadata (numeric rank, requires-confirmation flag) per the Enums
//                  Rule (2.7): Smart Enums are required for domain concepts with business rules
//                  or metadata that drive workflow.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;

/// <summary>Smart enum for public alert severity levels (CAP-aligned).</summary>
public sealed class PublicAlertSeverity : IEquatable<PublicAlertSeverity>
{
    /// <summary>Stable string code for persistence and API transport.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Numeric rank; higher is more severe.</summary>
    public int Rank { get; }

    /// <summary>Whether a broadcast at this severity requires post-send delivery confirmation.</summary>
    public bool RequiresDeliveryConfirmation { get; }

    private PublicAlertSeverity(string code, string displayName, int rank, bool requiresDeliveryConfirmation)
    {
        Code = code;
        DisplayName = displayName;
        Rank = rank;
        RequiresDeliveryConfirmation = requiresDeliveryConfirmation;
    }

    /// <summary>Informational notice; no immediate threat.</summary>
    public static readonly PublicAlertSeverity Advisory = new("Advisory", "Advisory", rank: 1, requiresDeliveryConfirmation: false);

    /// <summary>Conditions favorable for a hazardous event; monitor closely.</summary>
    public static readonly PublicAlertSeverity Watch = new("Watch", "Watch", rank: 2, requiresDeliveryConfirmation: false);

    /// <summary>Hazardous event is imminent or occurring; take action.</summary>
    public static readonly PublicAlertSeverity Warning = new("Warning", "Warning", rank: 3, requiresDeliveryConfirmation: true);

    /// <summary>Extreme, life-threatening event requiring immediate public action.</summary>
    public static readonly PublicAlertSeverity Emergency = new("Emergency", "Emergency", rank: 4, requiresDeliveryConfirmation: true);

    /// <summary>All defined severities.</summary>
    public static IReadOnlyCollection<PublicAlertSeverity> All { get; } = new[] { Advisory, Watch, Warning, Emergency };

    /// <summary>Default severity used when none is specified.</summary>
    public static PublicAlertSeverity Default => Advisory;

    /// <summary>Resolves a severity from its stable code. Throws if unknown.</summary>
    public static PublicAlertSeverity From(string code) =>
        TryParse(code, out var value) ? value! : throw new ArgumentException($"Unknown PublicAlertSeverity code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a severity from its stable code.</summary>
    public static bool TryParse(string? code, out PublicAlertSeverity? value)
    {
        value = All.FirstOrDefault(s => string.Equals(s.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised severity code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(PublicAlertSeverity? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as PublicAlertSeverity);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
