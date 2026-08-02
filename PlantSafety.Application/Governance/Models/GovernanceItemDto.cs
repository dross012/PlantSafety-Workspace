// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           GovernanceItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Governance record. Backs
//                  GetGovernanceByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Governance.Models;

/// <summary>A single Governance read-model record.</summary>
public sealed record GovernanceItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid GovernanceId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Governance area (e.g. PolicyOversight, RiskGovernance, RegulatoryLiaison).</summary>
    public required string GovernanceArea { get; init; }

    /// <summary>Optional identifier of a linked policy record.</summary>
    public Guid? PolicyReferenceId { get; init; }

    /// <summary>Cadence, in months, at which this governance record must be reviewed.</summary>
    public int ReviewCycleMonths { get; init; }

    /// <summary>Identity of the user accountable for this governance record.</summary>
    public Guid? OwnerUserId { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
