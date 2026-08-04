// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedgerLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions and mutation windows for the
//                  AdvancedMetricsLedger aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.AdvancedMetricsLedger"/> aggregate.</summary>
public static class AdvancedMetricsLedgerLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when new metric records may be added while the ledger is in <paramref name="current"/>.</summary>
    public static bool CanRecordMetrics(AdvancedMetricsLedgerStatus current) => current == AdvancedMetricsLedgerStatus.Open;

    /// <summary>Returns <see langword="true"/> when the ledger may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AdvancedMetricsLedgerStatus current) => current == AdvancedMetricsLedgerStatus.Open;

    /// <summary>Returns <see langword="true"/> when the ledger may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AdvancedMetricsLedgerStatus current) => current == AdvancedMetricsLedgerStatus.Closed;
}
