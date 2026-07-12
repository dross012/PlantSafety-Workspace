// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Financials read-model.
// SchemaVersion: 1

using PlantSafety.Application.Financials.Financials.Models;

namespace PlantSafety.Application.Financials.Financials.Repositories;

/// <summary>Read-only repository for Financials data. Implemented in the Infrastructure layer.</summary>
public interface IFinancialsRepository
{
    /// <summary>Returns Financials items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<FinancialsItemDto>> GetRecordsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
