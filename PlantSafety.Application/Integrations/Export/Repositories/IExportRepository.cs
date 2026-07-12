// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Export read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Export.Models;

namespace PlantSafety.Application.Integrations.Export.Repositories;

/// <summary>Read-only repository for Integrations.Export data. Implemented in the Infrastructure layer.</summary>
public interface IExportRepository
{
    /// <summary>Returns Integrations.Export export run items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ExportItemDto>> GetExportRunsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
