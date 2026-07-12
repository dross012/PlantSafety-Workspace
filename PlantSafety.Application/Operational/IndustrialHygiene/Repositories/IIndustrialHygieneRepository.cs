// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the IndustrialHygiene read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.IndustrialHygiene.Models;

namespace PlantSafety.Application.Operational.IndustrialHygiene.Repositories;

/// <summary>Read-only repository for IndustrialHygiene data. Implemented in the Infrastructure layer.</summary>
public interface IIndustrialHygieneRepository
{
    /// <summary>Returns IndustrialHygiene items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<IndustrialHygieneItemDto>> GetIndustrialHygieneAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
