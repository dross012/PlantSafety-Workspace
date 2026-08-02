// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           ProcessDefinitionItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single ProcessDefinition record. Backs
//                  GetProcessDefinitionByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Models;

/// <summary>A single ProcessDefinition read-model record.</summary>
public sealed record ProcessDefinitionItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ProcessDefinitionId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable process name.</summary>
    public required string ProcessName { get; init; }

    /// <summary>Safety-critical process category.</summary>
    public required string Category { get; init; }

    /// <summary>Current lifecycle status (Draft/UnderReview/Active/Retired/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Monotonically increasing revision number.</summary>
    public required int Version { get; init; }

    /// <summary>Identity of the accountable process owner.</summary>
    public Guid? OwnerId { get; init; }

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
