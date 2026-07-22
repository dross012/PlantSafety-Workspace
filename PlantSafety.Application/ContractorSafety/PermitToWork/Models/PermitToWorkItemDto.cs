// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PermitToWork record.
// SchemaVersion: 1

namespace PlantSafety.Application.ContractorSafety.PermitToWork.Models;

/// <summary>A single PermitToWork record (Permit-to-Work (PTW) systems formally authorize high-risk contractor tasks such as hot work, confined space entry, electrical/isolation work, working at height, and excavation.).</summary>
public sealed record PermitToWorkItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PermitToWorkId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Permit Title.</summary>
    public required string PermitTitle { get; init; }

    /// <summary>Classification: HotWork, ConfinedSpaceEntry, ElectricalIsolation, WorkingAtHeight, Excavation.</summary>
    public required string PermitCategory { get; init; }

    /// <summary>Work Location.</summary>
    public required string WorkLocation { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string PermitStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}