// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PPEZoneMapping record. PPE zone mapping: mandatory PPE zones, restricted areas, chemical/noise/hot-work zones, access control integration.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.PPEZoneMapping.Models;

/// <summary>A single PPEZoneMapping read-model record.</summary>
public sealed record PPEZoneMappingItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PPEZoneMappingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>ZoneCode.</summary>
    public required  string ZoneCode { get; init; }

    /// <summary>RequiredPpeCategories.</summary>
    public required  string RequiredPpeCategories { get; init; }

    /// <summary>AccessControlIntegrated.</summary>
    public required  bool AccessControlIntegrated { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}