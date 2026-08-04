// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single RouteObstruction record.
// SchemaVersion: 1

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;

/// <summary>A single obstruction (open or cleared) recorded against an evacuation route.</summary>
public sealed record RouteObstructionItemDto
{
    /// <summary>Obstruction record identifier.</summary>
    public required Guid RouteObstructionId { get; init; }

    /// <summary>Identity of the parent evacuation route.</summary>
    public required Guid EvacuationRouteId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Description of the obstruction.</summary>
    public required string Description { get; init; }

    /// <summary>Severity classification.</summary>
    public required string Severity { get; init; }

    /// <summary>UTC timestamp the obstruction was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>UTC timestamp the obstruction was cleared, or null while still open.</summary>
    public DateTime? ClearedAtUtc { get; init; }
}
