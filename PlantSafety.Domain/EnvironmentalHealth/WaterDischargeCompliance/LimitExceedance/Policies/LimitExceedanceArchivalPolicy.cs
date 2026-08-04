// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for LimitExceedance aggregates. Aligned
//                  with the water discharge compliance data-governance rule that regulatory
//                  submissions and sample chain-of-custody records must be retained for a minimum
//                  of 5 years before archival is considered. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Policies;

/// <summary>Determines whether a <see cref="LimitExceedance"/> aggregate is eligible for archival.</summary>
public static class LimitExceedanceArchivalPolicy
{
    /// <summary>Minimum retention period, in years, before a terminal-state record may be archived.</summary>
    public const int MinimumRetentionYears = 5;

    /// <summary>Returns <see langword="true"/> when the record is in a terminal lifecycle state and has
    /// been retained for at least <see cref="MinimumRetentionYears"/> since creation.</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance aggregate, DateTime nowUtc)
        => LimitExceedanceLifecycleRules.IsTerminal(aggregate.Status)
           && aggregate.CreatedAtUtc.Value.AddYears(MinimumRetentionYears) <= nowUtc;
}