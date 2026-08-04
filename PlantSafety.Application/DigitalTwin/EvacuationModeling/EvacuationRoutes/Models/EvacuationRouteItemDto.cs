// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single EvacuationRoute record
//                    ("DigitalTwin.EvacuationModeling.EvacuationRoutes"). The related
//                    RouteObstructionItemDto lives in RouteObstructionItemDto.cs.
// SchemaVersion: 1

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;

/// <summary>A single EvacuationRoute record (a modeled evacuation route validated against the Digital Twin spatial model).</summary>
public sealed record EvacuationRouteItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EvacuationRouteId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Route Name.</summary>
    public required string RouteName { get; init; }

    /// <summary>Identity of the zone this route originates from.</summary>
    public required Guid ZoneId { get; init; }

    /// <summary>Identity of the assembly point this route terminates at.</summary>
    public required Guid AssemblyPointId { get; init; }

    /// <summary>Reference to the validated Digital Twin spatial-model geometry backing this route.</summary>
    public required string PathGeometryRef { get; init; }

    /// <summary>Version of the Digital Twin model this route's geometry was sourced from.</summary>
    public required string TwinModelVersion { get; init; }

    /// <summary>Estimated egress time for this route under nominal conditions, in seconds.</summary>
    public required int EstimatedEgressTimeSeconds { get; init; }

    /// <summary>Cumulative count of obstruction events recorded against this route.</summary>
    public required int ObstructionCount { get; init; }

    /// <summary>Overall status classification: Active, Obstructed, Retired.</summary>
    public required string RouteStatus { get; init; }

    /// <summary>UTC timestamp when this record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }
}
