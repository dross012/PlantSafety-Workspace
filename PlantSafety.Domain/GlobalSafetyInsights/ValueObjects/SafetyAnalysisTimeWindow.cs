// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           SafetyAnalysisTimeWindow.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Value object representing the inclusive [Start, End] UTC analysis window a
//                  GlobalSafetyInsightsAggregate snapshot was computed over. Named
//                  "SafetyAnalysisTimeWindow" rather than the bare "TimeWindow" to avoid colliding
//                  with any pre-existing scheduling-oriented TimeWindow concept elsewhere in the
//                  platform. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.ValueObjects;

/// <summary>Immutable inclusive UTC [Start, End] analysis window.</summary>
public readonly record struct SafetyAnalysisTimeWindow
{
    /// <summary>Inclusive UTC start of the analysis window.</summary>
    public UtcTimestamp StartUtc { get; }

    /// <summary>Inclusive UTC end of the analysis window.</summary>
    public UtcTimestamp EndUtc { get; }

    /// <summary>Creates a validated analysis window. <paramref name="startUtc"/> must be strictly before <paramref name="endUtc"/>.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startUtc"/> is not before <paramref name="endUtc"/>.</exception>
    public SafetyAnalysisTimeWindow(UtcTimestamp startUtc, UtcTimestamp endUtc)
    {
        if (startUtc.Value >= endUtc.Value)
            throw new ArgumentException(
                $"SafetyAnalysisTimeWindow start ({startUtc}) must be strictly before end ({endUtc}).", nameof(startUtc));

        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    /// <summary>Total duration spanned by this window.</summary>
    public TimeSpan Duration => EndUtc.Value - StartUtc.Value;

    /// <inheritdoc/>
    public override string ToString() => $"[{StartUtc:O} .. {EndUtc:O}]";
}
