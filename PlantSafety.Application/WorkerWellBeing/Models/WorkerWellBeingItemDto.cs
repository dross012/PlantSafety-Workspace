// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           WorkerWellBeingItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single WorkerWellBeing record. Backs
//                  GetWorkerWellBeingByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.WorkerWellBeing.Models;

/// <summary>A single WorkerWellBeing read-model record.</summary>
public sealed record WorkerWellBeingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid WorkerWellBeingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Worker this check-in record relates to.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Wellbeing risk classification: Low, Medium, High, Critical.</summary>
    public required string WellBeingRiskLevel { get; init; }

    /// <summary>Check-in classification (e.g., MentalHealth, PhysicalHealth, WorkLifeBalance).</summary>
    public required string CheckInType { get; init; }

    /// <summary>Self-reported or assessed wellbeing score, 1-10.</summary>
    public required int WellBeingScore { get; init; }

    /// <summary>Whether this check-in requires follow-up.</summary>
    public required bool FollowUpRequired { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

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
