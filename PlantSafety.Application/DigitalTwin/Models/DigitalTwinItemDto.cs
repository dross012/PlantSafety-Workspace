// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           DigitalTwinItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single DigitalTwin record. Backs
//                  GetDigitalTwinByIdQuery. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.DigitalTwin.Models;

/// <summary>A single DigitalTwin read-model record.</summary>
public sealed record DigitalTwinItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DigitalTwinId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Name of the digital twin.</summary>
    public required string TwinName { get; init; }

    /// <summary>Identity of the physical asset this twin represents, if known.</summary>
    public Guid? AssetReferenceId { get; init; }

    /// <summary>Type of the underlying digital-twin model.</summary>
    public required string ModelType { get; init; }

    /// <summary>UTC timestamp of the last successful synchronization.</summary>
    public DateTime? LastSyncedAtUtc { get; init; }

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
