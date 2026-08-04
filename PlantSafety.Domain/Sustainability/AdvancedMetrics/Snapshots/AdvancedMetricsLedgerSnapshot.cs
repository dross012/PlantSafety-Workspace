// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedgerSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for AdvancedMetricsLedger aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.AdvancedMetricsLedger"/> aggregate for fast rehydration.</summary>
public sealed record AdvancedMetricsLedgerSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AdvancedMetricsLedgerId AdvancedMetricsLedgerId { get; init; }

    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Owning site, if site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Reporting period start.</summary>
    public UtcTimestamp ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end.</summary>
    public UtcTimestamp ReportingPeriodEndUtc { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AdvancedMetricsLedgerStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
