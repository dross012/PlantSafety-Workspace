// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Application
//  File:           NearMissesDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single NearMisses record, including every
//                  aggregate property plus an ETag. Backs GetNearMissesByIdQuery. Distinct from
//                  the pre-existing, simpler NearMissesItemDto used by the list-range query
//                  (GetNearMissesQuery), which is left untouched. Named "...DetailDto" rather than
//                  the "<Name>ItemDto" convention used elsewhere in this Phase 4 pass because
//                  "NearMissesItemDto" was already taken by the pre-existing list DTO -- a
//                  documented, deliberate naming deviation. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Models
//                  .PredictiveMaintenanceAlertItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.NearMisses.Models;

/// <summary>A single NearMisses read-model record, including the full lifecycle state.</summary>
public sealed record NearMissesDetailDto
{
    /// <summary>Record identifier.</summary>
    public required Guid NearMissesId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Reported/UnderReview/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Hazard category (e.g., Slip/Trip/Fall, EquipmentFailure, ChemicalExposure).</summary>
    public required string Category { get; init; }

    /// <summary>Potential severity had the event resulted in an actual incident.</summary>
    public required string PotentialSeverity { get; init; }

    /// <summary>UTC instant the near-miss was reported.</summary>
    public required DateTime ReportedAtUtc { get; init; }

    /// <summary>Identity of the reviewer who started reviewing this near-miss.</summary>
    public Guid? ReviewStartedBy { get; init; }

    /// <summary>UTC instant the review started.</summary>
    public DateTime? ReviewStartedAtUtc { get; init; }

    /// <summary>Identity of the user who completed the review.</summary>
    public Guid? CompletedBy { get; init; }

    /// <summary>Free-text findings recorded when the review is completed.</summary>
    public string? Findings { get; init; }

    /// <summary>UTC instant the review was completed.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed this near-miss.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>UTC instant the near-miss was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the near-miss was archived for compliance retention.</summary>
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
