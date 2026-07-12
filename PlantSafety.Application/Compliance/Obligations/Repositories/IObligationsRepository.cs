// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Obligations read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Obligations.Models;

namespace PlantSafety.Application.Compliance.Obligations.Repositories;

/// <summary>Read-only repository for Obligations data. Implemented in the Infrastructure layer.</summary>
public interface IObligationsRepository
{
    /// <summary>Returns Obligations items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ObligationsItemDto>> GetObligationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
