// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for the EvacuationRoute read-model.
// SchemaVersion: 1

using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Repositories;

/// <summary>Read-only repository for EvacuationRoute data. Implemented in the Infrastructure layer.</summary>
public interface IEvacuationRouteRepository
{
    /// <summary>Returns a single evacuation route by identity, or null if not found.</summary>
    Task<EvacuationRouteItemDto?> GetByIdAsync(Guid tenantId, Guid evacuationRouteId, CancellationToken cancellationToken);

    /// <summary>Returns evacuation routes belonging to the given zone.</summary>
    Task<IReadOnlyList<EvacuationRouteItemDto>> ListByZoneAsync(Guid tenantId, Guid zoneId, CancellationToken cancellationToken);

    /// <summary>Returns all currently open (uncleared) obstructions for the tenant/site.</summary>
    Task<IReadOnlyList<RouteObstructionItemDto>> ListActiveObstructionsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);
}
