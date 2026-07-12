// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the DataLake read-model.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.DataLake.Models;

namespace PlantSafety.Application.Intelligence.DataLake.Repositories;

/// <summary>Read-only repository for DataLake data. Implemented in the Infrastructure layer.</summary>
public interface IDataLakeRepository
{
    /// <summary>Returns DataLake items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<DataLakeItemDto>> GetDataLakeRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}