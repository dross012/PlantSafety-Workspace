// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions and mutation windows for the
//                  CognitiveLoadLedger aggregate. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.CognitiveLoadLedger"/> aggregate.</summary>
public static class CognitiveLoadLedgerLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when new readings/profiles/trend recalculations may be recorded while the ledger is in <paramref name="current"/>.</summary>
    public static bool CanRecord(CognitiveLoadLedgerStatus current) => current == CognitiveLoadLedgerStatus.Open;

    /// <summary>Returns <see langword="true"/> when the ledger may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(CognitiveLoadLedgerStatus current) => current == CognitiveLoadLedgerStatus.Open;

    /// <summary>Returns <see langword="true"/> when the ledger may be archived from <paramref name="current"/> (per the HR retention window policy).</summary>
    public static bool CanArchive(CognitiveLoadLedgerStatus current) => current == CognitiveLoadLedgerStatus.Closed;
}
