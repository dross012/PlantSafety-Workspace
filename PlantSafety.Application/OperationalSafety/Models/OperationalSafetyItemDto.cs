// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           OperationalSafetyItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single OperationalSafety record. Backs
//                  GetOperationalSafetyByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.OperationalSafety.Models;

/// <summary>A single OperationalSafety read-model record.</summary>
public sealed record OperationalSafetyItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid OperationalSafetyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Operational-safety domain (e.g. LineOfFire, WorkAtHeight, MobileEquipment).</summary>
    public required string SafetyDomain { get; init; }

    /// <summary>Risk level classification: Low, Medium, High, Critical.</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Identity of the user accountable for this oversight record.</summary>
    public Guid? OwnerUserId { get; init; }

    /// <summary>UTC timestamp by which this record is next due for review, if scheduled.</summary>
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
