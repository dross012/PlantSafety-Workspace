// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the AccessEvent read-model.
// SchemaVersion: 1

using PlantSafety.Application.PhysicalAccessControl.AccessEvents.Models;

namespace PlantSafety.Application.PhysicalAccessControl.AccessEvents.Repositories;

/// <summary>Read-only repository for AccessEvent data. Implemented in the Infrastructure layer.</summary>
public interface IAccessEventReadRepository
{
    /// <summary>Returns AccessEvent items for the given zone within an inclusive UTC date range.</summary>
    Task<IReadOnlyList<AccessEventDto>> ListByZoneAsync(
        Guid tenantId,
        Guid safetyZoneId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
