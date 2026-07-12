// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the CAPA read-model.
// SchemaVersion: 1

using PlantSafety.Application.ContinuousImprovement.CAPA.Models;

namespace PlantSafety.Application.ContinuousImprovement.CAPA.Repositories;

/// <summary>Read-only repository for CAPA data. Implemented in the Infrastructure layer.</summary>
public interface ICAPARepository
{
    /// <summary>Returns CAPA items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CAPAItemDto>> GetActionsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
