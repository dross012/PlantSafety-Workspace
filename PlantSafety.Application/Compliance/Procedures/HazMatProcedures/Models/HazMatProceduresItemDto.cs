// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single HazMatProcedures record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Models;

/// <summary>A single HazMatProcedures read-model item.</summary>
public sealed record HazMatProceduresItemDto
{
    /// <summary>Procedure Id identifier.</summary>
    public required Guid ProcedureId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Hazardous Material.</summary>
    public required string HazardousMaterial { get; init; }

    /// <summary>Handling Step.</summary>
    public required string HandlingStep { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
