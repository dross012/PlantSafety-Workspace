// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for CognitiveLoadLedger aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Snapshots;

/// <summary>Immutable snapshot of a <see cref="Aggregates.CognitiveLoadLedger"/> aggregate for fast rehydration.</summary>
public sealed record CognitiveLoadLedgerSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public CognitiveLoadLedgerId CognitiveLoadLedgerId { get; init; }

    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Owning site, if site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Shift classification.</summary>
    public ShiftType ShiftType { get; init; }

    /// <summary>UTC start-of-shift instant.</summary>
    public UtcTimestamp ShiftDateUtc { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public CognitiveLoadLedgerStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
