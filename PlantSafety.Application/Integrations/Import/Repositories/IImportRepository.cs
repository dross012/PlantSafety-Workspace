// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Import read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Import.Models;

namespace PlantSafety.Application.Integrations.Import.Repositories;

/// <summary>Read-only repository for Integrations.Import data. Implemented in the Infrastructure layer.</summary>
public interface IImportRepository
{
    /// <summary>Returns Integrations.Import import run items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ImportItemDto>> GetImportRunsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
