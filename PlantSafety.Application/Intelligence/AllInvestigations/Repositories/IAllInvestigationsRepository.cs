// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Repository abstraction for the AllInvestigations read-model.
// SchemaVersion: 1
using PlantSafety.Application.Intelligence.AllInvestigations.Models;

namespace PlantSafety.Application.Intelligence.AllInvestigations.Repositories;

/// <summary>Read-only repository for AllInvestigations data. Implemented in the Infrastructure layer.</summary>
public interface IAllInvestigationsRepository
{
    /// <summary>Returns AllInvestigations items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<AllInvestigationsItemDto>> GetInvestigationsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}