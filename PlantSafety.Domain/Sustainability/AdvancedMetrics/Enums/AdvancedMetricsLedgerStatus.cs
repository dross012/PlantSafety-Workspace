// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedgerStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AdvancedMetricsLedger aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

/// <summary>Lifecycle states for the AdvancedMetricsLedger aggregate root.</summary>
public enum AdvancedMetricsLedgerStatus
{
    /// <summary>Ledger is open for the current reporting period and accepting new records.</summary>
    Open = 0,

    /// <summary>Reporting period has ended; the ledger is closed to new records pending disclosure.</summary>
    Closed = 1,

    /// <summary>Ledger has been archived for long-term compliance retention.</summary>
    Archived = 2,
}
