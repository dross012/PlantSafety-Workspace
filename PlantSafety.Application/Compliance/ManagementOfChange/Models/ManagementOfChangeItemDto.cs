// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           ManagementOfChangeItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single ManagementOfChange record. Backs
//                  GetManagementOfChangeByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.ManagementOfChange.Models;

/// <summary>A single ManagementOfChange read-model record.</summary>
public sealed record ManagementOfChangeItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ManagementOfChangeId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable title of the proposed change.</summary>
    public required string ChangeTitle { get; init; }

    /// <summary>Change classification (Process, Equipment, Personnel, Procedure, Software).</summary>
    public required string ChangeType { get; init; }

    /// <summary>Business/safety justification for the change.</summary>
    public required string Justification { get; init; }

    /// <summary>Risk level of the change (Low, Medium, High, Critical).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Identity of the user who requested the change.</summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>Identity of the user who approved the change, if approved.</summary>
    public Guid? ApprovedByUserId { get; init; }

    /// <summary>Approval status (Pending, Approved, Rejected, Deferred).</summary>
    public required string ApprovalStatus { get; init; }

    /// <summary>Planned/actual implementation date for the change.</summary>
    public DateTime? ImplementationAtUtc { get; init; }

    /// <summary>Scheduled post-implementation review date.</summary>
    public DateTime? ReviewDueAtUtc { get; init; }

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
