// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the Integrations.Analyzers read-model.
// SchemaVersion: 1

using PlantSafety.Application.Integrations.Analyzers.Models;

namespace PlantSafety.Application.Integrations.Analyzers.Repositories;

/// <summary>Read-only repository for Integrations.Analyzers data. Implemented in the Infrastructure layer.</summary>
public interface IAnalyzersRepository
{
    /// <summary>Returns Integrations.Analyzers items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AnalyzersItemDto>> GetRunsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
