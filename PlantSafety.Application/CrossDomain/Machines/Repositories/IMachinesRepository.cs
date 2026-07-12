// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Machines read-model.
// SchemaVersion: 1

using PlantSafety.Application.CrossDomain.Machines.Models;

namespace PlantSafety.Application.CrossDomain.Machines.Repositories;

/// <summary>Read-only repository for Machines data. Implemented in the Infrastructure layer.</summary>
public interface IMachinesRepository
{
    /// <summary>Returns Machines items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<MachinesItemDto>> GetMachinesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
