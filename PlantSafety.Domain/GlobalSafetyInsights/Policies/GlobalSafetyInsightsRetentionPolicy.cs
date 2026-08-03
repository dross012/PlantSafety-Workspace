// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyInsightsRetentionPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Pure policy determining how long computed GlobalSafetyInsights snapshots are
//                  retained before they become eligible for archival/purge by the platform's
//                  existing Operations.Archival job. Mirrors the *ArchivalPolicy convention used
//                  by every other module (e.g. RiskArchivalPolicy). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Policies;

/// <summary>Determines retention/archival eligibility for a computed GlobalSafetyInsights snapshot.</summary>
public sealed class GlobalSafetyInsightsRetentionPolicy
{
    /// <summary>Default retention window for a computed snapshot before it is archival-eligible.</summary>
    public static readonly TimeSpan DefaultRetentionWindow = TimeSpan.FromDays(400);

    /// <summary>Returns <see langword="true"/> when a snapshot computed at <paramref name="computedAtUtc"/> is eligible for archival as of <paramref name="nowUtc"/>.</summary>
    public bool IsArchivalEligible(UtcTimestamp computedAtUtc, UtcTimestamp nowUtc)
        => nowUtc.Value - computedAtUtc.Value >= DefaultRetentionWindow;
}
