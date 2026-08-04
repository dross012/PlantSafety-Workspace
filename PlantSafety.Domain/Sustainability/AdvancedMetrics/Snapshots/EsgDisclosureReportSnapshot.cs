// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EsgDisclosureReport aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Snapshots;

/// <summary>Immutable snapshot of an <see cref="Aggregates.EsgDisclosureReport"/> aggregate for fast rehydration.</summary>
public sealed record EsgDisclosureReportSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EsgDisclosureReportId EsgDisclosureReportId { get; init; }

    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Owning site, if site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Report version number.</summary>
    public int Version { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EsgDisclosureReportStatus Status { get; init; }

    /// <summary>UTC generation timestamp.</summary>
    public UtcTimestamp GeneratedAtUtc { get; init; }

    /// <summary>Identity of the user who generated this record.</summary>
    public UserId GeneratedBy { get; init; }
}
