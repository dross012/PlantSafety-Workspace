// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           EnterpriseFabricItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single EnterpriseFabric record. Backs
//                  GetEnterpriseFabricByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.EnterpriseFabric.Models;

/// <summary>A single EnterpriseFabric read-model record.</summary>
public sealed record EnterpriseFabricItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EnterpriseFabricId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Integration area classification.</summary>
    public required string FabricArea { get; init; }

    /// <summary>External system/endpoint name.</summary>
    public required string SystemReference { get; init; }

    /// <summary>Integration mechanism (Api/EventBus/FileTransfer/Database).</summary>
    public required string IntegrationType { get; init; }

    /// <summary>Identity of the user who owns this integration record.</summary>
    public required Guid OwnerUserId { get; init; }

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
