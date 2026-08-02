// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Application
//  File:           ProceduresItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Procedures record. Backs
//                  GetProceduresByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Procedures.Models;

/// <summary>A single Procedures read-model record.</summary>
public sealed record ProceduresItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ProceduresId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable procedure title (e.g., "Confined Space Entry SOP").</summary>
    public required string ProcedureTitle { get; init; }

    /// <summary>Procedure document number/reference.</summary>
    public required string ProcedureNumber { get; init; }

    /// <summary>Procedure category (e.g., LOTO, HotWork, ConfinedSpace, EmergencyResponse).</summary>
    public required string Category { get; init; }

    /// <summary>Monotonically increasing revision number, starting at 1.</summary>
    public required int RevisionNumber { get; init; }

    /// <summary>Identity of the accountable procedure owner.</summary>
    public Guid? OwnerId { get; init; }

    /// <summary>UTC timestamp the procedure becomes effective.</summary>
    public DateTime? EffectiveAtUtc { get; init; }

    /// <summary>UTC timestamp the procedure is next due for review.</summary>
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
