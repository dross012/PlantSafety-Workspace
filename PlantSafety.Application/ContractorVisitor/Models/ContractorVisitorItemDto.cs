// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Application
//  File:           ContractorVisitorItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single ContractorVisitor record. Backs
//                  GetContractorVisitorByIdQuery. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ContractorVisitor.Models;

/// <summary>A single ContractorVisitor read-model record.</summary>
public sealed record ContractorVisitorItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ContractorVisitorId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Full name of the contractor or visitor.</summary>
    public required string FullName { get; init; }

    /// <summary>Company name the contractor or visitor represents.</summary>
    public required string CompanyName { get; init; }

    /// <summary>Visitor classification: Contractor or Visitor.</summary>
    public required string VisitorType { get; init; }

    /// <summary>Narrative purpose of the visit.</summary>
    public required string PurposeOfVisit { get; init; }

    /// <summary>Identity of the host user sponsoring this visit.</summary>
    public Guid? HostUserId { get; init; }

    /// <summary>Scheduled arrival timestamp, if planned.</summary>
    public DateTime? ScheduledArrivalUtc { get; init; }

    /// <summary>Actual arrival timestamp, populated on check-in.</summary>
    public DateTime? ActualArrivalUtc { get; init; }

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
