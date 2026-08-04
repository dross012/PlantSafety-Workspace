// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the SafetyZone occupancy read-model.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.SafetyZones.Models;

namespace PlantSafety.Application.PhysicalAccessControl.SafetyZones.Repositories;

/// <summary>Read-only repository for SafetyZone occupancy data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyZoneReadRepository
{
    /// <summary>Returns the occupancy projection for a single SafetyZone, scoped to the tenant, or null if not found.</summary>
    Task<SafetyZoneOccupancyDto?> GetOccupancyAsync(Guid tenantId, Guid safetyZoneId, CancellationToken cancellationToken);
}
