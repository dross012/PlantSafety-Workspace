// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Electrical read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Permits.Electrical.Models;

namespace PlantSafety.Application.Compliance.Permits.Electrical.Repositories;

/// <summary>Read-only repository for Electrical data. Implemented in the Infrastructure layer.</summary>
public interface IElectricalRepository
{
    /// <summary>Returns Electrical items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ElectricalItemDto>> GetElectricalAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
