// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           AuditingItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Auditing record. Backs
//                  GetAuditingByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Auditing.Models;

/// <summary>A single Auditing read-model record.</summary>
public sealed record AuditingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AuditingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable audit program/execution title.</summary>
    public required string AuditTitle { get; init; }

    /// <summary>Audit classification: Internal, External, Regulatory, Supplier.</summary>
    public required string AuditType { get; init; }

    /// <summary>Narrative scope of the audit.</summary>
    public required string Scope { get; init; }

    /// <summary>Identity of the lead auditor assigned to this audit.</summary>
    public Guid? AuditorId { get; init; }

    /// <summary>Scheduled execution date/time for the audit, if planned.</summary>
    public DateTime? ScheduledAtUtc { get; init; }

    /// <summary>UTC timestamp the audit was completed, if finished.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Count of findings raised during this audit.</summary>
    public int FindingsCount { get; init; }

    /// <summary>Computed compliance score (0-100) for this audit, when available.</summary>
    public decimal? Score { get; init; }

    /// <summary>Overall compliance rating classification.</summary>
    public required string ComplianceRating { get; init; }

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
