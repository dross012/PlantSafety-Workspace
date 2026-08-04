// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the EvacuationZone read-model. Implemented in the
//                    Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationZones.Repositories;

/// <summary>Read-only repository for EvacuationZone data.</summary>
public interface IEvacuationZoneRepository
{
    /// <summary>Returns a single EvacuationZone item by id, or null if not found.</summary>
    Task<EvacuationZoneItemDto?> GetByIdAsync(Guid tenantId, Guid evacuationZoneId, CancellationToken cancellationToken);

    /// <summary>Returns occupancy readings for the given zone within the inclusive UTC date range.</summary>
    Task<IReadOnlyList<OccupancyReadingItemDto>> GetOccupancyByZoneAsync(
        Guid tenantId, Guid? siteId, Guid zoneId, DateTime from, DateTime to, CancellationToken cancellationToken);
}
