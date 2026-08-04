// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for AdvancedMetricsLedger
//                  aggregates. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Policies;

/// <summary>Determines whether an <see cref="Aggregates.AdvancedMetricsLedger"/> is eligible for archival.</summary>
public static class AdvancedMetricsArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the ledger record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(Aggregates.AdvancedMetricsLedger aggregate)
        => aggregate.Status == AdvancedMetricsLedgerStatus.Closed;
}
