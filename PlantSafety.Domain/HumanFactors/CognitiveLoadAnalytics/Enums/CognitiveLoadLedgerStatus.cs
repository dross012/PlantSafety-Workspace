// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CognitiveLoadLedger aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;

/// <summary>Lifecycle states for the CognitiveLoadLedger aggregate root.</summary>
public enum CognitiveLoadLedgerStatus
{
    /// <summary>Ledger is open for the current shift and accepting new readings/profiles/trend recalculations.</summary>
    Open = 0,

    /// <summary>The shift has ended; the ledger is closed to new records pending retention review.</summary>
    Closed = 1,

    /// <summary>Ledger has been archived per the HR data-retention policy (active employment + defined window).</summary>
    Archived = 2,
}
