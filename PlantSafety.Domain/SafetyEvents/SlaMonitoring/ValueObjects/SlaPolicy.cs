// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.SlaMonitoring
//  Layer:          Domain
//  File:           SlaPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Value object defining the acknowledge/resolve SLA targets for a single
//                  incident severity tier. Persisted to [SafetyEvents].[IncidentSlaPolicy] and
//                  seeded with a sensible default set (no admin authoring UI exists yet for this
//                  genuinely new domain -- see UI Gaps Scoping, IncidentManagement Part 2).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.SlaMonitoring.ValueObjects;

/// <summary>Defines the acknowledge/resolve SLA targets for a single incident severity tier.</summary>
public readonly record struct SlaPolicy
{
    /// <summary>Severity this policy applies to (Minor, Moderate, Serious, Critical).</summary>
    public string Severity { get; }

    /// <summary>Target time to first assignment/acknowledgement.</summary>
    public TimeSpan AcknowledgeWithin { get; }

    /// <summary>Target time to resolution.</summary>
    public TimeSpan ResolveWithin { get; }

    /// <summary>Creates a new SLA policy for a severity tier.</summary>
    public SlaPolicy(string severity, TimeSpan acknowledgeWithin, TimeSpan resolveWithin)
    {
        if (string.IsNullOrWhiteSpace(severity))
            throw new ArgumentException("Severity is required.", nameof(severity));
        if (acknowledgeWithin <= TimeSpan.Zero)
            throw new ArgumentException("AcknowledgeWithin must be positive.", nameof(acknowledgeWithin));
        if (resolveWithin <= TimeSpan.Zero)
            throw new ArgumentException("ResolveWithin must be positive.", nameof(resolveWithin));

        Severity = severity;
        AcknowledgeWithin = acknowledgeWithin;
        ResolveWithin = resolveWithin;
    }

    /// <summary>
    /// Sensible platform-default policy set, used when no persisted policy row exists yet for a
    /// given severity (defence-in-depth against an empty/partially-seeded policy table).
    /// </summary>
    public static SlaPolicy Default(string severity) => (severity ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "critical" => new SlaPolicy("Critical", TimeSpan.FromMinutes(15), TimeSpan.FromHours(4)),
        "serious" => new SlaPolicy("Serious", TimeSpan.FromMinutes(30), TimeSpan.FromHours(8)),
        "moderate" => new SlaPolicy("Moderate", TimeSpan.FromHours(1), TimeSpan.FromHours(24)),
        _ => new SlaPolicy("Minor", TimeSpan.FromHours(4), TimeSpan.FromHours(72)),
    };
}
