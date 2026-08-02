// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           CrisisManagementDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single CrisisManagement record, including every
//                  aggregate property plus an ETag. Backs GetCrisisManagementByIdQuery. Distinct
//                  from the pre-existing, simpler CrisisManagementItemDto used by the list-range
//                  query (GetCrisisManagementQuery), which is left untouched. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Models;

/// <summary>A single CrisisManagement read-model record.</summary>
public sealed record CrisisManagementDetailDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CrisisManagementId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Declared/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Short crisis title.</summary>
    public required string Title { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>UTC timestamp when the crisis was declared.</summary>
    public required DateTime DeclaredAtUtc { get; init; }

    /// <summary>Identity of the user who responded to the crisis.</summary>
    public Guid? RespondedBy { get; init; }

    /// <summary>UTC instant the crisis was responded to.</summary>
    public DateTime? RespondedAtUtc { get; init; }

    /// <summary>UTC instant the crisis was reported stabilized.</summary>
    public DateTime? StabilizedAtUtc { get; init; }

    /// <summary>Identity of the user who resolved the crisis.</summary>
    public Guid? ResolvedBy { get; init; }

    /// <summary>UTC instant the crisis was resolved.</summary>
    public DateTime? ResolvedAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed the crisis.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>UTC instant the crisis was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the crisis record was archived.</summary>
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
