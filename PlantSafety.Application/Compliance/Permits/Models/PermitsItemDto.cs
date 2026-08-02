// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           PermitsItemDto.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Permits record. Backs both
//                  GetPermitsByIdQuery and GetPermitsQuery. Mirrors
//                  PlantSafety.Application.PermitToWork.PermitWorkflow.Models.PermitWorkflowItemDto /
//                  RiskItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Permits.Models;

/// <summary>A single Permits read-model record.</summary>
public sealed record PermitsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PermitsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived/Rejected/Expired).</summary>
    public required string Status { get; init; }

    /// <summary>Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</summary>
    public required string PermitType { get; init; }

    /// <summary>Human-readable permit number/reference.</summary>
    public required string PermitNumber { get; init; }

    /// <summary>Description of the work covered by this permit.</summary>
    public required string WorkDescription { get; init; }

    /// <summary>Physical location the permit applies to.</summary>
    public required string Location { get; init; }

    /// <summary>Hazards identified for this permitted work.</summary>
    public required string HazardsIdentified { get; init; }

    /// <summary>Identity of the user who requested the permit.</summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>Identity of the user who approved/issued the permit, if issued.</summary>
    public Guid? ApprovedByUserId { get; init; }

    /// <summary>UTC timestamp the permit was issued.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp the permit expires.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>Reason recorded when a permit request is rejected.</summary>
    public string? RejectedReason { get; init; }

    /// <summary>UTC instant the permit request was rejected.</summary>
    public DateTime? RejectedAtUtc { get; init; }

    /// <summary>UTC instant the permitted work was marked completed.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>UTC instant an issued permit expired without being completed/closed.</summary>
    public DateTime? ExpiredAtUtc { get; init; }

    /// <summary>UTC instant the permit was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the permit was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

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
